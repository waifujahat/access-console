/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Identity / SignInManager
 *
 * Records recent password authentication in protected authentication tickets
 */

using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Waifujahat.AccessConsole.Api.Infrastructure.Identity
{
    public sealed class AccessConsoleSignInManager
        : SignInManager<AccessConsoleIdentityUser>
    {
        public const string PenandaWaktuAutentikasi =
            "access_console:password_authenticated_at";

        private bool _AutentikasiKataSandi;

        public AccessConsoleSignInManager(
            UserManager<AccessConsoleIdentityUser> userManager,
            IHttpContextAccessor contextAccessor,
            IUserClaimsPrincipalFactory<AccessConsoleIdentityUser> claimsFactory,
            IOptions<IdentityOptions> optionsAccessor,
            ILogger<SignInManager<AccessConsoleIdentityUser>> logger,
            IAuthenticationSchemeProvider schemes,
            IUserConfirmation<AccessConsoleIdentityUser> confirmation)
            : base(
                userManager,
                contextAccessor,
                claimsFactory,
                optionsAccessor,
                logger,
                schemes,
                confirmation)
        {
        }

        public override async Task<SignInResult> PasswordSignInAsync(
            AccessConsoleIdentityUser user,
            string password,
            bool isPersistent,
            bool lockoutOnFailure)
        {
            _AutentikasiKataSandi = true;

            try
            {
                return await base.PasswordSignInAsync(
                    user,
                    password,
                    isPersistent,
                    lockoutOnFailure);
            }
            finally
            {
                _AutentikasiKataSandi = false;
            }
        }

        public override Task SignInWithClaimsAsync(
            AccessConsoleIdentityUser user,
            AuthenticationProperties? authenticationProperties,
            IEnumerable<Claim> additionalClaims)
        {
            if (_AutentikasiKataSandi &&
                AuthenticationScheme == IdentityConstants.BearerScheme)
            {
                authenticationProperties ??=
                    new AuthenticationProperties();

                authenticationProperties.Items[PenandaWaktuAutentikasi] =
                    DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                        .ToString(CultureInfo.InvariantCulture);
            }

            return base.SignInWithClaimsAsync(
                user,
                authenticationProperties,
                additionalClaims);
        }
    }
}
