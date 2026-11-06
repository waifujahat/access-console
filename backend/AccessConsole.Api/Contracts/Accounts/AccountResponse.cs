/*
 * Access Console
 * Author : waifujahat
 * Module : API / Contracts / Accounts
 *
 * Defines the response payload for account information
 */

namespace Waifujahat
{
    namespace AccessConsole.Api.Contracts.Accounts
    {
        public sealed record AccountResponse(
            Guid Id,
            string Email,
            string DisplayName,
            DateTimeOffset CreatedatUTC);
    }
}
