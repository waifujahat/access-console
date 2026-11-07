/*
 * Access Console
 * Author : waifujahat
 * Module : Tests / Identity
 *
 * Verifies ASP.NET Core Identity service registration
 */

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Waifujahat.AccessConsole.Api.Infrastructure.Identity;

namespace Waifujahat
{
    namespace AccessConsole.Api.Tests
    {
        public sealed class IdentityServiceRegistrationTests
        {
            [Fact]
            public void LayananIdentityTerdaftar()
            {
                using var Aplikasi = new WebApplicationFactory<Program>()
                    .WithWebHostBuilder(Builder =>
                    {
                        Builder.ConfigureAppConfiguration((Konteks, Konfigurasi) =>
                        {
                            Konfigurasi.AddInMemoryCollection(
                                new Dictionary<string, string?>
                                {
                                    ["ConnectionStrings:AccessConsole"] =
                                        "Host=127.0.0.1;Database=access_console_test;Username=test;Password=unused"
                                });
                        });
                    });

                using var Lingkup = Aplikasi.Services.CreateScope();

                var PengelolaPengguna = Lingkup.ServiceProvider
                    .GetRequiredService<UserManager<AccessConsoleIdentityUser>>();

                var PengelolaRole = Lingkup.ServiceProvider
                    .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

                var LayananRegistrasi = Lingkup.ServiceProvider
                    .GetRequiredService<AccountRegistrationService>();

                Assert.NotNull(PengelolaPengguna);
                Assert.NotNull(PengelolaRole);
                Assert.NotNull(LayananRegistrasi);
            }
        }
    }
}
