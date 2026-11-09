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
import { Suspense } from "react";
import { keluarAkun } from "./keluar-action";

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
  }>;
};

type DataAkun = {
  id: string;
  email: string;
  displayName: string;
  createdatUTC: string;
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
    typeof Akun.createdatUTC === "string"
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
    "konfigurasi-logout":
      "Layanan logout belum dikonfigurasi",
    "koneksi-logout":
      "Backend tidak dapat dihubungi, sesi belum berhasil diakhiri",
    logout:
      "Logout belum berhasil, coba lagi",
  };

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

async function KontenAkun() {
  const PenyimpananCookie = await cookies();
  const Token = PenyimpananCookie.get("access_console_token")?.value;

  if (!Token) {
    redirect("/login");
  }

  const AlamatBackend =
    process.env.ACCESS_CONSOLE_API_URL?.trim() ||
    (process.env.NODE_ENV === "development"
      ? "http://127.0.0.1:5080"
      : null);

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
            <KontenAkun />
          </Suspense>
        </section>
      </div>
    </main>
  );
}
