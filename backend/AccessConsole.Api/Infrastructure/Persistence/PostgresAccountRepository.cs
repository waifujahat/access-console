/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Persistence
 *
 * Implements account persistence using PostgreSQL
 */

using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Accounts;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Persistence
    {
        public sealed class PostgresAccountRepository : IAccountRepository
        {
            private readonly AccessConsoleDbContext KonteksDatabase;

            public PostgresAccountRepository(
                AccessConsoleDbContext Konteks)
            {
                KonteksDatabase = Konteks;
            }

            public async Task AddAsync(
                Account Akun,
                CancellationToken TokenPembatalan = default)
            {
                await KonteksDatabase.Accounts.AddAsync(
                    Akun,
                    TokenPembatalan);

                await KonteksDatabase.SaveChangesAsync(
                    TokenPembatalan);
            }

            public Task<Account?> GetByIdAsync(
                Guid Id,
                CancellationToken TokenPembatalan = default)
            {
                return KonteksDatabase.Accounts
                    .AsNoTracking()
                    .FirstOrDefaultAsync(
                        Akun => Akun.Id == Id,
                        TokenPembatalan);
            }
        }
    }
}
