/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Authentication
 * Module    : CasdoorLoginCompletion
 *
 * Exchanges a verified Casdoor identity for an Access Console session without exposing tokens
 */

import { getToken } from "next-auth/jwt";
import { NextRequest, NextResponse } from "next/server";
import { dapatkanAlamatBackend } from "../../../lib/alamat-backend";

export async function GET(Permintaan: NextRequest) {
  const KembaliKeLogin = (Kesalahan: string) =>
    NextResponse.redirect(
      new URL(
        `/login?kesalahan=${Kesalahan}`,
        Permintaan.url,
      ),
    );

  if (Permintaan.cookies.has("access_console_token")) {
    return NextResponse.redirect(
      new URL("/akun", Permintaan.url),
    );
  }

  const Rahasia = process.env.AUTH_SECRET;

  if (!Rahasia) {
    return KembaliKeLogin("konfigurasi");
  }

  const MenggunakanHttps = process.env.AUTH_URL
    ? process.env.AUTH_URL.startsWith("https://")
    : process.env.NODE_ENV === "production";

  const NamaCookie = MenggunakanHttps
    ? "__Secure-authjs.session-token"
    : "authjs.session-token";

  let SesiCasdoor;

  try {
    SesiCasdoor = await getToken({
      req: Permintaan,
      secret: Rahasia,
      salt: NamaCookie,
      cookieName: NamaCookie,
    });
  } catch {
    return KembaliKeLogin("sesi-casdoor");
  }

    const PenandaLogin = Permintaan.cookies
    .get("access_console_casdoor_intent")?.value;

  if (
    !PenandaLogin ||
    SesiCasdoor?.casdoorLoginIntent !== PenandaLogin
  ) {
    return KembaliKeLogin("sesi-casdoor");
  }

  const TokenCasdoor = SesiCasdoor?.casdoorAccessToken;
  const WaktuKedaluwarsa = SesiCasdoor?.casdoorExpiresAt;

  if (typeof TokenCasdoor !== "string" || !TokenCasdoor) {
    return KembaliKeLogin("sesi-casdoor");
  }

  if (
    typeof WaktuKedaluwarsa === "number" &&
    WaktuKedaluwarsa <= Math.floor(Date.now() / 1000)
  ) {
    return KembaliKeLogin("sesi-casdoor");
  }

  const AlamatBackend = dapatkanAlamatBackend();

  if (!AlamatBackend) {
    return KembaliKeLogin("konfigurasi");
  }

  let Respons: Response;

  try {
    Respons = await fetch(
      `${AlamatBackend.replace(/\/+$/, "")}/api/auth/casdoor/exchange`,
      {
        method: "POST",
        headers: {
          Authorization: `Bearer ${TokenCasdoor}`,
        },
        cache: "no-store",
        signal: AbortSignal.timeout(10000),
      },
    );
  } catch {
    return KembaliKeLogin("koneksi");
  }

  if (Respons.status === 409) {
    return KembaliKeLogin("tautkan-casdoor");
  }

  if (Respons.status === 401 || Respons.status === 403) {
    return KembaliKeLogin("sesi-casdoor");
  }

  if (!Respons.ok) {
    return KembaliKeLogin("server");
  }

  let Data: unknown;

  try {
    Data = await Respons.json();
  } catch {
    return KembaliKeLogin("server");
  }

  if (
    typeof Data !== "object" ||
    Data === null ||
    !("accessToken" in Data) ||
    typeof Data.accessToken !== "string" ||
    !Data.accessToken ||
    !("expiresIn" in Data) ||
    typeof Data.expiresIn !== "number" ||
    !Number.isFinite(Data.expiresIn) ||
    Data.expiresIn <= 0
  ) {
    return KembaliKeLogin("server");
  }

  const ResponsAkhir = NextResponse.redirect(
    new URL("/akun", Permintaan.url),
  );

  ResponsAkhir.cookies.set(
    "access_console_token",
    Data.accessToken,
    {
      httpOnly: true,
      secure: process.env.NODE_ENV === "production",
      sameSite: "lax",
      path: "/",
      maxAge: Math.min(Math.floor(Data.expiresIn), 3600),
    },
  );
    ResponsAkhir.cookies.delete("access_console_casdoor_intent");

    ResponsAkhir.headers.set("Cache-Control", "no-store");

    return ResponsAkhir;
}
