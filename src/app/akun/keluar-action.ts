/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Actions
 * Module    : AccountLogout
 *
 * Revokes access token through the backend before clearing authentication cookie
 */

"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";

export async function keluarAkun(): Promise<void> {
  const penyimpananCookie = await cookies();
  const token = penyimpananCookie.get("access_console_token")?.value;

  if (!token) {
    redirect("/login");
  }

  const alamatBackend =
    process.env.ACCESS_CONSOLE_API_URL?.trim() ||
    (process.env.NODE_ENV === "development"
      ? "http://127.0.0.1:5080"
      : null);

  if (!alamatBackend) {
    redirect("/akun?kesalahan=konfigurasi-logout");
  }

  let respons: Response;

  try {
    respons = await fetch(
      `${alamatBackend.replace(/\/+$/, "")}/api/auth/logout`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${token}`,
        },
        cache: "no-store",
        signal: AbortSignal.timeout(10000),
      },
    );
  } catch {
    redirect("/akun?kesalahan=koneksi-logout");
  }

  if (respons.status !== 204 && respons.status !== 401) {
    redirect("/akun?kesalahan=logout");
  }

  penyimpananCookie.delete("access_console_token");

  redirect("/login");
}
