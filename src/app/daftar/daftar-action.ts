/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Actions
 * Module    : AccountRegistration
 *
 * Handles account registration by validating user input and sending registration requests to the ASP.NET Core API
 */

"use server";

import { redirect } from "next/navigation";

export async function daftarAkun(formulir: FormData): Promise<void> {
  const namaPengguna = formulir.get("namaPengguna");
  const email = formulir.get("email");
  const kataSandi = formulir.get("kataSandi");

  if (
    typeof namaPengguna !== "string" ||
    typeof email !== "string" ||
    typeof kataSandi !== "string"
  ) {
    redirect("/daftar?kesalahan=wajib");
  }

  const namaBersih = namaPengguna.trim();
  const emailBersih = email.trim();

  if (!namaBersih || !emailBersih || !kataSandi) {
    redirect("/daftar?kesalahan=wajib");
  }

  if (
    namaBersih.length > 100 ||
    emailBersih.length > 254 ||
    !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(emailBersih)
  ) {
    redirect("/daftar?kesalahan=data");
  }

  const kataSandiValid =
    kataSandi.length >= 6 &&
    /[A-Z]/.test(kataSandi) &&
    /[a-z]/.test(kataSandi) &&
    /[0-9]/.test(kataSandi) &&
    /[^A-Za-z0-9]/.test(kataSandi);

  if (!kataSandiValid) {
    redirect("/daftar?kesalahan=sandi");
  }

  const alamatBackend =
    process.env.ACCESS_CONSOLE_API_URL?.trim() ||
    (process.env.NODE_ENV === "development"
      ? "http://127.0.0.1:5080"
      : null);

  if (!alamatBackend) {
    redirect("/daftar?kesalahan=konfigurasi");
  }

  let respons: Response;

  try {
    respons = await fetch(
      `${alamatBackend.replace(/\/+$/, "")}/api/auth/register`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          displayName: namaBersih,
          email: emailBersih,
          kataSandi,
        }),
        cache: "no-store",
        signal: AbortSignal.timeout(10000),
      },
    );
  } catch {
    redirect("/daftar?kesalahan=layanan");
  }

  if (respons.status === 201) {
    redirect("/login?pendaftaran=berhasil");
  }

  if (respons.status === 429) {
    redirect("/daftar?kesalahan=batas");
  }

  if (respons.status === 400 || respons.status === 409) {
    redirect("/daftar?kesalahan=data");
  }

  redirect("/daftar?kesalahan=layanan");
}
