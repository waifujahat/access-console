/*
 * Access Console
 * Author : waifujahat
 * Module : Application / Accounts
 *
 * Defines contract for account persistence
 */

using Waifujahat.AccessConsole.Api.Domain.Accounts;

namespace Waifujahat
{
    namespace AccessConsole.Api.Application.Accounts
    {
        public interface IAccountRepository
        {
            Task AddAsync(
                Account Akun,
                CancellationToken TokenPembatalan = default);

            Task<Account?> GetByIdAsync(
                Guid Id,
                CancellationToken TokenPembatalan = default);
        }
    }
}
