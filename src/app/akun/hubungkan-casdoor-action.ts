/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Actions
 * Module    : CasdoorAccountLinking
 *
 * Verifies an external identity and links it to an authenticated Access Console account
 */

"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";
import { signIn } from "../../auth";
import { dapatkanAlamatBackend } from "../../lib/alamat-backend";
import { buatPenandaPengaitan, bacaTokenPengaitanCasdoor,} from "../../lib/sesi-pengaitan-casdoor";

export async function HubungkanCasdoor(
  Formulir: FormData,
): Promise<void> {
  const Provider = Formulir.get("provider");

  const NamaProvider =
    Provider === "google"
      ? process.env.CASDOOR_GOOGLE_PROVIDER?.trim()
      : Provider === "github"
        ? process.env.CASDOOR_GITHUB_PROVIDER?.trim()
        : undefined;

  if (!NamaProvider) {
    redirect("/akun?kesalahan=konfigurasi-casdoor");
  }

  const PenyimpananCookie = await cookies();
  const Token = PenyimpananCookie.get("access_console_token")?.value;

  if (!Token) {
    redirect("/login");
  }

  const AlamatBackend = dapatkanAlamatBackend();

  if (!AlamatBackend) {
    redirect("/akun?kesalahan=konfigurasi-casdoor");
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
    redirect("/akun?kesalahan=koneksi-casdoor");
  }

  if (Respons.status === 401 || Respons.status === 403) {
    redirect("/login");
  }

  if (!Respons.ok) {
    redirect("/akun?kesalahan=koneksi-casdoor");
  }

  PenyimpananCookie.delete("access_console_casdoor_intent");

  PenyimpananCookie.set(
  "access_console_casdoor_link_intent",
    buatPenandaPengaitan(Token),
    {
        httpOnly: true,
        secure: process.env.NODE_ENV === "production",
        sameSite: "lax",
        path: "/",
        maxAge: 600,
    },
  );

    await signIn(
    "casdoor",
    {
        redirectTo: "/auth/tautkan",
    },
    {
        provider_hint: NamaProvider,
    },
  );
}

export async function KonfirmasiHubungkanCasdoor(
  Formulir: FormData,
): Promise<void> {
  const KataSandi = Formulir.get("kataSandi");

  if (typeof KataSandi !== "string" || !KataSandi) {
  redirect("/akun?pengaitan=konfirmasi&kesalahan=sandi-casdoor");
  }

  const PenyimpananCookie = await cookies();
  const TokenLokal =
    PenyimpananCookie.get("access_console_token")?.value;

  if (!TokenLokal) {
    redirect("/login");
  }

  const TokenCasdoor = await bacaTokenPengaitanCasdoor(TokenLokal);

  if (!TokenCasdoor) {
    redirect("/akun?kesalahan=sesi-casdoor");
  }

  const AlamatBackend = dapatkanAlamatBackend();

  if (!AlamatBackend) {
    redirect("/akun?kesalahan=konfigurasi-casdoor");
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
          kataSandi: KataSandi,
        }),
        cache: "no-store",
        signal: AbortSignal.timeout(10000),
      },
    );
  } catch {
    redirect("/akun?kesalahan=koneksi-casdoor");
  }

  if (Respons.status === 204) {
  PenyimpananCookie.delete("access_console_casdoor_link_intent");
  redirect("/akun?pengaitan=berhasil");
  }

  if (Respons.status === 401 || Respons.status === 403) {
  redirect("/akun?pengaitan=konfirmasi&kesalahan=autentikasi-casdoor");
  }

  if (Respons.status === 409) {
    redirect("/akun?kesalahan=konflik-casdoor");
  }

  if (Respons.status === 429) {
    redirect("/akun?kesalahan=batas-casdoor");
  }

  redirect("/akun?kesalahan=server-casdoor");
}
