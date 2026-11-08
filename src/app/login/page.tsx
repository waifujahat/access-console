/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Pages
 * Module    : Authentication / Login
 *
 * Renders the account login form and displays
 * authentication and registration feedback
 */

import type { Metadata } from "next";
import Link from "next/link";
import { Suspense } from "react";
import { masukAkun } from "./login-action";
import { TombolFormulir } from "../../components/tombol-formulir";

export const metadata: Metadata = {
  title: "Login | Access Console",
  description: "Masuk ke akun Access Console",
  robots: {
    index: false,
    follow: false,
  },
};

type PropertiHalaman = {
  searchParams: Promise<{
    kesalahan?: string | string[];
    pendaftaran?: string | string[];
  }>;
};

const PesanKesalahan: Record<string, string> = {
  input: "Email dan kata sandi wajib diisi",
  kredensial: "Email atau kata sandi tidak sesuai",
  batas: "Terlalu banyak percobaan masuk. Coba lagi nanti",
  koneksi: "Layanan autentikasi tidak dapat dihubungi",
  konfigurasi: "Layanan autentikasi belum dikonfigurasi",
  server: "Terjadi gangguan saat masuk. Coba lagi nanti",
};

async function PemberitahuanMasuk({
  searchParams,
}: PropertiHalaman) {
  const Parameter = await searchParams;

  const KodeKesalahan = Array.isArray(Parameter.kesalahan)
    ? Parameter.kesalahan[0]
    : Parameter.kesalahan;

  if (KodeKesalahan) {
    const Kesalahan =
      PesanKesalahan[KodeKesalahan] ??
      "Gagal masuk. Silakan coba lagi";

    return (
      <p
        role="alert"
        className="rounded-xl border border-red-200 bg-red-50 px-4 py-3 text-sm leading-6 text-red-700"
      >
        {Kesalahan}
      </p>
    );
  }

  if (Parameter.pendaftaran === "berhasil") {
    return (
      <p
        role="status"
        className="rounded-xl border border-emerald-200 bg-emerald-50 px-4 py-3 text-sm leading-6 text-emerald-800"
      >
        Akun berhasil dibuat, masuk menggunakan akun yang baru didaftarkan
      </p>
    );
  }

  return null;
}

function FormulirMasuk({
  searchParams,
}: PropertiHalaman) {
  return (
    <form action={masukAkun} className="mt-8 space-y-5">
      <Suspense fallback={null}>
        <PemberitahuanMasuk searchParams={searchParams} />
      </Suspense>

      <section className="space-y-2">
        <label
          htmlFor="email"
          className="block text-sm font-medium text-slate-700"
        >
          Email
        </label>

        <input
          id="email"
          name="email"
          type="email"
          autoComplete="username"
          placeholder="nama@contoh.com"
          required
          className="h-12 w-full rounded-xl border border-slate-200 bg-white px-4 text-sm text-slate-900 outline-none transition-colors placeholder:text-slate-400 focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
        />
      </section>

      <section className="space-y-2">
        <label
          htmlFor="kataSandi"
          className="block text-sm font-medium text-slate-700"
        >
          Kata sandi
        </label>

        <input
          id="kataSandi"
          name="kataSandi"
          type="password"
          autoComplete="current-password"
          minLength={6}
          required
          className="h-12 w-full rounded-xl border border-slate-200 bg-white px-4 text-sm text-slate-900 outline-none transition-colors focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
        />
      </section>

      <TombolFormulir
        teks="Masuk"
        teksMemproses="Please wait..."
        className="flex h-12 w-full items-center justify-center rounded-xl bg-slate-950 px-5 text-sm font-medium text-white transition-colors hover:bg-slate-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
      />
    </form>
  );
}

export default function HalamanMasuk({
  searchParams,
}: PropertiHalaman) {
  return (
    <main className="flex min-h-screen flex-col bg-[#f6f8fc] text-slate-900">
      <header className="mx-auto w-full max-w-6xl px-6 py-6">
        <Link
          href="/"
          className="inline-flex items-center gap-3"
        >
          <span
            aria-hidden="true"
            className="flex size-10 items-center justify-center rounded-xl bg-slate-950 text-sm font-semibold text-white"
          >
            W
          </span>

          <span className="font-semibold tracking-tight">
            Access Console
          </span>
        </Link>
      </header>

      <section
        aria-labelledby="judul-masuk"
        className="flex flex-1 items-center justify-center px-6 pb-20"
      >
        <div className="w-full max-w-sm rounded-2xl border border-slate-200 bg-white p-8 shadow-sm">
          <h1
            id="judul-masuk"
            className="text-2xl font-semibold tracking-tight"
          >
            Masuk
          </h1>

          <p className="mt-3 text-sm leading-6 text-slate-500">
            Gunakan akun yang sudah terdaftar untuk melanjutkan
          </p>

          <FormulirMasuk searchParams={searchParams} />

          <footer className="mt-7 border-t border-slate-100 pt-6 text-center">
            <p className="text-sm text-slate-500">
              Belum memiliki akun?{" "}
              <Link
                href="/daftar"
                className="font-semibold text-indigo-600 transition-colors hover:text-indigo-700"
              >
                Daftar akun
              </Link>
            </p>
          </footer>
        </div>
      </section>
    </main>
  );
}
