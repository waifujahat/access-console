/*
 * Access Console
 * Author : waifujahat
 * Module : API / Application
 *
 * Configures application, health checks and account management endpoints
 */

using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Contracts.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

var Builder = WebApplication.CreateBuilder(args);

Builder.Services.AddHealthChecks();

Builder.Services.AddSingleton<IAccountRepository, InMemoryAccountRepository>();

var App = Builder.Build();

App.MapHealthChecks("/health");

App.MapPost("/api/accounts", async (
    CreateAccountRequest Permintaan,
    IAccountRepository Repositori,
    CancellationToken TokenPembatalan) =>
{
    try
    {
        var AkunBaru = Account.Create(
            Permintaan.Email,
            Permintaan.DisplayName);

        await Repositori.AddAsync(AkunBaru, TokenPembatalan);

        var Respons = new AccountResponse(
            AkunBaru.Id,
            AkunBaru.Email,
            AkunBaru.DisplayName,
            AkunBaru.CreatedatUTC);

        return Results.Created(
            $"/api/accounts/{AkunBaru.Id}",
            Respons);
    }
    catch (ArgumentException Error)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                [Error.ParamName ?? "Account"] =
                    [Error.Message]
            });
    }
});

App.MapGet("/api/accounts/{Id:guid}", async (
    Guid Id,
    IAccountRepository Repositori,
    CancellationToken TokenPembatalan) =>
{
    var Akun = await Repositori.GetByIdAsync(Id, TokenPembatalan);

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
});

App.Run();
