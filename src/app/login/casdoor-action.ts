/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Authentication
 * Module    : CasdoorLoginAction
 *
 * Initiates external authentication through Casdoor
 */

"use server";

import { cookies } from "next/headers";
import { signIn } from "../../auth";

export async function MasukCasdoor(Formulir: FormData): Promise<void> {
  const Provider = Formulir.get("provider");

  const NamaProvider =
    Provider === "google"
      ? process.env.CASDOOR_GOOGLE_PROVIDER?.trim()
      : Provider === "github"
        ? process.env.CASDOOR_GITHUB_PROVIDER?.trim()
        : undefined;

  if (!NamaProvider) {
    throw new Error("Provider Casdoor tidak valid");
  }

  const PenyimpananCookie = await cookies();

  PenyimpananCookie.delete("access_console_casdoor_link_intent");

  PenyimpananCookie.set(
    "access_console_casdoor_intent",
    crypto.randomUUID(),
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
      redirectTo: "/auth/selesaikan",
    },
    {
      provider_hint: NamaProvider,
    },
  );
}
