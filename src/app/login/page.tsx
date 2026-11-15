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
import { MasukCasdoor } from "./casdoor-action";
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
          placeholder="nama@mail.com"
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
            className="text-center text-2xl font-semibold tracking-tight"
          >
            Login
          </h1>

          <p className="mt-3 text-center text-sm leading-6 text-slate-500">
            Identity and Access Management
          </p>

          <FormulirMasuk searchParams={searchParams} />

                    <div className="my-6 flex items-center gap-4">
            <div className="h-px flex-1 bg-slate-200" />

            <span className="text-xs text-slate-500">
              atau lanjutkan dengan
            </span>

            <div className="h-px flex-1 bg-slate-200" />
          </div>

          <form action={MasukCasdoor} className="space-y-3">
            <button
              type="submit"
              name="provider"
              value="google"
              className="flex h-12 w-full items-center justify-center gap-2 rounded-xl bg-slate-950 px-5 text-sm font-medium text-white transition-colors hover:bg-slate-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
            >
              <svg
                aria-hidden="true"
                className="h-4 w-4"
                viewBox="0 0 48 48"
              >
                <path
                  fill="#4285F4"
                  d="M43.61 24.46c0-1.36-.12-2.66-.35-3.92H24v7.43h11.01a9.42 9.42 0 0 1-4.08 6.2v5.15h6.61c3.87-3.57 6.07-8.83 6.07-14.86Z"
                />
                <path
                  fill="#34A853"
                  d="M24 44c5.51 0 10.13-1.83 13.54-4.68l-6.61-5.15c-1.83 1.23-4.17 1.96-6.93 1.96-5.33 0-9.85-3.6-11.47-8.44H5.71v5.31A20 20 0 0 0 24 44Z"
                />
                <path
                  fill="#FBBC05"
                  d="M12.53 27.69a12 12 0 0 1 0-7.38V15H5.71a20 20 0 0 0 0 18l6.82-5.31Z"
                />
                <path
                  fill="#EA4335"
                  d="M24 11.87c3 0 5.69 1.03 7.81 3.06l5.86-5.86C34.12 5.77 29.5 4 24 4A20 20 0 0 0 5.71 15l6.82 5.31C14.15 15.47 18.67 11.87 24 11.87Z"
                />
              </svg>

              Masuk dengan Google
            </button>

            <button
              type="submit"
              name="provider"
              value="github"
              className="flex h-12 w-full items-center justify-center gap-2 rounded-xl bg-slate-950 px-5 text-sm font-medium text-white transition-colors hover:bg-slate-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
            >
              <svg
                aria-hidden="true"
                className="h-4 w-4"
                viewBox="0 0 24 24"
                fill="currentColor"
              >
                <path d="M12 .75a11.25 11.25 0 0 0-3.56 21.92c.56.1.77-.24.77-.54v-2.1c-3.13.68-3.79-1.33-3.79-1.33-.51-1.3-1.25-1.65-1.25-1.65-1.02-.7.08-.69.08-.69 1.13.08 1.72 1.16 1.72 1.16 1 .1.9-1.57 3.27-.8a2.39 2.39 0 0 1 .71-1.5c-2.5-.28-5.12-1.25-5.12-5.56a4.35 4.35 0 0 1 1.16-3.03 4.05 4.05 0 0 1 .11-2.99s.95-.3 3.1 1.16a10.75 10.75 0 0 1 5.64 0c2.15-1.46 3.1-1.16 3.1-1.16a4.05 4.05 0 0 1 .11 2.99 4.34 4.34 0 0 1 1.16 3.03c0 4.32-2.63 5.27-5.14 5.55.4.35.76 1.03.76 2.08v4.84c0 .3.2.65.77.54A11.25 11.25 0 0 0 12 .75Z" />
              </svg>

              Masuk dengan GitHub
            </button>
          </form>

          <footer className="mt-4 text-center">
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
