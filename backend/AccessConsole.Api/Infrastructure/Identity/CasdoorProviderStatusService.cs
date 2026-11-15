/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Identity
 *
 * Synchronizes connected provider status from the authenticated Casdoor account
 */

using System.Data.Common;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat.AccessConsole.Api.Infrastructure.Identity
{
    public sealed class CasdoorProviderStatusService(
        HttpClient Klien,
        AccessConsoleDbContext Konteks,
        IConfiguration Konfigurasi,
        ILogger<CasdoorProviderStatusService> Log)
    {
        private string? Issuer => Konfigurasi["CASDOOR_ISSUER"]?
            .Trim().TrimEnd('/');

        private static string NamaStatus(string Subject) =>
            $"connected-providers:{Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(Subject)))}";

        public async Task<(bool? Google, bool? Github)> DapatkanStatusAsync(
            Guid IdAkun,
            CancellationToken TokenPembatalan)
        {
            if (string.IsNullOrWhiteSpace(Issuer))
            {
                return (null, null);
            }

            var NamaPenyedia = $"Casdoor:{Issuer}";
            var Subjects = await Konteks.UserLogins.AsNoTracking()
                .Where(Login => Login.UserId == IdAkun &&
                    Login.LoginProvider == NamaPenyedia)
                .Select(Login => Login.ProviderKey)
                .ToListAsync(TokenPembatalan);

            if (Subjects.Count == 0)
            {
                return (false, false);
            }

            var NamaStatusAkun = Subjects.Select(NamaStatus).ToArray();
            var StatusAkun = await Konteks.UserTokens.AsNoTracking()
                .Where(Token => Token.UserId == IdAkun &&
                    Token.LoginProvider == NamaPenyedia &&
                    NamaStatusAkun.Contains(Token.Name))
                .Select(Token => Token.Value)
                .ToListAsync(TokenPembatalan);

            var StatusValid = StatusAkun.Where(Nilai =>
                Nilai is "0:0" or "0:1" or "1:0" or "1:1").ToList();
            var Lengkap = StatusValid.Count == Subjects.Count;

            bool? Google = StatusValid.Any(Nilai => Nilai is "1:0" or "1:1")
                ? true : Lengkap ? false : null;
            bool? Github = StatusValid.Any(Nilai => Nilai is "0:1" or "1:1")
                ? true : Lengkap ? false : null;

            return (Google, Github);
        }

        public async Task SinkronkanAsync(
            Guid IdAkun,
            string Subject,
            string? AccessToken,
            CancellationToken TokenPembatalan)
        {
            var Alamat = Issuer;

            if (string.IsNullOrWhiteSpace(Alamat) ||
                string.IsNullOrWhiteSpace(AccessToken))
            {
                return;
            }

            try
            {
                using var Permintaan = new HttpRequestMessage(
                    HttpMethod.Get, $"{Alamat}/api/get-account");
                Permintaan.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", AccessToken);

                using var Respons = await Klien.SendAsync(
                    Permintaan, TokenPembatalan);

                if (!Respons.IsSuccessStatusCode)
                {
                    return;
                }

                var Profil = await Respons.Content.ReadFromJsonAsync<ProfilCasdoor>(
                    cancellationToken: TokenPembatalan);

                if (Profil is null || Profil.Status != "ok" ||
                    Profil.Sub != Subject ||
                    Profil.Data is not { } Identitas ||
                    Identitas.Id != Subject ||
                    Identitas.Google is null ||
                    Identitas.Github is null)
                {
                    return;
                }

                var StatusGoogle = string.IsNullOrWhiteSpace(Identitas.Google) ? 0 : 1;
                var StatusGithub = string.IsNullOrWhiteSpace(Identitas.Github) ? 0 : 1;
                var Nilai = $"{StatusGoogle}:{StatusGithub}";
                var NamaPenyedia = $"Casdoor:{Alamat}";
                var Nama = NamaStatus(Subject);

                // Only persist status for an identity already owned by this account.
                // Store flags, never the external identifiers or access tokens.
                await Konteks.Database.ExecuteSqlInterpolatedAsync($"""
                    INSERT INTO "AspNetUserTokens" ("UserId", "LoginProvider", "Name", "Value")
                    SELECT "UserId", "LoginProvider", {Nama}, {Nilai}
                    FROM "AspNetUserLogins"
                    WHERE "UserId" = {IdAkun}
                        AND "LoginProvider" = {NamaPenyedia}
                        AND "ProviderKey" = {Subject}
                    ON CONFLICT ("UserId", "LoginProvider", "Name")
                    DO UPDATE SET "Value" = EXCLUDED."Value"
                    """, TokenPembatalan);
            }
            catch (Exception Kesalahan) when (
                Kesalahan is HttpRequestException or JsonException or DbException ||
                Kesalahan is OperationCanceledException &&
                !TokenPembatalan.IsCancellationRequested)
            {
                // Status synchronization is informational, not an authentication decision.
                Log.LogWarning("Status provider Casdoor belum dapat diperbarui");
            }
        }

        private sealed record ProfilCasdoor(
            string? Status,
            string? Sub,
            IdentitasCasdoor? Data);

        private sealed record IdentitasCasdoor(
            string? Id,
            string? Google,
            string? Github);
    }
}
