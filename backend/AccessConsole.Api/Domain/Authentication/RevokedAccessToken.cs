/*
 * Access Console
 * Author : waifujahat
 * Module : Domain / Authentication
 *
 * Revoked access token without persisting the original token value
 */

using System.Security.Cryptography;
using System.Text;

namespace Waifujahat
{
    namespace AccessConsole.Api.Domain.Authentication
    {
        public sealed class RevokedAccessToken
        {
            private RevokedAccessToken(
                string TokenHash,
                DateTimeOffset RevokedAtUtc,
                DateTimeOffset ExpiresAtUtc)
            {
                this.TokenHash = TokenHash;
                this.RevokedAtUtc = RevokedAtUtc;
                this.ExpiresAtUtc = ExpiresAtUtc;
            }

            public string TokenHash { get; }

            public DateTimeOffset RevokedAtUtc { get; }

            public DateTimeOffset ExpiresAtUtc { get; }

            public static RevokedAccessToken Create(
                string Token,
                DateTimeOffset ExpiresAtUtc)
            {
                var WaktuPencabutan = DateTimeOffset.UtcNow;

                if (ExpiresAtUtc <= WaktuPencabutan ||
                    ExpiresAtUtc.Offset != TimeSpan.Zero)
                {
                    throw new ArgumentException(
                        "Waktu kedaluwarsa token tidak valid",
                        nameof(ExpiresAtUtc));
                }

                return new RevokedAccessToken(
                    HitungHash(Token),
                    WaktuPencabutan,
                    ExpiresAtUtc);
            }

            public static string HitungHash(string Token)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(Token);

                var DataToken = Encoding.UTF8.GetBytes(Token);
                var Hash = SHA256.HashData(DataToken);

                return Convert.ToHexString(Hash);
            }
        }
    }
}
