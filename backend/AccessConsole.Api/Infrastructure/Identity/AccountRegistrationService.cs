/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Identity
 *
 * Registers domain accounts and Identity users in one transaction
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat
{
    namespace AccessConsole.Api.Infrastructure.Identity
    {
        public sealed class AccountRegistrationService
        {
            private readonly AccessConsoleDbContext Konteks;
            private readonly UserManager<AccessConsoleIdentityUser> PengelolaPengguna;

            public AccountRegistrationService(
                AccessConsoleDbContext Konteks,
                UserManager<AccessConsoleIdentityUser> PengelolaPengguna)
            {
                this.Konteks = Konteks;
                this.PengelolaPengguna = PengelolaPengguna;
            }

            public async Task<(Account? Akun, IdentityResult Hasil)> DaftarkanAsync(
                string Email,
                string DisplayName,
                string KataSandi,
                CancellationToken TokenPembatalan = default)
            {
                var Akun = Account.Create(Email, DisplayName);

                var Pengguna = new AccessConsoleIdentityUser
                {
                    Id = Akun.Id,
                    UserName = Akun.Email,
                    Email = Akun.Email
                };

                await using var Transaksi =
                    await Konteks.Database.BeginTransactionAsync(TokenPembatalan);

                Konteks.Accounts.Add(Akun);

                await Konteks.SaveChangesAsync(TokenPembatalan);

                var Hasil = await PengelolaPengguna.CreateAsync(
                    Pengguna,
                    KataSandi);

                if (!Hasil.Succeeded)
                {
                    await Transaksi.RollbackAsync(TokenPembatalan);

                    Konteks.Entry(Akun).State = EntityState.Detached;

                    return (null, Hasil);
                }

                await Transaksi.CommitAsync(TokenPembatalan);

                return (Akun, Hasil);
            }
        }
    }
}
