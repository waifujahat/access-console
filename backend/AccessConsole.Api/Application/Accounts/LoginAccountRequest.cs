/*
 * Access Console
 * Author : waifujahat
 * Module : Application / Accounts
 *
 * Represents the required data for account login
 */

namespace Waifujahat
{
    namespace AccessConsole.Api.Application.Accounts
    {
        public sealed record LoginAccountRequest(
            string Email,
            string KataSandi);
    }
}
