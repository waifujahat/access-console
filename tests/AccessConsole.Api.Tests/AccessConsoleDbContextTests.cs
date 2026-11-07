/*
 * Access Console
 * Author : waifujahat
 * Module : Tests / Infrastructure / Persistence
 *
 * Verifies the Entity Framework Core account model configuration
 */

using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat
{
    namespace AccessConsole.Api.Tests.Infrastructure.Persistence
    {
        public sealed class AccessConsoleDbContextTests
        {
            private static AccessConsoleDbContext BuatKonteks()
            {
                var Opsi = new DbContextOptionsBuilder<AccessConsoleDbContext>()
                    .UseNpgsql("Host=localhost;Database=access_console")
                    .Options;

                return new AccessConsoleDbContext(Opsi);
            }

            [Fact]
            public void AkunMenggunakanTabelAccounts()
            {
                using var Konteks = BuatKonteks();

                var Entitas = Konteks.Model.FindEntityType(typeof(Account));

                Assert.NotNull(Entitas);
                Assert.Equal("accounts", Entitas.GetTableName());
            }

            [Fact]
            public void AkunMemilikiPrimaryKeyDanBatasKolom()
            {
                using var Konteks = BuatKonteks();

                var Entitas = Konteks.Model.FindEntityType(typeof(Account));

                Assert.NotNull(Entitas);

                var KunciUtama = Entitas.FindPrimaryKey();

                Assert.NotNull(KunciUtama);
                Assert.Equal(
                    nameof(Account.Id),
                    Assert.Single(KunciUtama.Properties).Name);

                var Email = Entitas.FindProperty(nameof(Account.Email));
                var NamaTampilan = Entitas.FindProperty(nameof(Account.DisplayName));

                Assert.NotNull(Email);
                Assert.NotNull(NamaTampilan);

                Assert.Equal(254, Email.GetMaxLength());
                Assert.Equal(100, NamaTampilan.GetMaxLength());
            }

            [Fact]
            public void EmailMemilikiUniqueIndex()
            {
                using var Konteks = BuatKonteks();

                var Entitas = Konteks.Model.FindEntityType(typeof(Account));

                Assert.NotNull(Entitas);

                var Indeks = Assert.Single(Entitas.GetIndexes());

                Assert.True(Indeks.IsUnique);
                Assert.Equal(
                    nameof(Account.Email),
                    Assert.Single(Indeks.Properties).Name);
            }

            [Fact]
            public void EmailMenggunakanTipeCitext()
            {
                using var Konteks = BuatKonteks();

                var Entitas = Konteks.Model.FindEntityType(typeof(Account));

                Assert.NotNull(Entitas);

                var Email = Entitas.FindProperty(nameof(Account.Email));

                Assert.NotNull(Email);
                Assert.Equal("citext", Email.GetColumnType());
            }
        }
    }
}
