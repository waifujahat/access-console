/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Persistence / Accounts
 *
 * Implements temporary in-memory account persistence
 */

using System.Collections.Concurrent;
using Waifujahat.AccessConsole.Api.Application.Accounts;
using Waifujahat.AccessConsole.Api.Domain.Accounts;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Persistence
    {
        public sealed class InMemoryAccountRepository : IAccountRepository
        {
            private readonly ConcurrentDictionary<Guid, Account> AkunSementara = new();

            public Task AddAsync(
                Account Akun,
                CancellationToken TokenPembatalan = default)
            {
                TokenPembatalan.ThrowIfCancellationRequested();

                if (!AkunSementara.TryAdd(Akun.Id, Akun))
                {
                    throw new InvalidOperationException(
                        "Akun dengan ID tersebut sudah ada");
                }

                return Task.CompletedTask;
            }

            public Task<Account?> GetByIdAsync(
                Guid Id,
                CancellationToken TokenPembatalan = default)
            {
                TokenPembatalan.ThrowIfCancellationRequested();

                AkunSementara.TryGetValue(Id, out var Akun);

                return Task.FromResult<Account?>(Akun);
            }
        }
    }
}
