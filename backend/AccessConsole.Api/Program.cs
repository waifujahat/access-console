/*
 * Access Console
 * Author : waifujahat
 * Module : API / Application
 *
 * Configures application, health checks and account management endpoints
 */

using Microsoft.EntityFrameworkCore;
using Npgsql;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Contracts.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

var Builder = WebApplication.CreateBuilder(args);

var KoneksiDatabase = Builder.Configuration.GetConnectionString("AccessConsole");

if (string.IsNullOrWhiteSpace(KoneksiDatabase))
{
    throw new InvalidOperationException(
        "Connection string AccessConsole belum dikonfigurasi");
}

Builder.Services.AddHealthChecks();

Builder.Services.AddDbContext<AccessConsoleDbContext>(
    Opsi => Opsi.UseNpgsql(KoneksiDatabase));

Builder.Services.AddScoped<IAccountRepository, PostgresAccountRepository>();

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
    catch (DbUpdateException Error) when (
        Error.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: "IX_accounts_email"
        })
    {
        return Results.Conflict(
            new { Pesan = "Email sudah terdaftar" });
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

public partial class Program // Exposes application entry point for integration testing
{
}
