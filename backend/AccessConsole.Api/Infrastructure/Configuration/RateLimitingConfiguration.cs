/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Configuration
 *
 * Configures API rate limiting policies
 */

using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Configuration
    {
        public static class RateLimitingConfiguration
        {
            public static IServiceCollection TambahkanPembatasPermintaan(
                this IServiceCollection Layanan)
            {
                Layanan.AddRateLimiter(Opsi =>
                {
                    Opsi.RejectionStatusCode =
                        StatusCodes.Status429TooManyRequests;

                    Opsi.AddPolicy("registrasi", Konteks =>
                        RateLimitPartition.GetFixedWindowLimiter(
                            partitionKey:
                                Konteks.Connection.RemoteIpAddress?.ToString()
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
                            partitionKey:
                                Konteks.Connection.RemoteIpAddress?.ToString()
                                ?? "tidak-diketahui",
                            factory: _ => new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 120,
                                Window = TimeSpan.FromMinutes(1),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            }));
                });

                Layanan.AddSingleton<PartitionedRateLimiter<Guid>>(
                    _ => PartitionedRateLimiter.Create<Guid, Guid>(
                        IdAkun =>
                            RateLimitPartition.GetFixedWindowLimiter(
                                partitionKey: IdAkun,
                                factory: _ => new FixedWindowRateLimiterOptions
                                {
                                    PermitLimit = 10,
                                    Window = TimeSpan.FromMinutes(1),
                                    QueueLimit = 0,
                                    AutoReplenishment = true
                                }
                            )));

                return Layanan;
            }
        }
    }
}
