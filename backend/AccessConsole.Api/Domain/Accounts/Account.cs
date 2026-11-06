/*
 * Access Console
 * Author : waifujahat
 * Module : Identity / Accounts
 *
 * Represents an account and validates
 * account creation input
 */

using System.Net.Mail;

namespace Waifujahat
{
    namespace AccessConsole.Api.Domain.Accounts
    {
        public sealed class Account
        {
            private Account(
                Guid Id,
                string Email,
                string DisplayName,
                DateTimeOffset CreatedatUTC)
            {
                this.Id = Id;
                this.Email = Email;
                this.DisplayName = DisplayName;
                this.CreatedatUTC = CreatedatUTC;
            }

            public Guid Id { get; }

            public string Email { get; }

            public string DisplayName { get; }

            public DateTimeOffset CreatedatUTC { get; }

            public static Account Create(string Email, string DisplayName)
            {
                ArgumentException.ThrowIfNullOrWhiteSpace(Email);
                ArgumentException.ThrowIfNullOrWhiteSpace(DisplayName);

                var EmailBersih = Email.Trim();
                var NamaBersih = DisplayName.Trim();

                if (EmailBersih.Length > 254)
                {
                    throw new ArgumentException(
                        "Email tidak boleh melebihi 254 karakter",
                        nameof(Email));
                }

                if (!MailAddress.TryCreate(EmailBersih, out var AlamatEmail) ||
                    !string.Equals(
                        AlamatEmail.Address,
                        EmailBersih,
                        StringComparison.Ordinal))
                {
                    throw new ArgumentException(
                        "Format email tidak valid",
                        nameof(Email));
                }

                if (NamaBersih.Length > 100)
                {
                    throw new ArgumentException(
                        "Nama tidak boleh melebihi 100 karakter",
                        nameof(DisplayName));
                }

                return new Account(
                    Guid.NewGuid(),
                    EmailBersih,
                    NamaBersih,
                    DateTimeOffset.UtcNow);
            }
        }
    }
}
