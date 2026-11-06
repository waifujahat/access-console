/*
 * Access Console
 * Author : waifujahat
 * Module : API / Application
 *
 * Configures application, health checks and account management endpoints
 */

using System.Collections.Concurrent;
using Waifujahat.AccessConsole.Api.Contracts.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Accounts;

var Builder = WebApplication.CreateBuilder(args);

Builder.Services.AddHealthChecks();

var App = Builder.Build();

var AkunSementara = new ConcurrentDictionary<Guid, Account>();

App.MapHealthChecks("/health");

App.MapPost("/api/accounts", (CreateAccountRequest Permintaan) =>
{
    try
    {
        var AkunBaru = Account.Create(
            Permintaan.Email,
            Permintaan.DisplayName);

        AkunSementara[AkunBaru.Id] = AkunBaru;

        var Respons = new AccountResponse(
            AkunBaru.Id,
            AkunBaru.Email,
            AkunBaru.DisplayName,
            AkunBaru.CreatedatUTC);

        return Results.Created(
            $"/api/accounts/{AkunBaru.Id}",
            Respons);
    }
    catch (ArgumentException Kesalahan)
    {
        return Results.ValidationProblem(
            new Dictionary<string, string[]>
            {
                [Kesalahan.ParamName ?? "Account"] =
                    [Kesalahan.Message]
            });
    }
});

App.MapGet("/api/accounts/{Id:guid}", (Guid Id) =>
{
    if (!AkunSementara.TryGetValue(Id, out var Akun))
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
