/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Pages
 * Module    : AccountDashboard
 *
 * Displays the authenticated user's account information retrieved securely from the ASP.NET Core API
 */

import type { Metadata } from "next";
import { cookies } from "next/headers";
import Link from "next/link";
import { redirect } from "next/navigation";
import { Suspense, type ReactNode } from "react";
import { dapatkanAlamatBackend } from "../../lib/alamat-backend";
import { bacaTokenPengaitanCasdoor } from "../../lib/sesi-pengaitan-casdoor";
import { keluarAkun } from "./keluar-action";
import { HubungkanCasdoor, KonfirmasiHubungkanCasdoor } from "./hubungkan-casdoor-action";
import { connection } from "next/server";

export const metadata: Metadata = {
  title: "My Account | Access Console",
  description: "Informasi akun pengguna Access Console",
  robots: {
    index: false,
    follow: false,
  },
};

type ParameterHalamanAkun = {
  searchParams: Promise<{
    kesalahan?: string | string[];
    pengaitan?: string | string[];
  }>;
};

type DataAkun = {
  id: string;
  email: string;
  displayName: string;
  createdatUTC: string;
  googleConnected?: boolean | null;
  githubConnected?: boolean | null;
};

function DataAkunValid(Data: unknown): Data is DataAkun {
  if (typeof Data !== "object" || Data === null) {
    return false;
  }

  const Akun = Data as Record<string, unknown>;

  return (
    typeof Akun.id === "string" &&
    typeof Akun.email === "string" &&
    typeof Akun.displayName === "string" &&
    typeof Akun.createdatUTC === "string" &&
    (Akun.googleConnected == null || typeof Akun.googleConnected === "boolean") &&
    (Akun.githubConnected == null || typeof Akun.githubConnected === "boolean")
  );
}

function StatusProvider({
  nama,
  provider,
  terhubung,
  children,
}: {
  nama: string;
  provider: "google" | "github";
  terhubung: boolean | null;
  children: ReactNode;
}) {
  return (
    <div className="flex flex-wrap items-center gap-3 rounded-xl border border-slate-200 px-4 py-3">
      <span className="flex size-10 shrink-0 items-center justify-center rounded-lg bg-slate-50 text-slate-950">
        {children}
      </span>
      <div className="min-w-0 flex-1">
        <p className="text-sm font-semibold text-slate-900">{nama}</p>
        <p className="mt-1 text-xs text-slate-500">
          {terhubung === true
            ? "Akun sudah terhubung"
            : terhubung === false
              ? "Akun belum terhubung"
              : "Status belum terverifikasi"}
        </p>
      </div>
      {terhubung === true ? (
        <span className="rounded-full bg-emerald-50 px-3 py-1 text-xs font-semibold text-emerald-700">
          Terhubung
        </span>
      ) : (
        <button
          type="submit"
          name="provider"
          value={provider}
          aria-label={`${terhubung === false ? "Hubungkan" : "Verifikasi"} ${nama}`}
          className="rounded-lg bg-slate-950 px-4 py-2 text-sm font-medium text-white transition-colors hover:bg-slate-800 focus-visible:outline-2 focus-visible:outline-offset-2 focus-visible:outline-indigo-600"
        >
          {terhubung === false ? "Hubungkan" : "Verifikasi"}
        </button>
      )}
    </div>
  );
}

function PesanStatus({ pesan }: { pesan: string }) {
  return (
    <div
      role="status"
      className="rounded-2xl border border-slate-200 bg-white p-8 text-sm text-slate-600 shadow-sm"
    >
      {pesan}
    </div>
  );
}

