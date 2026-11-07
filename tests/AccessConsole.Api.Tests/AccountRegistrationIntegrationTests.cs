/*
 * Access Console
 * Author : waifujahat
 * Module : Tests / Identity
 *
 * Verifies account registration using a dedicated PostgreSQL test database
 */

using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;
using Waifujahat.AccessConsole.Api.Infrastructure.Identity;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat
{
    namespace AccessConsole.Api.Tests
    {
        public sealed class AccountRegistrationIntegrationTests
        {
            [Fact]
            [Trait("Category", "Database")]
            public async Task RegistrasiMenyimpanAkunDanHashKataSandi()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var LayananRegistrasi = Lingkup.ServiceProvider
                    .GetRequiredService<AccountRegistrationService>();

                var PengelolaPengguna = Lingkup.ServiceProvider
                    .GetRequiredService<UserManager<AccessConsoleIdentityUser>>();

                var Email = $"registrasi-{Guid.NewGuid():N}@example.invalid";
                var KataSandi = "SandiKuat#2026";

                try
                {
                    var (Akun, Hasil) = await LayananRegistrasi.DaftarkanAsync(
                        Email,
                        "Pengguna Pengujian",
                        KataSandi);

                    Assert.True(
                        Hasil.Succeeded,
                        string.Join(", ", Hasil.Errors.Select(Error => Error.Description)));

                    Assert.NotNull(Akun);

                    var Profil = await Konteks.Accounts
                        .AsNoTracking()
                        .SingleAsync(Data => Data.Id == Akun.Id);

                    var Pengguna = await Konteks.Users
                        .AsNoTracking()
                        .SingleAsync(Data => Data.Id == Akun.Id);

                    Assert.Equal(Profil.Id, Pengguna.Id);
                    Assert.Equal(Email, Profil.Email);
                    Assert.NotNull(Pengguna.PasswordHash);
                    Assert.NotEqual(KataSandi, Pengguna.PasswordHash);

                    Assert.True(
                        await PengelolaPengguna.CheckPasswordAsync(
                            Pengguna,
                            KataSandi));
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task RegistrasiDenganKataSandiTidakValidDibatalkan()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var LayananRegistrasi = Lingkup.ServiceProvider
                    .GetRequiredService<AccountRegistrationService>();

                var Email = $"registrasi-gagal-{Guid.NewGuid():N}@example.invalid";

                try
                {
                    var (Akun, Hasil) = await LayananRegistrasi.DaftarkanAsync(
                        Email,
                        "Pengguna Pengujian",
                        "abc");

                    Assert.Null(Akun);
                    Assert.False(Hasil.Succeeded);
                    Assert.NotEmpty(Hasil.Errors);

                    var ProfilTersimpan = await Konteks.Accounts
                        .AsNoTracking()
                        .AnyAsync(Data => Data.Email == Email);

                    var PenggunaTersimpan = await Konteks.Users
                        .AsNoTracking()
                        .AnyAsync(Data => Data.Email == Email);

                    Assert.False(ProfilTersimpan);
                    Assert.False(PenggunaTersimpan);
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task EndpointRegistrasiMembuatAkunMelaluiHttp()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var Email = $"registrasi-http-{Guid.NewGuid():N}@example.invalid";
                var KataSandi = "SandiKuat#2026";

                try
                {
                    using var Klien = Aplikasi.CreateClient();

                    using var Respons = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email,
                            DisplayName = "Pengguna HTTP",
                            KataSandi
                        });

                    Assert.Equal(HttpStatusCode.Created, Respons.StatusCode);

                    var IsiRespons = await Respons.Content.ReadAsStringAsync();

                    Assert.DoesNotContain(KataSandi, IsiRespons);

                    var Akun = await Konteks.Accounts
                        .AsNoTracking()
                        .SingleAsync(Data => Data.Email == Email);

                    var Pengguna = await Konteks.Users
                        .AsNoTracking()
                        .SingleAsync(Data => Data.Id == Akun.Id);

                    Assert.Equal(Akun.Id, Pengguna.Id);
                    Assert.NotNull(Pengguna.PasswordHash);
                    Assert.NotEqual(KataSandi, Pengguna.PasswordHash);
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task EndpointRegistrasiMenolakEmailDuplikat()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var Email = $"duplikat-http-{Guid.NewGuid():N}@example.invalid";

                using var Klien = Aplikasi.CreateClient();

                try
                {
                    using var ResponsPertama = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email,
                            DisplayName = "Pengguna Pertama",
                            KataSandi = "SandiKuat#2026"
                        });

                    Assert.Equal(
                        HttpStatusCode.Created,
                        ResponsPertama.StatusCode);

                    using var ResponsKedua = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email = Email.ToUpperInvariant(),
                            DisplayName = "Pengguna Kedua",
                            KataSandi = "SandiKuat#2026"
                        });

                    Assert.Equal(
                        HttpStatusCode.Conflict,
                        ResponsKedua.StatusCode);

                    var JumlahProfil = await Konteks.Accounts
                        .AsNoTracking()
                        .CountAsync(Data => Data.Email == Email);

                    var JumlahPengguna = await Konteks.Users
                        .AsNoTracking()
                        .CountAsync(Data => Data.Email == Email);

                    Assert.Equal(1, JumlahProfil);
                    Assert.Equal(1, JumlahPengguna);
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task EndpointRegistrasiMenolakKataSandiTidakValid()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var Email = $"kata-sandi-gagal-{Guid.NewGuid():N}@example.invalid";

                using var Klien = Aplikasi.CreateClient();

                try
                {
                    using var Respons = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email,
                            DisplayName = "Pengguna Pengujian",
                            KataSandi = "abc"
                        });

                    Assert.Equal(
                        HttpStatusCode.BadRequest,
                        Respons.StatusCode);

                    var JumlahProfil = await Konteks.Accounts
                        .AsNoTracking()
                        .CountAsync(Data => Data.Email == Email);

                    var JumlahPengguna = await Konteks.Users
                        .AsNoTracking()
                        .CountAsync(Data => Data.Email == Email);

                    Assert.Equal(0, JumlahProfil);
                    Assert.Equal(0, JumlahPengguna);
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task EndpointLoginBerhasilMengembalikanBearerToken()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var Email = $"login-http-{Guid.NewGuid():N}@example.invalid";
                var KataSandi = "SandiKuat#2026";

                using var Klien = Aplikasi.CreateClient();

                try
                {
                    using var ResponsRegistrasi = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email,
                            DisplayName = "Pengguna Login",
                            KataSandi
                        });

                    Assert.Equal(
                        HttpStatusCode.Created,
                        ResponsRegistrasi.StatusCode);

                    using var ResponsLogin = await Klien.PostAsJsonAsync(
                        "/api/auth/login",
                        new
                        {
                            Email,
                            KataSandi
                        });

                    Assert.Equal(
                        HttpStatusCode.OK,
                        ResponsLogin.StatusCode);

                    using var IsiLogin = JsonDocument.Parse(
                        await ResponsLogin.Content.ReadAsStringAsync());

                    var Akar = IsiLogin.RootElement;

                    Assert.Equal(
                        "Bearer",
                        Akar.GetProperty("tokenType").GetString());

                    var TokenAkses = Akar.GetProperty("accessToken").GetString();

                    Assert.False(string.IsNullOrWhiteSpace(TokenAkses));
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task EndpointLoginMenolakKataSandiSalah()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var Email = $"login-gagal-{Guid.NewGuid():N}@example.invalid";

                using var Klien = Aplikasi.CreateClient();

                try
                {
                    using var ResponsRegistrasi = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email,
                            DisplayName = "Pengguna Login Gagal",
                            KataSandi = "SandiKuat#2026"
                        });

                    Assert.Equal(
                        HttpStatusCode.Created,
                        ResponsRegistrasi.StatusCode);

                    using var ResponsLogin = await Klien.PostAsJsonAsync(
                        "/api/auth/login",
                        new
                        {
                            Email,
                            KataSandi = "SandiSalah#2026"
                        });

                    Assert.Equal(
                        HttpStatusCode.Unauthorized,
                        ResponsLogin.StatusCode);

                    var IsiRespons = await ResponsLogin.Content.ReadAsStringAsync();

                    Assert.DoesNotContain("accessToken", IsiRespons);

                    var Pengguna = await Konteks.Users
                        .AsNoTracking()
                        .SingleAsync(Data => Data.Email == Email);

                    Assert.Equal(1, Pengguna.AccessFailedCount);
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task EndpointLoginMengunciAkunSetelahLimaKegagalan()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var Email = $"akun-terkunci-{Guid.NewGuid():N}@example.invalid";
                var KataSandi = "SandiKuat#2026";

                using var Klien = Aplikasi.CreateClient();

                try
                {
                    using var ResponsRegistrasi = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email,
                            DisplayName = "Pengguna Pengujian Lockout",
                            KataSandi
                        });

                    Assert.Equal(
                        HttpStatusCode.Created,
                        ResponsRegistrasi.StatusCode);

                    for (var Percobaan = 1; Percobaan <= 5; Percobaan++)
                    {
                        using var ResponsGagal = await Klien.PostAsJsonAsync(
                            "/api/auth/login",
                            new
                            {
                                Email,
                                KataSandi = "SandiSalah#2026"
                            });

                        Assert.Equal(
                            HttpStatusCode.Unauthorized,
                            ResponsGagal.StatusCode);
                    }

                    var Pengguna = await Konteks.Users
                        .AsNoTracking()
                        .SingleAsync(Data => Data.Email == Email);

                    Assert.True(Pengguna.LockoutEnabled);
                    Assert.True(Pengguna.LockoutEnd.HasValue);
                    Assert.True(Pengguna.LockoutEnd.Value > DateTimeOffset.UtcNow);

                    using var ResponsSetelahTerkunci = await Klien.PostAsJsonAsync(
                        "/api/auth/login",
                        new
                        {
                            Email,
                            KataSandi
                        });

                    Assert.Equal(
                        HttpStatusCode.Unauthorized,
                        ResponsSetelahTerkunci.StatusCode);

                    var IsiRespons = await ResponsSetelahTerkunci.Content
                        .ReadAsStringAsync();

                    Assert.DoesNotContain("accessToken", IsiRespons);
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun => Akun.Email == Email)
                        .ExecuteDeleteAsync();
                }
            }

            [Fact]
            [Trait("Category", "Database")]
            public async Task EndpointAkunHanyaDapatDiaksesOlehPemilik()
            {
                var KoneksiUji = Environment.GetEnvironmentVariable(
                    "ACCESS_CONSOLE_TEST_CONNECTION_STRING");

                if (string.IsNullOrWhiteSpace(KoneksiUji))
                {
                    throw new InvalidOperationException(
                        "Connection string database pengujian belum tersedia");
                }

                var PengaturanKoneksi = new NpgsqlConnectionStringBuilder(KoneksiUji);

                if (PengaturanKoneksi.Database != "access_console_test")
                {
                    throw new InvalidOperationException(
                        "Pengujian hanya boleh menggunakan access_console_test");
                }

                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureTestServices(Layanan =>
                        {
                            Layanan.RemoveAll<
                                IDbContextOptionsConfiguration<AccessConsoleDbContext>>();

                            Layanan.AddDbContext<AccessConsoleDbContext>(
                                Opsi => Opsi.UseNpgsql(KoneksiUji));
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var Konteks = Lingkup.ServiceProvider
                    .GetRequiredService<AccessConsoleDbContext>();

                Assert.Equal(
                    "access_console_test",
                    Konteks.Database.GetDbConnection().Database);

                var EmailPertama = $"pemilik-{Guid.NewGuid():N}@example.invalid";
                var EmailKedua = $"pengguna-lain-{Guid.NewGuid():N}@example.invalid";
                var KataSandi = "SandiKuat#2026";

                using var Klien = Aplikasi.CreateClient();

                try
                {
                    using var RegistrasiPertama = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email = EmailPertama,
                            DisplayName = "Pemilik Akun",
                            KataSandi
                        });

                    Assert.Equal(
                        HttpStatusCode.Created,
                        RegistrasiPertama.StatusCode);

                    using var RegistrasiKedua = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        new
                        {
                            Email = EmailKedua,
                            DisplayName = "Pengguna Lain",
                            KataSandi
                        });

                    Assert.Equal(
                        HttpStatusCode.Created,
                        RegistrasiKedua.StatusCode);

                    var AkunPertama = await Konteks.Accounts
                        .AsNoTracking()
                        .SingleAsync(Akun => Akun.Email == EmailPertama);

                    var AkunKedua = await Konteks.Accounts
                        .AsNoTracking()
                        .SingleAsync(Akun => Akun.Email == EmailKedua);

                    using var ResponsLogin = await Klien.PostAsJsonAsync(
                        "/api/auth/login",
                        new
                        {
                            Email = EmailPertama,
                            KataSandi
                        });

                    Assert.Equal(
                        HttpStatusCode.OK,
                        ResponsLogin.StatusCode);

                    using var IsiLogin = JsonDocument.Parse(
                        await ResponsLogin.Content.ReadAsStringAsync());

                    var TokenAkses = IsiLogin.RootElement
                        .GetProperty("accessToken")
                        .GetString();

                    Assert.False(string.IsNullOrWhiteSpace(TokenAkses));

                    Klien.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue(
                            "Bearer",
                            TokenAkses);

                    using var ResponsMilikSendiri = await Klien.GetAsync(
                        $"/api/accounts/{AkunPertama.Id}");

                    Assert.Equal(
                        HttpStatusCode.OK,
                        ResponsMilikSendiri.StatusCode);

                    using var IsiAkun = JsonDocument.Parse(
                        await ResponsMilikSendiri.Content.ReadAsStringAsync());

                    Assert.Equal(
                        AkunPertama.Id,
                        IsiAkun.RootElement.GetProperty("id").GetGuid());

                    using var ResponsAkunLain = await Klien.GetAsync(
                        $"/api/accounts/{AkunKedua.Id}");

                    Assert.Equal(
                        HttpStatusCode.Forbidden,
                        ResponsAkunLain.StatusCode);

                    await Konteks.Accounts
                        .Where(Akun => Akun.Id == AkunPertama.Id)
                        .ExecuteDeleteAsync();

                    using var ResponsAkunTidakDitemukan = await Klien.GetAsync(
                        $"/api/accounts/{AkunPertama.Id}");

                    Assert.Equal(
                        HttpStatusCode.NotFound,
                        ResponsAkunTidakDitemukan.StatusCode);
                }
                finally
                {
                    await Konteks.Accounts
                        .Where(Akun =>
                            Akun.Email == EmailPertama ||
                            Akun.Email == EmailKedua)
                        .ExecuteDeleteAsync();

                    var PengelolaIdentitas = Lingkup.ServiceProvider
                        .GetRequiredService<UserManager<AccessConsoleIdentityUser>>();

                    foreach (var Email in new[] { EmailPertama, EmailKedua })
                    {
                        var Pengguna = await PengelolaIdentitas.FindByEmailAsync(Email);

                        if (Pengguna is null)
                        {
                            continue;
                        }

                        var HasilPenghapusan = await PengelolaIdentitas.DeleteAsync(Pengguna);

                        Assert.True(
                            HasilPenghapusan.Succeeded,
                            string.Join(
                                ", ",
                                HasilPenghapusan.Errors.Select(
                                    Kesalahan => Kesalahan.Description)));
                    }
                }
            }
        }
    }
}
