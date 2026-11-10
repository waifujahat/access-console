/*
 * Access Console
 * Author : waifujahat
 * Module : API / Endpoints / Accounts
 *
 * Maps account endpoints and enforces account ownership
 */

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Contracts.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Identity;

namespace Waifujahat
{
    namespace AccessConsole.Api.Endpoints
    {
        public static class AccountEndpoints
        {
            public static IEndpointRouteBuilder MapAccountEndpoints(
                this IEndpointRouteBuilder Endpoints)
            {
                Endpoints.MapPost("/api/accounts", () =>
                {
                    return Results.StatusCode(
                        StatusCodes.Status410Gone);
                });

                Endpoints.MapGet("/api/accounts/{Id:guid}", async (
                    Guid Id,
                    HttpContext KonteksHttp,
                    UserManager<AccessConsoleIdentityUser> PengelolaPengguna,
                    IAccountRepository Repositori,
                    CancellationToken TokenPembatalan) =>
                {
                    var IdPengguna = PengelolaPengguna.GetUserId(
                        KonteksHttp.User);

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

                Endpoints.MapGet("/api/accounts/me", async (
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

                return Endpoints;
            }
        }
    }
}