async function PemberitahuanKeluar({
  searchParams,
}: ParameterHalamanAkun) {
  const parameter = await searchParams;

  const kodeKesalahan =
    typeof parameter.kesalahan === "string"
      ? parameter.kesalahan
      : "";

  const pesanKesalahan: Record<string, string> = {
    "sandi-casdoor": "Kata sandi wajib diisi",
    "sesi-casdoor": "Verifikasi Casdoor diperlukan kembali",
    "konfigurasi-casdoor": "Konfigurasi Casdoor belum tersedia",
    "koneksi-casdoor": "Layanan Casdoor tidak dapat dihubungi",
    "autentikasi-casdoor": "Konfirmasi akun gagal atau sesi tidak valid",
    "konflik-casdoor": "Identitas Casdoor sudah terhubung ke akun lain",
    "batas-casdoor": "Terlalu banyak percobaan, coba kembali nanti",
    "server-casdoor": "Pengaitan akun belum berhasil",
    "konfigurasi-logout":
      "Layanan logout belum dikonfigurasi",
    "koneksi-logout":
      "Backend tidak dapat dihubungi, sesi belum berhasil diakhiri",
    logout:
      "Logout belum berhasil, coba lagi",
  };

    if (parameter.pengaitan === "berhasil") {
    return (
      <p
        role="status"
        className="mb-6 rounded-xl border border-emerald-200 bg-emerald-50 px-5 py-4 text-sm text-emerald-800"
      >
        Identitas berhasil dihubungkan ke akun Access Console
      </p>
    );
  }

  const pesan = pesanKesalahan[kodeKesalahan];

  if (!pesan) {
    return null;
  }

  return (
    <p
      role="alert"
      className="mb-6 rounded-xl border border-red-200 bg-red-50 px-5 py-4 text-sm text-red-700"
    >
      {pesan}
    </p>
  );
}

