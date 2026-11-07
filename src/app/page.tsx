/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Pages
 * Module    : Home
 *
 * Renders the Access Console landing page and presents project capabilities.
 */

import { KartuFitur } from "@/components/beranda/kartu-fitur";
import { fiturUtama, teknologi } from "@/data/beranda";

export default function HalamanBeranda() {
  return (
    <div className="min-h-screen bg-[#f6f8fc] text-slate-900">
      <div className="mx-auto max-w-7xl px-5 sm:px-8 lg:px-12">
        <header className="flex items-center justify-between border-b border-slate-200 py-6">
          <div className="flex items-center gap-3">
            <div className="flex size-11 items-center justify-center rounded-xl bg-slate-950 text-sm font-bold text-white">
              AC
            </div>

            <div>
              <p className="text-base font-semibold tracking-tight">
                Access Console
              </p>
              <p className="text-xs text-slate-500">
                Identity & Access Management
              </p>
            </div>
          </div>

          <nav
            aria-label="Navigasi utama"
            className="hidden items-center gap-7 text-sm text-slate-600 sm:flex"
          >
            <a href="#fitur" className="hover:text-slate-950">
              Fitur
            </a>
            <a href="#arsitektur" className="hover:text-slate-950">
              Arsitektur
            </a>
            <a
              href="https://github.com/waifujahat/access-console"
              className="hover:text-slate-950"
              target="_blank"
              rel="noopener noreferrer"
            >
              GitHub
            </a>
          </nav>
        </header>

        <main>
          <section className="grid gap-12 py-20 lg:grid-cols-[1.2fr_0.8fr] lg:items-center lg:py-28">
            <div>
              <span className="inline-flex rounded-full border border-indigo-200 bg-indigo-50 px-4 py-2 text-xs font-semibold tracking-wide text-indigo-700">
                PROJECT IN DEVELOPMENT
              </span>

              <h1 className="mt-7 max-w-3xl text-4xl font-semibold leading-tight tracking-tight sm:text-5xl lg:text-6xl">
                Kelola identitas.
                <span className="mt-2 block text-indigo-600">
                  Jaga setiap akses.
                </span>
              </h1>

              <p className="mt-7 max-w-xl text-base leading-8 text-slate-600">
                Access Console adalah proyek full-stack untuk
                membangun sistem autentikasi, otorisasi, dan
                manajemen identitas dengan arsitektur yang
                terstruktur.
              </p>

              <div className="mt-9 flex flex-wrap gap-3">
                <a
                  href="#fitur"
                  className="inline-flex min-h-12 items-center justify-center rounded-xl bg-slate-950 px-6 text-sm font-medium text-white transition-colors hover:bg-slate-800"
                >
                  Jelajahi fitur
                  <span className="ml-3" aria-hidden="true">
                    →
                  </span>
                </a>

                <a
                  href="https://github.com/waifujahat/access-console#4-authentication--account-api"
                  target="_blank"
                  rel="noopener noreferrer"
                  className="inline-flex min-h-12 items-center justify-center rounded-xl border border-slate-200 bg-white px-6 text-sm font-medium text-slate-700 transition-colors hover:bg-slate-100"
                >
                  Dokumentasi API
                </a>
              </div>
            </div>

            <aside
              aria-label="Status pengembangan"
              className="overflow-hidden rounded-2xl border border-slate-200 bg-white shadow-sm"
            >
              <div className="border-b border-slate-100 px-7 py-6">
                <p className="text-xs font-semibold uppercase tracking-widest text-slate-400">
                  Project Overview
                </p>
                <h2 className="mt-2 text-xl font-semibold">
                  Status pengembangan
                </h2>
              </div>

              <div className="space-y-6 p-7">
                <div>
                  <div className="flex items-center justify-between gap-4">
                    <h3 className="text-sm font-semibold">
                      Backend API
                    </h3>
                    <span className="rounded-md bg-emerald-50 px-2.5 py-1 text-xs font-medium text-emerald-700">
                      Implemented
                    </span>
                  </div>
                  <p className="mt-2 text-sm leading-6 text-slate-500">
                    Registrasi, login, dan akses akun terproteksi
                    sudah diimplementasikan
                  </p>
                </div>

                <div className="border-t border-slate-100 pt-6">
                  <div className="flex items-center justify-between gap-4">
                    <h3 className="text-sm font-semibold">
                      Frontend
                    </h3>
                    <span className="rounded-md bg-amber-50 px-2.5 py-1 text-xs font-medium text-amber-700">
                      In Progress
                    </span>
                  </div>
                  <p className="mt-2 text-sm leading-6 text-slate-500">
                    Antarmuka pengguna dan integrasi autentikasi
                    sedang dikembangkan
                  </p>
                </div>

                <div className="border-t border-slate-100 pt-6">
                  <div className="flex items-center justify-between gap-4">
                    <h3 className="text-sm font-semibold">
                      Database
                    </h3>
                    <span className="rounded-md bg-emerald-50 px-2.5 py-1 text-xs font-medium text-emerald-700">
                      Implemented
                    </span>
                  </div>
                  <p className="mt-2 text-sm leading-6 text-slate-500">
                    PostgreSQL dengan Entity Framework Core,
                    Identity, dan database migrations
                  </p>
                </div>
              </div>
            </aside>
          </section>

          <section id="fitur" className="border-t border-slate-200 py-20">
            <div className="mb-10">
              <p className="text-xs font-semibold uppercase tracking-widest text-indigo-600">
                Core Capabilities
              </p>
              <h2 className="mt-3 text-3xl font-semibold tracking-tight">
                Fondasi keamanan aplikasi
              </h2>
              <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-600">
                Fitur berikut sudah tersedia pada backend dan
                akan dihubungkan ke antarmuka pengguna.
              </p>
            </div>

            <div className="grid gap-5 md:grid-cols-3">
              {fiturUtama.map((fitur) => (
                <KartuFitur key={fitur.nomor} fitur={fitur} />
              ))}
            </div>
          </section>

          <section id="arsitektur" className="pb-20">
            <div className="rounded-3xl bg-slate-950 p-8 text-white sm:p-12">
              <p className="text-xs font-semibold uppercase tracking-widest text-indigo-300">
                System Architecture
              </p>

              <h2 className="mt-3 text-3xl font-semibold tracking-tight">
                Teknologi dengan tanggung jawab yang jelas
              </h2>

              <p className="mt-4 max-w-2xl text-sm leading-7 text-slate-300">
                Frontend, API, dan database dipisahkan agar
                pengembangan serta pemeliharaan aplikasi
                lebih terstruktur.
              </p>

              <div className="mt-10 grid gap-4 md:grid-cols-3">
                {teknologi.map((item) => (
                  <div
                    key={item.nama}
                    className="rounded-xl border border-white/15 bg-white/5 p-6"
                  >
                    <p className="text-xs font-medium uppercase tracking-wider text-slate-400">
                      {item.fungsi}
                    </p>
                    <p className="mt-3 text-xl font-semibold">
                      {item.nama}
                    </p>
                  </div>
                ))}
              </div>
            </div>
          </section>
        </main>

        <footer className="flex flex-col gap-3 border-t border-slate-200 py-7 text-sm text-slate-500 sm:flex-row sm:items-center sm:justify-between">
          <p>Access Console · Full-stack Project</p>
          <a
            href="https://github.com/waifujahat"
            target="_blank"
            rel="noopener noreferrer"
            className="hover:text-slate-900"
          >
            Developed by waifujahat
          </a>
        </footer>
      </div>
    </div>
  );
}
