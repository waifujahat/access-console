/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Authentication
 * Module    : CasdoorLinkingCompletion
 *
 * Completes external account linking using recent authentication
 */

import { connection, NextRequest, NextResponse } from "next/server";
import { dapatkanAlamatBackend } from "../../../lib/alamat-backend";
import { bacaTokenPengaitanCasdoor } from "../../../lib/sesi-pengaitan-casdoor";

export async function GET(Permintaan: NextRequest) {
  await connection();

  const Kembali = (
    Jalur: string,
    HapusPenanda = true,
  ) => {
    const Respons = NextResponse.redirect(
      new URL(Jalur, Permintaan.url),
    );

    Respons.headers.set("Cache-Control", "no-store");

    if (HapusPenanda) {
      Respons.cookies.delete("access_console_casdoor_link_intent");
    }

    return Respons;
  };

  const TokenLokal = Permintaan.cookies
    .get("access_console_token")?.value;

  if (!TokenLokal) {
    return Kembali("/login");
  }

  let TokenCasdoor: string | null;

  try {
    TokenCasdoor = await bacaTokenPengaitanCasdoor(TokenLokal);
  } catch {
    return Kembali("/akun?kesalahan=sesi-casdoor");
  }

  if (!TokenCasdoor) {
    return Kembali("/akun?kesalahan=sesi-casdoor");
  }

  const AlamatBackend = dapatkanAlamatBackend();

  if (!AlamatBackend) {
    return Kembali("/akun?kesalahan=konfigurasi-casdoor");
  }

  let Respons: Response;

  try {
    Respons = await fetch(
      `${AlamatBackend.replace(/\/+$/, "")}/api/auth/casdoor/link`,
      {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${TokenLokal}`,
          "X-Casdoor-Token": TokenCasdoor,
        },
        body: JSON.stringify({
          kataSandi: null,
        }),
        cache: "no-store",
        signal: AbortSignal.timeout(10000),
      },
    );
  } catch {
    return Kembali("/akun?kesalahan=koneksi-casdoor");
  }

  console.info("Hasil pengaitan Casdoor:", Respons.status);

  switch (Respons.status) {
    case 204:
      return Kembali("/akun?pengaitan=berhasil");

    case 428:
      return Kembali("/akun?pengaitan=konfirmasi", false);

    case 401:
    case 403:
      return Kembali("/akun?kesalahan=autentikasi-casdoor");

    case 409:
      return Kembali("/akun?kesalahan=konflik-casdoor");

    case 429:
      return Kembali("/akun?kesalahan=batas-casdoor");

    default:
      return Kembali("/akun?kesalahan=server-casdoor");
  }
}
