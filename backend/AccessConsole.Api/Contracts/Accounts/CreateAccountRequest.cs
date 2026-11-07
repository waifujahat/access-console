/*
 * Access Console
 * Author : waifujahat
 * Module : API / Contracts / Accounts
 *
 * Defines request payload for account creation
 */

namespace Waifujahat
{
    namespace AccessConsole.Api.Contracts.Accounts
    {
        public sealed record CreateAccountRequest(
            string Email,
            string DisplayName);
    }
}
