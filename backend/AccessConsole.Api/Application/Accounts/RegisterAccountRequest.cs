/*
 * Access Console
 * Author : waifujahat
 * Module : Application / Accounts
 *
 * Represents required data for account registration
 */

namespace Waifujahat
{
    namespace AccessConsole.Api.Application.Accounts
    {
        public sealed record RegisterAccountRequest(
            string Email,
            string DisplayName,
            string KataSandi);
    }
}
