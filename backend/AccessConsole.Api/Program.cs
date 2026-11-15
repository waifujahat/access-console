/*
 * Access Console
 * Author : waifujahat
 * Module : API / Application
 *
 * Configures application, health checks and account management endpoints
 */

using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Contracts.Accounts;
using Waifujahat.AccessConsole.Api.Endpoints;
using Waifujahat.AccessConsole.Api.Infrastructure.Authentication;
using Waifujahat.AccessConsole.Api.Infrastructure.Configuration;
using Waifujahat.AccessConsole.Api.Infrastructure.Identity;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

var Builder = WebApplication.CreateBuilder(args);

var KoneksiDatabase = Builder.Configuration.GetConnectionString("AccessConsole");

if (string.IsNullOrWhiteSpace(KoneksiDatabase))
{
    throw new InvalidOperationException(
        "Connection string AccessConsole belum dikonfigurasi");
}

var AlamatProksi = Builder.Configuration["ReverseProxy:TrustedProxy"];

if (!string.IsNullOrWhiteSpace(AlamatProksi))
{
    if (!IPAddress.TryParse(AlamatProksi, out var IpProksi))
    {
        throw new InvalidOperationException(
            "Alamat reverse proxy tidak valid");
    }

    Builder.Services.Configure<ForwardedHeadersOptions>(Opsi =>
    {
        Opsi.ForwardedHeaders =
            ForwardedHeaders.XForwardedFor |
            ForwardedHeaders.XForwardedProto;

        Opsi.ForwardLimit = 1;

        Opsi.KnownProxies.Clear();
        Opsi.KnownIPNetworks.Clear();

        Opsi.KnownProxies.Add(IpProksi);
    });
}

Builder.Services.TambahkanPembatasPermintaan();

var AlamatCasdoor = Builder.Configuration["CASDOOR_ISSUER"]?.Trim();
var ClientIdCasdoor = Builder.Configuration["CASDOOR_CLIENT_ID"]?.Trim();

var CasdoorAktif =
    !string.IsNullOrWhiteSpace(AlamatCasdoor) &&
    !string.IsNullOrWhiteSpace(ClientIdCasdoor);

if (!CasdoorAktif &&
    (!string.IsNullOrWhiteSpace(AlamatCasdoor) ||
     !string.IsNullOrWhiteSpace(ClientIdCasdoor)))
{
    throw new InvalidOperationException(
        "Konfigurasi Casdoor belum lengkap");
}

var Autentikasi = Builder.Services
    .AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

if (CasdoorAktif)
{
    if (!Uri.TryCreate(
        AlamatCasdoor,
        UriKind.Absolute,
        out var UriCasdoor))
    {
        throw new InvalidOperationException(
            "Alamat Casdoor tidak valid");
    }

    var MenggunakanHttps =
        UriCasdoor.Scheme == Uri.UriSchemeHttps;

    var HttpLokal =
        Builder.Environment.IsDevelopment() &&
        UriCasdoor.IsLoopback &&
        UriCasdoor.Scheme == Uri.UriSchemeHttp;

    if (!MenggunakanHttps && !HttpLokal)
    {
        throw new InvalidOperationException(
            "Casdoor harus menggunakan HTTPS di luar Development lokal");
    }

    Autentikasi.AddJwtBearer("CasdoorOIDC", Opsi =>
    {
        Opsi.Authority = AlamatCasdoor!.TrimEnd('/');
        Opsi.Audience = ClientIdCasdoor;
        Opsi.RequireHttpsMetadata = MenggunakanHttps;
        Opsi.MapInboundClaims = false;
        Opsi.SaveToken = true;
        Opsi.Events = new JwtBearerEvents
        {
            OnMessageReceived = Konteks =>
            {
                if (Konteks.Request.Path == "/api/auth/casdoor/link")
                {
                    var TokenCasdoor = Konteks.Request.Headers
                        ["X-Casdoor-Token"].ToString();

                    if (!string.IsNullOrWhiteSpace(TokenCasdoor))
                    {
                        Konteks.Token = TokenCasdoor;
                    }
                }

                return Task.CompletedTask;
            },
            OnTokenValidated = Konteks =>
            {
                var JenisToken = Konteks.Principal?
                    .FindFirst("tokenType")?.Value;

                var IdPengguna = Konteks.Principal?
                    .FindFirst("sub")?.Value;

                if (JenisToken != "access-token" ||
                    string.IsNullOrWhiteSpace(IdPengguna))
                {
                    Konteks.Fail("Identitas Casdoor tidak valid");
                }

                return Task.CompletedTask;
            }
        };
    });
}

