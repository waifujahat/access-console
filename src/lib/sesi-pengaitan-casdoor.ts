/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Authentication
 * Module    : CasdoorLinkingSession
 *
 * Binds an external authentication session to the originating local session
 */

import { createHash, randomUUID } from "node:crypto";
import { cookies, headers } from "next/headers";
import { getToken } from "next-auth/jwt";

function buatSidikToken(Token: string): string {
  return createHash("sha256").update(Token).digest("hex");
}

export function buatPenandaPengaitan(TokenLokal: string): string {
  const Waktu = Math.floor(Date.now() / 1000);

  return `${randomUUID()}.${Waktu}.${buatSidikToken(TokenLokal)}`;
}

export async function bacaTokenPengaitanCasdoor(
  TokenLokal: string,
): Promise<string | null> {
  const PenyimpananCookie = await cookies();

  const Penanda = PenyimpananCookie.get(
    "access_console_casdoor_link_intent",
  )?.value;

  if (!Penanda) {
    return null;
  }

  const [Id, Waktu, SidikToken] = Penanda.split(".");
  const WaktuMulai = Number(Waktu);
  const WaktuSekarang = Math.floor(Date.now() / 1000);

  if (
    !Id ||
    !Number.isSafeInteger(WaktuMulai) ||
    WaktuMulai > WaktuSekarang ||
    WaktuSekarang - WaktuMulai > 600 ||
    SidikToken !== buatSidikToken(TokenLokal)
  ) {
    return null;
  }

  const Rahasia = process.env.AUTH_SECRET;

  if (!Rahasia) {
    return null;
  }

  const MenggunakanHttps = process.env.AUTH_URL
    ? process.env.AUTH_URL.startsWith("https://")
    : process.env.NODE_ENV === "production";

  const NamaCookie = MenggunakanHttps
    ? "__Secure-authjs.session-token"
    : "authjs.session-token";

  const SesiCasdoor = await getToken({
    req: {
      headers: await headers(),
    },
    secret: Rahasia,
    salt: NamaCookie,
    cookieName: NamaCookie,
  });

  if (SesiCasdoor?.casdoorLinkIntent !== Penanda) {
    return null;
  }

  const TokenCasdoor = SesiCasdoor?.casdoorAccessToken;
  const WaktuKedaluwarsa = SesiCasdoor?.casdoorExpiresAt;

  if (typeof TokenCasdoor !== "string" || !TokenCasdoor) {
    return null;
  }

  if (
    typeof WaktuKedaluwarsa === "number" &&
    WaktuKedaluwarsa <= WaktuSekarang
  ) {
    return null;
  }

  return TokenCasdoor;
}
