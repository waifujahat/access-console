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
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat
{
    namespace AccessConsole.Api.Tests
    {
        public sealed class AccountApiTests
        {
            [Fact]
            public async Task EndpointPembuatanAkunLamaMengembalikanGone()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                var Permintaan = new
                {
                    Email = "akun-baru@example.invalid",
                    DisplayName = "Akun Baru"
                };

                using var Respons = await Klien.PostAsJsonAsync(
                    "/api/accounts",
                    Permintaan);

                Assert.Equal(
                    HttpStatusCode.Gone,
                    Respons.StatusCode);
            }

            [Fact]
            public async Task EndpointPembuatanAkunLamaTanpaPayloadMengembalikanGone()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                using var Respons = await Klien.PostAsync(
                    "/api/accounts",
                    null);

                Assert.Equal(
                    HttpStatusCode.Gone,
                    Respons.StatusCode);
            }

            [Fact]
            public async Task EndpointAkunMenolakAksesTanpaToken()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                var IdAkun = Guid.NewGuid();

                using var Respons = await Klien.GetAsync(
                    $"/api/accounts/{IdAkun}");

                Assert.Equal(
                    HttpStatusCode.Unauthorized,
                    Respons.StatusCode);
            }

            [Fact]
            public async Task EndpointAkunMenolakTokenTidakValid()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                Klien.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue(
                        "Bearer",
                        "token-tidak-valid");

                var IdAkun = Guid.NewGuid();

                using var Respons = await Klien.GetAsync(
                    $"/api/accounts/{IdAkun}");

                Assert.Equal(
                    HttpStatusCode.Unauthorized,
                    Respons.StatusCode);
            }

            [Fact]
            public async Task EndpointLoginMembatasiPermintaanKesebelas()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                var Permintaan = new
                {
                    Email = "batas-login@example.invalid",
                    KataSandi = ""
                };

                for (var Nomor = 1; Nomor <= 10; Nomor++)
                {
                    using var Respons = await Klien.PostAsJsonAsync(
                        "/api/auth/login",
                        Permintaan);

                    Assert.Equal(
                        HttpStatusCode.BadRequest,
                        Respons.StatusCode);
                }

                using var ResponsTerbatas = await Klien.PostAsJsonAsync(
                    "/api/auth/login",
                    Permintaan);

                Assert.Equal(
                    HttpStatusCode.TooManyRequests,
                    ResponsTerbatas.StatusCode);
            }

            [Fact]
            public async Task EndpointRegistrasiMembatasiPermintaanKesebelas()
            {
                using var Pabrik = new PabrikApiPengujian();
                using var Klien = Pabrik.CreateClient();

                var Permintaan = new
                {
                    Email = "batas-registrasi@example.invalid",
                    DisplayName = "Pengguna Pengujian",
                    KataSandi = ""
                };

                for (var Nomor = 1; Nomor <= 10; Nomor++)
                {
                    using var Respons = await Klien.PostAsJsonAsync(
                        "/api/auth/register",
                        Permintaan);

                    Assert.Equal(
                        HttpStatusCode.BadRequest,
                        Respons.StatusCode);
                }

                using var ResponsTerbatas = await Klien.PostAsJsonAsync(
                    "/api/auth/register",
                    Permintaan);

                Assert.Equal(
                    HttpStatusCode.TooManyRequests,
                    ResponsTerbatas.StatusCode);
            }

            private sealed class PabrikApiPengujian
                : WebApplicationFactory<Program>
            {
                public InMemoryAccountRepository Repositori { get; } = new();

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

                        Layanan.AddSingleton<IAccountRepository>(
                            Repositori);
                    });
                }
            }
        }
    }
}
