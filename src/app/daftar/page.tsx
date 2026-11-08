/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Pages
 * Module    : AccountRegistrationPage
 *
 * Account registration form and handling registration feedback from the ASP.NET Core API
 */

import type { Metadata } from "next";
import Link from "next/link";
import { Suspense } from "react";
import { daftarAkun } from "./daftar-action";
import { TombolFormulir } from "../../components/tombol-formulir";

export const metadata: Metadata = {
  title: "Daftar | Access Console",
  description: "Buat akun baru di Access Console",
  robots: {
    index: false,
    follow: false,
  },
};

type ParameterDaftar = {
  searchParams: Promise<{
    kesalahan?: string | string[];
  }>;
};

const pesanKesalahan: Record<string, string> = {
  wajib: "Semua kolom wajib diisi",
  data: "Data pendaftaran tidak valid atau email sudah digunakan",
  sandi: "Kata sandi harus mengandung huruf besar, huruf kecil, angka dan karakter khusus",
  batas: "Terlalu banyak percobaan, silakan coba kembali nanti",
  konfigurasi: "Layanan pendaftaran belum dikonfigurasi",
  layanan: "Pendaftaran belum dapat diproses, silakan coba kembali",
};

async function PemberitahuanDaftar({
  searchParams,
}: ParameterDaftar) {
  const parameter = await searchParams;

  const kodeKesalahan =
    typeof parameter.kesalahan === "string"
      ? parameter.kesalahan
      : "";

  const pesan = pesanKesalahan[kodeKesalahan];

  if (!pesan) {
    return null;
  }

  return (
    <p
      role="alert"
      className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700"
    >
      {pesan}
    </p>
  );
}

export default function HalamanDaftar({
  searchParams,
}: ParameterDaftar) {
  return (
    <main className="min-h-screen bg-[#f6f8fc] text-slate-900">
      <div className="mx-auto max-w-3xl px-6">
        <header className="border-b border-slate-200 py-6">
            <Link
            href="/login"
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
          aria-labelledby="judul-daftar"
          className="mx-auto max-w-md py-12 sm:py-16"
        >
          <header className="mb-8 text-center">
            <h1
              id="judul-daftar"
              className="text-3xl font-semibold tracking-tight"
            >
              Buat akun
            </h1>
          </header>

          <div className="rounded-2xl border border-slate-200 bg-white p-7 shadow-sm sm:p-8">
            <form
              action={daftarAkun}
              className="space-y-5"
            >
              <Suspense fallback={null}>
                <PemberitahuanDaftar
                  searchParams={searchParams}
                />
              </Suspense>

              <div>
                <label
                  htmlFor="namaPengguna"
                  className="block text-sm font-medium text-slate-700"
                >
                  Nama pengguna
                </label>

                <input
                  id="namaPengguna"
                  name="namaPengguna"
                  type="text"
                  autoComplete="name"
                  placeholder="Masukkan nama pengguna"
                  required
                  maxLength={100}
                  className="mt-2 w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                />
              </div>

              <div>
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
                  autoComplete="email"
                  placeholder="nama@example.com"
                  required
                  maxLength={254}
                  className="mt-2 w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                />
              </div>

              <div>
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
                  autoComplete="new-password"
                  placeholder="Buat kata sandi"
                  aria-describedby="panduan-kata-sandi"
                  required
                  minLength={6}
                  className="mt-2 w-full rounded-lg border border-slate-300 bg-white px-4 py-3 text-sm text-slate-900 outline-none transition focus:border-indigo-500 focus:ring-2 focus:ring-indigo-100"
                />

                <p
                  id="panduan-kata-sandi"
                  className="mt-2 text-xs leading-5 text-slate-500"
                >
                  Minimal 6 karakter, gunakan huruf besar,
                  huruf kecil, angka dan karakter khusus
                </p>
              </div>
                <TombolFormulir
                teks="Buat akun"
                teksMemproses="Proses mendaftarkan..."
                className="w-full rounded-lg bg-indigo-600 px-5 py-3 text-sm font-semibold text-white transition-colors hover:bg-indigo-700 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
                />
            </form>

            <p className="mt-7 text-center text-sm text-slate-500">
              Sudah memiliki akun?{" "}
              <Link
                href="/login"
                className="font-semibold text-indigo-600 hover:text-indigo-700"
              >
                Masuk
              </Link>
            </p>
          </div>
        </section>
      </div>
    </main>
  );
}