async function KontenAkun({
  searchParams,
}: ParameterHalamanAkun) {
  await connection();

  const PenyimpananCookie = await cookies();
  const Token = PenyimpananCookie.get("access_console_token")?.value;

  if (!Token) {
    redirect("/login");
  }

  const Parameter = await searchParams;

  const PerluKonfirmasi =
  Parameter.pengaitan === "konfirmasi" &&
  Boolean(
    await bacaTokenPengaitanCasdoor(Token).catch(() => null),
  );

  const AlamatBackend = dapatkanAlamatBackend();

  if (!AlamatBackend) {
    return (
      <PesanStatus pesan="Konfigurasi layanan akun belum tersedia" />
    );
  }

  let Respons: Response;

  try {
    Respons = await fetch(
      `${AlamatBackend.replace(/\/+$/, "")}/api/accounts/me`,
      {
        headers: {
          Authorization: `Bearer ${Token}`,
        },
        cache: "no-store",
        signal: AbortSignal.timeout(10000),
      },
    );
  } catch {
    return (
      <PesanStatus pesan="Layanan akun belum dapat dihubungi" />
    );
  }

  if (Respons.status === 401 || Respons.status === 403) {
    redirect("/login");
  }

  if (Respons.status === 404) {
    return <PesanStatus pesan="Data akun tidak ditemukan" />;
  }

  if (!Respons.ok) {
    return <PesanStatus pesan="Gagal mengambil informasi akun" />;
  }

  let Data: unknown;

  try {
    Data = await Respons.json();
  } catch {
    return <PesanStatus pesan="Respons layanan akun tidak valid" />;
  }

  if (!DataAkunValid(Data)) {
    return <PesanStatus pesan="Informasi akun tidak valid" />;
  }

  const TanggalDibuat = new Date(Data.createdatUTC);

  const TanggalPendaftaran = Number.isNaN(TanggalDibuat.getTime())
    ? "Tidak tersedia"
    : new Intl.DateTimeFormat("id-ID", {
        dateStyle: "long",
        timeZone: "Asia/Jakarta",
      }).format(TanggalDibuat);

  return (
    <section
      aria-labelledby="judul-ringkasan-akun"
      className="rounded-2xl border border-slate-200 bg-white p-8 shadow-sm"
    >
      <header className="border-b border-slate-100 pb-6">
        <p className="text-xs font-semibold uppercase tracking-widest text-indigo-600">
          Account Overview
        </p>

        <h2
          id="judul-ringkasan-akun"
          className="mt-3 text-2xl font-semibold tracking-tight"
        >
          {Data.displayName}
        </h2>

        <p className="mt-2 text-sm text-slate-500">
          Informasi akun yang terhubung dengan identitas Anda
        </p>
      </header>

      <dl className="mt-7 space-y-6">
        <div>
          <dt className="text-xs font-medium text-slate-500">
            Nama pengguna
          </dt>

          <dd className="mt-2 text-sm font-medium text-slate-900">
            {Data.displayName}
          </dd>
        </div>

        <div>
          <dt className="text-xs font-medium text-slate-500">
            Email
          </dt>

          <dd className="mt-2 break-all text-sm text-slate-900">
            {Data.email}
          </dd>
        </div>

        <div>
          <dt className="text-xs font-medium text-slate-500">
            ID akun
          </dt>

          <dd className="mt-2 break-all font-mono text-xs text-slate-700">
            {Data.id}
          </dd>
        </div>

        <div>
          <dt className="text-xs font-medium text-slate-500">
            Tanggal pendaftaran
          </dt>

          <dd className="mt-2 text-sm text-slate-900">
            {TanggalPendaftaran}
          </dd>
        </div>
      </dl>
      <form action={HubungkanCasdoor} className="mt-8 space-y-3 border-t border-slate-100 pt-6">
        <h3 className="text-sm font-semibold text-slate-900">Connected Accounts</h3>
        <p className="text-sm text-slate-500">
          Identitas terverifikasi yang terhubung dengan akun Anda
        </p>
        <StatusProvider nama="Google" provider="google" terhubung={Data.googleConnected ?? null}>
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

        </StatusProvider>

        <StatusProvider nama="GitHub" provider="github" terhubung={Data.githubConnected ?? null}>
          <svg
            aria-hidden="true"
            className="h-4 w-4"
            viewBox="0 0 24 24"
            fill="currentColor"
          >
            <path d="M12 .75a11.25 11.25 0 0 0-3.56 21.92c.56.1.77-.24.77-.54v-2.1c-3.13.68-3.79-1.33-3.79-1.33-.51-1.3-1.25-1.65-1.25-1.65-1.02-.7.08-.69.08-.69 1.13.08 1.72 1.16 1.72 1.16 1 .1.9-1.57 3.27-.8a2.39 2.39 0 0 1 .71-1.5c-2.5-.28-5.12-1.25-5.12-5.56a4.35 4.35 0 0 1 1.16-3.03 4.05 4.05 0 0 1 .11-2.99s.95-.3 3.1 1.16a10.75 10.75 0 0 1 5.64 0c2.15-1.46 3.1-1.16 3.1-1.16a4.05 4.05 0 0 1 .11 2.99 4.34 4.34 0 0 1 1.16 3.03c0 4.32-2.63 5.27-5.14 5.55.4.35.76 1.03.76 2.08v4.84c0 .3.2.65.77.54A11.25 11.25 0 0 0 12 .75Z" />
          </svg>

        </StatusProvider>
        {(Data.googleConnected == null || Data.githubConnected == null) && (
          <p className="text-xs text-slate-500">
            Masuk ulang melalui Casdoor untuk memperbarui status pengaitan akun.
          </p>
        )}
      </form>

      {PerluKonfirmasi && (
      <form
        action={KonfirmasiHubungkanCasdoor}
        className="mt-6 space-y-4 rounded-xl border border-slate-200 p-5"
      >
        <div>
          <h3 className="text-sm font-semibold text-slate-900">
            Konfirmasi pengaitan akun
          </h3>
        </div>

        <div>
          <label
            htmlFor="kataSandiCasdoor"
            className="block text-sm font-medium text-slate-700"
          >
            Kata sandi Access Console
          </label>

          <input
            id="kataSandiCasdoor"
            name="kataSandi"
            type="password"
            autoComplete="current-password"
            required
            className="mt-2 h-11 w-full rounded-lg border border-slate-300 px-4 text-sm text-slate-900 outline-none focus:border-indigo-500"
          />
        </div>

        <button
          type="submit"
          className="rounded-lg bg-indigo-600 px-5 py-3 text-sm font-medium text-white transition-colors hover:bg-indigo-700"
        >
          Konfirmasi dan hubungkan akun
        </button>
      </form>
    )}

      <form action={keluarAkun} className="mt-8">
        <button
          type="submit"
          className="inline-flex items-center justify-center rounded-lg border border-zinc-300 px-5 py-2.5 text-sm font-medium text-zinc-800 transition-colors hover:bg-zinc-100"
        >
          Keluar
        </button>
      </form>
    </section>
  );
}

export default function HalamanAkun({
  searchParams,
}: ParameterHalamanAkun) {
  return (
    <main className="min-h-screen bg-[#f6f8fc] text-slate-900">
      <div className="mx-auto max-w-3xl px-6">
        <header className="border-b border-slate-200 py-6">
          <Link href="/" className="inline-flex items-center gap-3">
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
          aria-label="Informasi akun pribadi"
          className="py-12 sm:py-16"
        >
          <Suspense fallback={null}>
            <PemberitahuanKeluar
              searchParams={searchParams}
            />
          </Suspense>

          <Suspense
            fallback={<PesanStatus pesan="Memuat informasi akun" />}
          >
            <KontenAkun searchParams={searchParams} />
          </Suspense>
        </section>
      </div>
    </main>
  );
}