Builder.Services.AddAuthorization(Opsi =>
{
    if (CasdoorAktif)
    {
        Opsi.AddPolicy("CasdoorOIDC", Kebijakan =>
        {
            Kebijakan
                .AddAuthenticationSchemes("CasdoorOIDC")
                .RequireAuthenticatedUser();
        });
    }
});

Builder.Services.AddDbContext<AccessConsoleDbContext>(
    Opsi => Opsi.UseNpgsql(KoneksiDatabase));

var ProteksiData = Builder.Services.AddDataProtection()
    .SetApplicationName("Waifujahat.AccessConsole.Api")
    .PersistKeysToDbContext<AccessConsoleDbContext>();

var SertifikatBase64 = Builder.Configuration["DataProtection:CertificateBase64"];
var SandiSertifikat = Builder.Configuration["DataProtection:CertificatePassword"];

if (!string.IsNullOrWhiteSpace(SertifikatBase64) &&
    !string.IsNullOrWhiteSpace(SandiSertifikat))
{
    var Sertifikat = X509CertificateLoader.LoadPkcs12(
        Convert.FromBase64String(SertifikatBase64),
        SandiSertifikat,
        X509KeyStorageFlags.EphemeralKeySet);

    if (!Sertifikat.HasPrivateKey)
    {
        throw new InvalidOperationException(
            "Sertifikat Data Protection harus memiliki private key");
    }

    ProteksiData.ProtectKeysWithCertificate(Sertifikat);
}
else if (!Builder.Environment.IsDevelopment())
{
    throw new InvalidOperationException(
        "Sertifikat Data Protection wajib dikonfigurasi di luar Development");
}

Builder.Services.AddHealthChecks()
    .AddDbContextCheck<AccessConsoleDbContext>(
        name: "postgresql",
        tags: ["ready"]);

Builder.Services.AddIdentityCore<AccessConsoleIdentityUser>(Opsi =>
{
    Opsi.User.RequireUniqueEmail = true;

    Opsi.Password.RequiredLength = 6;
    Opsi.Password.RequireDigit = true;
    Opsi.Password.RequireLowercase = true;
    Opsi.Password.RequireUppercase = true;
    Opsi.Password.RequireNonAlphanumeric = true;

    Opsi.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    Opsi.Lockout.MaxFailedAccessAttempts = 5;
    Opsi.Lockout.AllowedForNewUsers = true;
})
.AddRoles<IdentityRole<Guid>>()
.AddEntityFrameworkStores<AccessConsoleDbContext>()
.AddSignInManager<AccessConsoleSignInManager>();

Builder.Services.AddScoped<IAccountRepository, PostgresAccountRepository>();

Builder.Services.AddScoped<AccountRegistrationService>();

Builder.Services.AddScoped<CasdoorAccountService>();

Builder.Services.AddHttpClient<CasdoorProviderStatusService>(Klien =>
{
    Klien.Timeout = TimeSpan.FromSeconds(4);
}).ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    AllowAutoRedirect = false,
    UseCookies = false
});

Builder.Services.AddScoped<AccessTokenRevocationService>();

var App = Builder.Build();

if (!string.IsNullOrWhiteSpace(AlamatProksi))
{
    App.UseForwardedHeaders();
}

App.Use(async (Konteks, Berikutnya) =>
{
    if (Konteks.Request.Path.StartsWithSegments("/api/accounts") ||
        Konteks.Request.Path.StartsWithSegments("/api/auth"))
    {
        Konteks.Response.Headers.CacheControl = "no-store";
    }

    await Berikutnya();
});

App.UseRouting();
App.UseRateLimiter();
App.UseAuthentication();

App.Use(async (Konteks, Berikutnya) =>
{
    if (Konteks.User.Identity?.IsAuthenticated == true)
    {
        var Otorisasi = Konteks.Request.Headers.Authorization.ToString();

        if (Otorisasi.StartsWith(
            "Bearer ",
            StringComparison.OrdinalIgnoreCase))
        {
            var Token = Otorisasi["Bearer ".Length..].Trim();

            if (!string.IsNullOrWhiteSpace(Token))
            {
                var Layanan = Konteks.RequestServices
                    .GetRequiredService<AccessTokenRevocationService>();

                var SudahDicabut = await Layanan.SudahDicabutAsync(
                    Token,
                    Konteks.RequestAborted);

                if (SudahDicabut)
                {
                    Konteks.Response.StatusCode =
                        StatusCodes.Status401Unauthorized;

                    return;
                }
            }
        }
    }

    await Berikutnya();
});

App.UseAuthorization();

App.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => false
});

App.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = Pemeriksaan =>
        Pemeriksaan.Tags.Contains("ready")
});

