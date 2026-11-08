/*
 * Access Console
 * Author : waifujahat
 * Module : API / Application
 *
 * Configures application, health checks and account management endpoints
 */

using System.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Contracts.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Identity;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

var Builder = WebApplication.CreateBuilder(args);

var KoneksiDatabase = Builder.Configuration.GetConnectionString("AccessConsole");

if (string.IsNullOrWhiteSpace(KoneksiDatabase))
{
    throw new InvalidOperationException(
        "Connection string AccessConsole belum dikonfigurasi");
}

Builder.Services.AddHealthChecks();

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

Builder.Services.AddRateLimiter(Opsi =>
{
    Opsi.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    Opsi.AddPolicy("registrasi", Konteks =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: Konteks.Connection.RemoteIpAddress?.ToString()
                ?? "tidak-diketahui",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    Opsi.AddPolicy("login", Konteks =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: Konteks.Connection.RemoteIpAddress?.ToString()
                ?? "tidak-diketahui",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

Builder.Services.AddAuthentication(IdentityConstants.BearerScheme)
    .AddBearerToken(IdentityConstants.BearerScheme);

Builder.Services.AddAuthorization();

Builder.Services.AddDbContext<AccessConsoleDbContext>(
    Opsi => Opsi.UseNpgsql(KoneksiDatabase));

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
.AddSignInManager();

Builder.Services.AddScoped<IAccountRepository, PostgresAccountRepository>();

Builder.Services.AddScoped<AccountRegistrationService>();

var App = Builder.Build();

if (!string.IsNullOrWhiteSpace(AlamatProksi))
{
    App.UseForwardedHeaders();
}

App.UseRouting();
App.UseRateLimiter();
App.UseAuthentication();
App.UseAuthorization();

App.MapHealthChecks("/health");

App.MapPost("/api/accounts", () =>
{
    return Results.StatusCode(StatusCodes.Status410Gone);
});

App.MapGet("/api/accounts/{Id:guid}", async (
    Guid Id,
    HttpContext KonteksHttp,
    UserManager<AccessConsoleIdentityUser> PengelolaPengguna,
    IAccountRepository Repositori,
    CancellationToken TokenPembatalan) =>
{
    var IdPengguna = PengelolaPengguna.GetUserId(KonteksHttp.User);

    if (!Guid.TryParse(IdPengguna, out var IdPemilik) ||
        IdPemilik != Id)
    {
        return Results.Forbid();
    }

    var Akun = await Repositori.GetByIdAsync(
        Id,
        TokenPembatalan);

    if (Akun is null)
    {
        return Results.NotFound();
    }

    var Respons = new AccountResponse(
        Akun.Id,
        Akun.Email,
        Akun.DisplayName,
        Akun.CreatedatUTC);

    return Results.Ok(Respons);
}).RequireAuthorization();

App.MapGet("/api/accounts/me", async (
    HttpContext KonteksHttp,
    UserManager<AccessConsoleIdentityUser> PengelolaPengguna,
    IAccountRepository Repositori,
    CancellationToken TokenPembatalan) =>
{
    var IdPengguna = PengelolaPengguna.GetUserId(
        KonteksHttp.User);

    if (!Guid.TryParse(IdPengguna, out var IdAkun))
    {
        return Results.Unauthorized();
    }

    var Akun = await Repositori.GetByIdAsync(
        IdAkun,
        TokenPembatalan);

    if (Akun is null)
    {
        return Results.NotFound();
    }

    var Respons = new AccountResponse(
        Akun.Id,
        Akun.Email,
        Akun.DisplayName,
        Akun.CreatedatUTC);

    return Results.Ok(Respons);
}).RequireAuthorization();

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
    SignInManager<AccessConsoleIdentityUser> PengelolaMasuk) =>
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

    PengelolaMasuk.AuthenticationScheme =
        IdentityConstants.BearerScheme;

    var Hasil = await PengelolaMasuk.PasswordSignInAsync(
        Permintaan.Email,
        Permintaan.KataSandi,
        isPersistent: false,
        lockoutOnFailure: true);

    if (!Hasil.Succeeded)
    {
        return Results.Unauthorized();
    }

    return TypedResults.Empty;
}).RequireRateLimiting("login");

App.Run();

public partial class Program // Exposes application entry point for integration testing
{
}
