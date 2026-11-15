/*
 * Access Console
 * Author : waifujahat
 * Module : API / Endpoints / Authentication
 *
 * Verifies Casdoor identities and issues access Console authentication tokens
 */

using System.Globalization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Waifujahat.AccessConsole.Api.Infrastructure.Identity;

namespace Waifujahat.AccessConsole.Api.Endpoints
{
    public sealed record KonfirmasiTautanCasdoor(string? KataSandi);

    public static class CasdoorAuthenticationEndpoints
    {
        public static IEndpointRouteBuilder MapCasdoorAuthenticationEndpoints(
            this IEndpointRouteBuilder Endpoints)
        {
            Endpoints.MapPost("/api/auth/casdoor/verify", () =>
            {
                return Results.NoContent();
            })
            .RequireAuthorization("CasdoorOIDC");

            Endpoints.MapPost("/api/auth/casdoor/exchange", async (
                HttpContext KonteksHttp,
                IConfiguration Konfigurasi,
                CasdoorAccountService LayananAkun,
                CasdoorProviderStatusService LayananStatus,
                SignInManager<AccessConsoleIdentityUser> PengelolaMasuk,
                CancellationToken TokenPembatalan) =>
            {
                var Issuer = Konfigurasi["CASDOOR_ISSUER"]?
                    .Trim().TrimEnd('/');

                var Subject = KonteksHttp.User
                    .FindFirst("sub")?.Value;

                var Email = KonteksHttp.User
                    .FindFirst("email")?.Value;

                var Nama = KonteksHttp.User
                    .FindFirst("name")?.Value ??
                    KonteksHttp.User
                    .FindFirst("preferred_username")?.Value;

                var EmailTerverifikasi = string.Equals(
                    KonteksHttp.User.FindFirst("email_verified")?.Value,
                    "true",
                    StringComparison.OrdinalIgnoreCase);

                if (string.IsNullOrWhiteSpace(Issuer) ||
                    string.IsNullOrWhiteSpace(Subject))
                {
                    return Results.Unauthorized();
                }

                var (Pengguna, Kesalahan) =
                    await LayananAkun.DapatkanAtauDaftarkanAsync(
                        Issuer,
                        Subject,
                        Email,
                        Nama,
                        EmailTerverifikasi,
                        TokenPembatalan);

                if (Pengguna is null)
                {
                    if (Kesalahan == "Email sudah digunakan oleh akun lain")
                    {
                        return Results.Conflict(new { Kesalahan });
                    }

                    return Results.BadRequest(new { Kesalahan });
                }

                if (!await PengelolaMasuk.CanSignInAsync(Pengguna) ||
                    await PengelolaMasuk.UserManager
                        .IsLockedOutAsync(Pengguna))
                {
                    return Results.Forbid();
                }

                await LayananStatus.SinkronkanAsync(
                    Pengguna.Id,
                    Subject,
                    await KonteksHttp.GetTokenAsync("CasdoorOIDC", "access_token"),
                    TokenPembatalan);

                PengelolaMasuk.AuthenticationScheme =
                    IdentityConstants.BearerScheme;

                await PengelolaMasuk.SignInAsync(
                    Pengguna,
                    isPersistent: false);

                return TypedResults.Empty;
            })
            .RequireAuthorization("CasdoorOIDC")
            .RequireRateLimiting("login");

            Endpoints.MapPost("/api/auth/casdoor/link", async (
                KonfirmasiTautanCasdoor Permintaan,
                HttpContext KonteksHttp,
                IConfiguration Konfigurasi,
                UserManager<AccessConsoleIdentityUser> PengelolaPengguna,
                SignInManager<AccessConsoleIdentityUser> PengelolaMasuk,
                CasdoorAccountService LayananAkun,
                CasdoorProviderStatusService LayananStatus,
                CancellationToken TokenPembatalan) =>
            {
                var HasilLokal = await KonteksHttp.AuthenticateAsync(
                    IdentityConstants.BearerScheme);

                if (!HasilLokal.Succeeded || HasilLokal.Principal is null)
                {
                    return Results.Unauthorized();
                }

                var IdAkun = PengelolaPengguna.GetUserId(
                    HasilLokal.Principal);

                if (!Guid.TryParse(IdAkun, out var IdPengguna))
                {
                    return Results.Unauthorized();
                }

                var Pengguna = await PengelolaMasuk
                    .ValidateSecurityStampAsync(HasilLokal.Principal);

                if (Pengguna is null ||
                    Pengguna.Id != IdPengguna ||
                    !await PengelolaMasuk.CanSignInAsync(Pengguna) ||
                    await PengelolaPengguna.IsLockedOutAsync(Pengguna))
                {
                    return Results.Unauthorized();
                }

                string? PenandaWaktu = null;

                if (HasilLokal.Properties is { } PropertiAutentikasi)
                {
                    PropertiAutentikasi.Items.TryGetValue(
                        AccessConsoleSignInManager.PenandaWaktuAutentikasi,
                        out PenandaWaktu);
                }

                var WaktuSekarang = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

                var AutentikasiTerbaru =
                    long.TryParse(
                        PenandaWaktu,
                        NumberStyles.None,
                        CultureInfo.InvariantCulture,
                        out var WaktuAutentikasi) &&
                    WaktuAutentikasi <= WaktuSekarang &&
                    WaktuAutentikasi >= WaktuSekarang - 300;

                if (!string.IsNullOrEmpty(Permintaan.KataSandi))
                {
                    var HasilKonfirmasi = await PengelolaMasuk
                        .CheckPasswordSignInAsync(
                            Pengguna,
                            Permintaan.KataSandi,
                            lockoutOnFailure: true);

                    if (!HasilKonfirmasi.Succeeded)
                    {
                        return Results.Unauthorized();
                    }
                }
                else if (!AutentikasiTerbaru)
                {
                    return Results.Json(
                        new
                        {
                            Kesalahan = "Autentikasi ulang diperlukan"
                        },
                        statusCode: StatusCodes.Status428PreconditionRequired);
                }

                var HasilCasdoor = await KonteksHttp
                    .AuthenticateAsync("CasdoorOIDC");

                if (!HasilCasdoor.Succeeded ||
                    HasilCasdoor.Principal is null)
                {
                    return Results.Unauthorized();
                }

                var Issuer = Konfigurasi["CASDOOR_ISSUER"]?.Trim();

                var Subject = HasilCasdoor.Principal
                    .FindFirst("sub")?.Value;

                if (string.IsNullOrWhiteSpace(Issuer) ||
                    string.IsNullOrWhiteSpace(Subject))
                {
                    return Results.Unauthorized();
                }

                var (Berhasil, Kesalahan) =
                    await LayananAkun.TautkanAsync(
                        IdPengguna,
                        Issuer,
                        Subject);

                if (!Berhasil)
                {
                    return Results.Conflict(new { Kesalahan });
                }

                await LayananStatus.SinkronkanAsync(
                    IdPengguna,
                    Subject,
                    HasilCasdoor.Properties?.GetTokenValue("access_token"),
                    TokenPembatalan);

                return Results.NoContent();
            })
            .RequireAuthorization()
            .RequireRateLimiting("login");

            return Endpoints;
        }
    }
}
