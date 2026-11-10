/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Authentication
 * Module    : LoginAction
 *
 * Handles account authentication through ASP.NET Core API and stores the access token in an HTTP-only cookie
 */

"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { dapatkanAlamatBackend } from "../../lib/alamat-backend";

export async function masukAkun(Formulir: FormData) {
  const Email = Formulir.get("email");
  const KataSandi = Formulir.get("kataSandi");

  if (
    typeof Email !== "string" ||
    typeof KataSandi !== "string" ||
    !Email.trim() ||
    !KataSandi
  ) {
    redirect("/login?kesalahan=input");
  }

  const AlamatBackend = dapatkanAlamatBackend();

  if (!AlamatBackend) {
    redirect("/login?kesalahan=konfigurasi");
  }

  let Respons: Response;

  try {
    Respons = await fetch(
      `${AlamatBackend.replace(/\/+$/, "")}/api/auth/login`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
        },
        body: JSON.stringify({
          email: Email.trim(),
          kataSandi: KataSandi,
        }),
        cache: "no-store",
        signal: AbortSignal.timeout(10000),
      },
    );
  } catch {
    redirect("/login?kesalahan=koneksi");
  }

  if (Respons.status === 401) {
    redirect("/login?kesalahan=kredensial");
  }

  if (Respons.status === 429) {
    redirect("/login?kesalahan=batas");
  }

  if (!Respons.ok) {
    redirect("/login?kesalahan=server");
  }

  let DataToken: unknown;

  try {
    DataToken = await Respons.json();
  } catch {
    redirect("/login?kesalahan=server");
  }

  if (
    typeof DataToken !== "object" ||
    DataToken === null ||
    !("accessToken" in DataToken) ||
    typeof DataToken.accessToken !== "string" ||
    !DataToken.accessToken ||
    !("expiresIn" in DataToken) ||
    typeof DataToken.expiresIn !== "number" ||
    !Number.isFinite(DataToken.expiresIn) ||
    DataToken.expiresIn < 1
  ) {
    redirect("/login?kesalahan=server");
  }

  const DurasiSesi = Math.min(
    Math.floor(DataToken.expiresIn),
    3600,
  );

  const PenyimpananCookie = await cookies();

  PenyimpananCookie.set(
    "access_console_token",
    DataToken.accessToken,
    {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      sameSite: "lax",
      path: "/",
      maxAge: DurasiSesi,
    },
  );

  redirect("/akun");
}
