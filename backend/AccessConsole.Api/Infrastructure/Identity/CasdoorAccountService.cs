/*
 * Access Console
 * Author : waifujahat
 * Module : Infrastructure / Identity
 *
 * Resolves external Casdoor identities and registers new accounts without linking existing accounts by email
 */

using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Waifujahat.AccessConsole.Api.Domain.Accounts;
using Waifujahat.AccessConsole.Api.Infrastructure.Persistence;

namespace Waifujahat.AccessConsole.Api.Infrastructure.Identity
{
    public sealed class CasdoorAccountService
    {
        private readonly AccessConsoleDbContext Konteks;
        private readonly UserManager<AccessConsoleIdentityUser> PengelolaPengguna;

        public CasdoorAccountService(
            AccessConsoleDbContext Konteks,
            UserManager<AccessConsoleIdentityUser> PengelolaPengguna)
        {
            this.Konteks = Konteks;
            this.PengelolaPengguna = PengelolaPengguna;
        }

        public async Task<(AccessConsoleIdentityUser? Pengguna, string? Kesalahan)>
            DapatkanAtauDaftarkanAsync(
                string Issuer,
                string Subject,
                string? Email,
                string? Nama,
                bool EmailTerverifikasi,
                CancellationToken TokenPembatalan)
        {
            if (string.IsNullOrWhiteSpace(Issuer) ||
                string.IsNullOrWhiteSpace(Subject))
            {
                return (null, "Identitas Casdoor tidak valid");
            }

            var NamaPenyedia = $"Casdoor:{Issuer.TrimEnd('/')}";
            var IdEksternal = Subject.Trim();

            var PenggunaTerdaftar = await PengelolaPengguna.FindByLoginAsync(
                NamaPenyedia,
                IdEksternal);

            if (PenggunaTerdaftar is not null)
            {
                return (PenggunaTerdaftar, null);
            }

            if (!EmailTerverifikasi ||
                string.IsNullOrWhiteSpace(Email))
            {
                return (null, "Email Casdoor belum terverifikasi");
            }

            var EmailBersih = Email.Trim();

            var EmailSudahAda = await Konteks.Accounts
                .AnyAsync(
                    Akun => Akun.Email == EmailBersih,
                    TokenPembatalan);

            if (EmailSudahAda)
            {
                return (null, "Email sudah digunakan oleh akun lain");
            }

            var NamaBersih = string.IsNullOrWhiteSpace(Nama)
                ? EmailBersih.Split('@')[0]
                : Nama.Trim();

            if (NamaBersih.Length > 100)
            {
                NamaBersih = NamaBersih[..100];
            }

            Account AkunBaru;

            try
            {
                AkunBaru = Account.Create(EmailBersih, NamaBersih);
            }
            catch (ArgumentException)
            {
                return (null, "Profil Casdoor tidak valid");
            }

            var PenggunaBaru = new AccessConsoleIdentityUser
            {
                Id = AkunBaru.Id,
                UserName = AkunBaru.Email,
                Email = AkunBaru.Email,
                EmailConfirmed = true
            };

            await using var Transaksi =
                await Konteks.Database.BeginTransactionAsync(
                    TokenPembatalan);

            try
            {
                Konteks.Accounts.Add(AkunBaru);
                await Konteks.SaveChangesAsync(TokenPembatalan);

                var HasilRegistrasi = await PengelolaPengguna.CreateAsync(
                    PenggunaBaru);

                if (!HasilRegistrasi.Succeeded)
                {
                    await Transaksi.RollbackAsync(TokenPembatalan);
                    return (null, "Registrasi akun eksternal gagal");
                }

                var InformasiLogin = new UserLoginInfo(
                    NamaPenyedia,
                    IdEksternal,
                    "Casdoor");

                var HasilPengaitan = await PengelolaPengguna.AddLoginAsync(
                    PenggunaBaru,
                    InformasiLogin);

                if (!HasilPengaitan.Succeeded)
                {
                    await Transaksi.RollbackAsync(TokenPembatalan);
                    return (null, "Pengaitan akun eksternal gagal");
                }

                await Transaksi.CommitAsync(TokenPembatalan);

                return (PenggunaBaru, null);
            }

            catch (DbUpdateException)
            {
                await Transaksi.RollbackAsync(TokenPembatalan);
                return (null, "Konflik penyimpanan akun eksternal");
            }
        }

        public async Task<(bool Berhasil, string? Kesalahan)> TautkanAsync(
            Guid IdPengguna,
            string Issuer,
            string Subject)
        {
            if (IdPengguna == Guid.Empty ||
                string.IsNullOrWhiteSpace(Issuer) ||
                string.IsNullOrWhiteSpace(Subject))
            {
                return (false, "Identitas pengguna tidak valid");
            }

            var Pengguna = await PengelolaPengguna.FindByIdAsync(
                IdPengguna.ToString());

            if (Pengguna is null)
            {
                return (false, "Akun pengguna tidak ditemukan");
            }

            var NamaPenyedia = $"Casdoor:{Issuer.Trim().TrimEnd('/')}";
            var IdEksternal = Subject.Trim();

            var PenggunaTerhubung =
                await PengelolaPengguna.FindByLoginAsync(
                    NamaPenyedia,
                    IdEksternal);

            if (PenggunaTerhubung is not null)
            {
                if (PenggunaTerhubung.Id == Pengguna.Id)
                {
                    return (true, null);
                }

                return (
                    false,
                    "Identitas Casdoor sudah terhubung dengan akun lain");
            }

            var InformasiLogin = new UserLoginInfo(
                NamaPenyedia,
                IdEksternal,
                "Casdoor");

            try
            {
                var Hasil = await PengelolaPengguna.AddLoginAsync(
                    Pengguna,
                    InformasiLogin);

                if (!Hasil.Succeeded)
                {
                    return (false, "Pengaitan identitas Casdoor gagal");
                }

                return (true, null);
            }
            catch (DbUpdateException)
            {
                return (false, "Konflik penyimpanan identitas eksternal");
            }
        }
    }
}
