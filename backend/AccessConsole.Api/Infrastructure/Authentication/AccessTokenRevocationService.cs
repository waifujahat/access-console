/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Authentication
 *
 * Persists revoked access token hashes and checks whether an access token has been revoked
 */

using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Domain.Authentication;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Authentication
    {
        public sealed class AccessTokenRevocationService
        {
            private readonly AccessConsoleDbContext Konteks;

            public AccessTokenRevocationService(
                AccessConsoleDbContext Konteks)
            {
                this.Konteks = Konteks;
            }

            public async Task<bool> SudahDicabutAsync(
                string Token,
                CancellationToken TokenPembatalan = default)
            {
                var HashToken = RevokedAccessToken.HitungHash(Token);

                return await Konteks.RevokedAccessTokens
                    .AsNoTracking()
                    .AnyAsync(
                        Catatan => Catatan.TokenHash == HashToken,
                        TokenPembatalan);
            }

            public async Task CabutAsync(
                string Token,
                DateTimeOffset Kedaluwarsa,
                CancellationToken TokenPembatalan = default)
            {
                var Catatan = RevokedAccessToken.Create(
                    Token,
                    Kedaluwarsa);

                await Konteks.Database.ExecuteSqlInterpolatedAsync(
                    $"""
                    INSERT INTO revoked_access_tokens
                        (token_hash, revoked_at_utc, expires_at_utc)
                    VALUES
                        ({Catatan.TokenHash},
                         {Catatan.RevokedAtUtc},
                         {Catatan.ExpiresAtUtc})
                    ON CONFLICT (token_hash) DO NOTHING
                    """,
                    TokenPembatalan);
            }
        }
    }
}
