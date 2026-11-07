/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Data
 * Module    : Home
 *
 * Defines the static features and technology information shown on the home page.
 */

export type FiturBeranda = Readonly<{
  nomor: string;
  judul: string;
  deskripsi: string;
}>;

export type TeknologiBeranda = Readonly<{
  nama: string;
  fungsi: string;
}>;

export const fiturUtama: ReadonlyArray<FiturBeranda> = [
  {
    nomor: "01",
    judul: "Account Management",
    deskripsi:
      "Registrasi akun dengan validasi data, kebijakan kata sandi dan penyimpanan identitas",
  },
  {
    nomor: "02",
    judul: "Authentication",
    deskripsi:
      "Autentikasi menggunakan ASP.NET Core Identity dan Bearer Token",
  },
  {
    nomor: "03",
    judul: "Access Protection",
    deskripsi:
      "Otorisasi kepemilikan akun, pembatasan permintaan, dan perlindungan login",
  },
];

export const teknologi: ReadonlyArray<TeknologiBeranda> = [
  { nama: "Next.js", fungsi: "Frontend" },
  { nama: "ASP.NET Core", fungsi: "Backend API" },
  { nama: "PostgreSQL", fungsi: "Database" },
];
