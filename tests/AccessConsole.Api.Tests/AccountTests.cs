/*
 * Access Console
 * Author : waifujahat
 * Module : Identity / Accounts / Tests
 *
 * Validates account creation, input rules, unique identifiers and UTC timestamps
 */

using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Xunit;

namespace Waifujahat
{
    namespace AccessConsole.Api.Tests
    {
        public sealed class AccountTests
        {
            [Fact]
            public void Create_WithValidInput_NormalizesValues()
            {
                var AccountBaru = Account.Create(
                    "  admin@example.com  ",
                    "  Administrator  ");

                Assert.Equal("admin@example.com", AccountBaru.Email);
                Assert.Equal("Administrator", AccountBaru.DisplayName);
                Assert.NotEqual(Guid.Empty, AccountBaru.Id);
            }

            [Theory]
            [InlineData("", "Administrator")]
            [InlineData("   ", "Administrator")]
            [InlineData("admin@example.com", "")]
            [InlineData("admin@example.com", "   ")]
            public void Create_WithEmptyInput_ThrowsArgumentException(
                string Email,
                string DisplayName)
            {
                Assert.Throws<ArgumentException>(
                    () => Account.Create(Email, DisplayName));
            }

            [Fact]
            public void Create_WithLongEmail_ThrowsArgumentException()
            {
                var Email = new string('a', 250) + "@example.com";

                Assert.Throws<ArgumentException>(
                    () => Account.Create(Email, "Administrator"));
            }

            [Fact]
            public void Create_WithLongDisplayName_ThrowsArgumentException()
            {
                var DisplayName = new string('A', 101);

                Assert.Throws<ArgumentException>(
                    () => Account.Create("admin@example.com", DisplayName));
            }

            [Theory]
            [InlineData("bukan-email")]
            [InlineData("admin@")]
            [InlineData("@example.com")]
            [InlineData("admin example.com")]
            public void Create_WithInvalidEmail_ThrowsArgumentException(
                string Email)
            {
                Assert.Throws<ArgumentException>(
                    () => Account.Create(Email, "Administrator"));
            }

            [Fact]
            public void Create_GeneratesUniqueIdentifiers()
            {
                var AccountPertama = Account.Create(
                    "pertama@example.com",
                    "Pengguna Pertama");

                var AccountKedua = Account.Create(
                    "kedua@example.com",
                    "Pengguna Kedua");

                Assert.NotEqual(AccountPertama.Id, AccountKedua.Id);
            }

            [Fact]
            public void Create_UsesUTCTimestamp()
            {
                var WaktuSebelum = DateTimeOffset.UtcNow;

                var AccountBaru = Account.Create(
                    "admin@example.com",
                    "Administrator");

                var WaktuSesudah = DateTimeOffset.UtcNow;

                Assert.Equal(TimeSpan.Zero, AccountBaru.CreatedatUTC.Offset);

                Assert.InRange(
                    AccountBaru.CreatedatUTC,
                    WaktuSebelum,
                    WaktuSesudah);
            }
        }
    }
}
