/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Identity
 *
 * Represents an account in the ASP.NET Core Identity system
 */

using Microsoft.AspNetCore.Identity;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Identity
    {
        public sealed class AccessConsoleIdentityUser : IdentityUser<Guid>
        {
        }
    }
}