App.MapAccountEndpoints();

if (CasdoorAktif)
{
    App.MapCasdoorAuthenticationEndpoints();
}

App.MapPost("/api/auth/register", async (
    RegisterAccountRequest Permintaan,
    AccountRegistrationService LayananRegistrasi,
    CancellationToken TokenPembatalan) =>
{
    if (string.IsNullOrWhiteSpace(Permintaan.KataSandi))
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["kataSandi"] = ["Kata sandi wajib diisi"]
            });
    }

    try
    {
        var (Akun, Hasil) = await LayananRegistrasi.DaftarkanAsync(
            Permintaan.Email,
            Permintaan.DisplayName,
            Permintaan.KataSandi,
            TokenPembatalan);

        if (!Hasil.Succeeded)
        {
            var Kesalahan = Hasil.Errors
                .GroupBy(Error =>
                    Error.Code.StartsWith("Password", StringComparison.Ordinal)
                        ? "kataSandi"
                        : "registrasi")
                .ToDictionary(
                    Grup => Grup.Key,
                    Grup => Grup.Select(Error => Error.Description).ToArray());

            return Results.ValidationProblem(Kesalahan);
        }

        if (Akun is null)
        {
            return Results.Problem(
                "Registrasi tidak dapat diselesaikan",
                statusCode: StatusCodes.Status500InternalServerError);
        }

        return Results.Created(
            $"/api/accounts/{Akun.Id}",
            new
            {
                Akun.Id,
                Akun.Email,
                Akun.DisplayName
            });
    }
    catch (ArgumentException Error)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                [Error.ParamName ?? "registrasi"] = [Error.Message]
            });
    }
    catch (DbUpdateException Error) when (
        Error.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_accounts_email"
        })
    {
        return Results.Conflict(
            new { Kesalahan = "Email sudah terdaftar" });
    }
}).RequireRateLimiting("registrasi");


App.MapPost("/api/auth/login", async (
    LoginAccountRequest Permintaan,
    SignInManager<AccessConsoleIdentityUser> PengelolaMasuk,
    PartitionedRateLimiter<Guid> PembatasAkun,
    CancellationToken TokenPembatalan) =>
{
    if (string.IsNullOrWhiteSpace(Permintaan.Email) ||
        string.IsNullOrWhiteSpace(Permintaan.KataSandi))
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                ["login"] = ["Email dan kata sandi wajib diisi"]
            });
    }

    var Email = Permintaan.Email.Trim();

    var Pengguna = await PengelolaMasuk.UserManager
        .FindByNameAsync(Email);

    if (Pengguna is not null)
    {
        using var Izin = await PembatasAkun.AcquireAsync(
            Pengguna.Id,
            permitCount: 1,
            cancellationToken: TokenPembatalan);

        if (!Izin.IsAcquired)
        {
            return Results.StatusCode(
                StatusCodes.Status429TooManyRequests);
        }
    }

    PengelolaMasuk.AuthenticationScheme =
        IdentityConstants.BearerScheme;

    var Hasil = await PengelolaMasuk.PasswordSignInAsync(
        Email,
        Permintaan.KataSandi,
        isPersistent: false,
        lockoutOnFailure: true);

    if (!Hasil.Succeeded)
    {
        return Results.Unauthorized();
    }

    return TypedResults.Empty;
}).RequireRateLimiting("login");

App.MapPost("/api/auth/logout", async (
    HttpContext KonteksHttp,
    AccessTokenRevocationService LayananPencabutan,
    CancellationToken TokenPembatalan) =>
{
    var HasilAutentikasi = await KonteksHttp.AuthenticateAsync(
        IdentityConstants.BearerScheme);

    if (!HasilAutentikasi.Succeeded ||
        HasilAutentikasi.Properties?.ExpiresUtc is not
        { } WaktuKedaluwarsa)
    {
        return Results.Unauthorized();
    }

    var Otorisasi = KonteksHttp.Request.Headers.Authorization.ToString();

    if (!Otorisasi.StartsWith(
        "Bearer ",
        StringComparison.OrdinalIgnoreCase))
    {
        return Results.Unauthorized();
    }

    var Token = Otorisasi["Bearer ".Length..].Trim();

    if (string.IsNullOrWhiteSpace(Token))
    {
        return Results.Unauthorized();
    }

    await LayananPencabutan.CabutAsync(
        Token,
        WaktuKedaluwarsa.ToUniversalTime(),
        TokenPembatalan);

    return Results.NoContent();
}).RequireAuthorization();

App.Run();

public partial class Program // Exposes application entry point for integration testing
{
}
