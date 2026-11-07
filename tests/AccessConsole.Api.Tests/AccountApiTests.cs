/*
 * Access Console
 * Author : waifujahat
 * Module : Tests / Accounts
 *
 * Verifies account API responses using an isolated test server
 */

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Npgsql;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Contracts.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat
{
    namespace AccessConsole.Api.Tests
    {
        public sealed class AccountApiTests
        {
            [Fact]
            public async Task MembuatDanMengambilAkunMengembalikanCreatedDanOk()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                var Permintaan = new
                {
                    Email = "akun-baru@example.invalid",
                    DisplayName = "Akun Baru"
                };

                using var ResponsPembuatan = await Klien.PostAsJsonAsync(
                    "/api/accounts",
                    Permintaan);

                Assert.Equal(
                    HttpStatusCode.Created,
                    ResponsPembuatan.StatusCode);

                var Akun = await ResponsPembuatan.Content
                    .ReadFromJsonAsync<AccountResponse>();

                Assert.NotNull(Akun);
                Assert.Equal(Permintaan.Email, Akun.Email);
                Assert.Equal(Permintaan.DisplayName, Akun.DisplayName);

                Assert.Equal(
                    $"/api/accounts/{Akun.Id}",
                    ResponsPembuatan.Headers.Location?.OriginalString);

                using var ResponsPencarian = await Klien.GetAsync(
                    $"/api/accounts/{Akun.Id}");

                Assert.Equal(
                    HttpStatusCode.OK,
                    ResponsPencarian.StatusCode);

                var AkunDitemukan = await ResponsPencarian.Content
                    .ReadFromJsonAsync<AccountResponse>();

                Assert.NotNull(AkunDitemukan);
                Assert.Equal(Akun, AkunDitemukan);
            }

            [Fact]
            public async Task EmailTidakValidMengembalikanBadRequest()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                var Permintaan = new
                {
                    Email = "email-tidak-valid",
                    DisplayName = "Akun Pengujian"
                };

                using var Respons = await Klien.PostAsJsonAsync(
                    "/api/accounts",
                    Permintaan);

                Assert.Equal(
                    HttpStatusCode.BadRequest,
                    Respons.StatusCode);
            }

            [Fact]
            public async Task AkunTidakDitemukanMengembalikanNotFound()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                var Id = Guid.NewGuid();

                using var Respons = await Klien.GetAsync(
                    $"/api/accounts/{Id}");

                Assert.Equal(
                    HttpStatusCode.NotFound,
                    Respons.StatusCode);
            }

            [Fact]
            public async Task EmailDuplikatMengembalikanConflict()
            {
                using var Pabrik = new PabrikApiPengujian(true);
                using var Klien = Pabrik.CreateClient();

                var Permintaan = new
                {
                    Email = "duplikat@example.invalid",
                    DisplayName = "Akun Duplikat"
                };

                using var Respons = await Klien.PostAsJsonAsync(
                    "/api/accounts",
                    Permintaan);

                Assert.Equal(
                    HttpStatusCode.Conflict,
                    Respons.StatusCode);

                var Isi = await Respons.Content
                    .ReadFromJsonAsync<Dictionary<string, string>>();

                Assert.NotNull(Isi);
                Assert.Equal(
                    "Email sudah terdaftar",
                    Isi["pesan"]);
            }

            private sealed class PabrikApiPengujian
                : WebApplicationFactory<Program>
            {
                private readonly bool SimulasikanDuplikat;

                public PabrikApiPengujian(bool SimulasikanDuplikat = false)
                {
                    this.SimulasikanDuplikat = SimulasikanDuplikat;
                }

                protected override IHost CreateHost(IHostBuilder Builder)
                {
                    Builder.ConfigureHostConfiguration(Konfigurasi =>
                    {
                        Konfigurasi.AddInMemoryCollection(
                            new Dictionary<string, string?>
                            {
                                ["ConnectionStrings:AccessConsole"] =
                                    "Host=127.0.0.1;Database=access_console_test;Username=test;Password=unused"
                            });
                    });

                    return base.CreateHost(Builder);
                }

                protected override void ConfigureWebHost(
                    IWebHostBuilder Builder)
                {
                    Builder.ConfigureTestServices(Layanan =>
                    {
                        Layanan.RemoveAll<IAccountRepository>();

                        if (SimulasikanDuplikat)
                        {
                            Layanan.AddScoped<
                                IAccountRepository,
                                RepositoriEmailDuplikat>();
                        }
                        else
                        {
                            Layanan.AddSingleton<
                                IAccountRepository,
                                InMemoryAccountRepository>();
                        }
                    });
                }
            }

            private sealed class RepositoriEmailDuplikat
                : IAccountRepository
            {
                public Task AddAsync(
                    Account Akun,
                    CancellationToken TokenPembatalan = default)
                {
                    var ErrorPostgres = new PostgresException(
                        "Email sudah terdaftar",
                        "ERROR",
                        "ERROR",
                        PostgresErrorCodes.UniqueViolation,
                        constraintName: "IX_accounts_email");

                    throw new DbUpdateException(
                        "Gagal menyimpan akun",
                        ErrorPostgres);
                }

                public Task<Account?> GetByIdAsync(
                    Guid Id,
                    CancellationToken TokenPembatalan = default)
                {
                    return Task.FromResult<Account?>(null);
                }
            }
        }
    }
}
