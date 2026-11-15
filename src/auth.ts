/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Authentication
 * Module    : CasdoorOIDC
 *
 * Configures external authentication using Casdoor and maintains provider credentials in the server-side session
 */

import NextAuth from "next-auth";
import { cookies } from "next/headers";
import { bacaKonfigurasiCasdoor } from "./lib/konfigurasi-casdoor";

const konfigurasiCasdoor = bacaKonfigurasiCasdoor({
  CASDOOR_ISSUER: process.env.CASDOOR_ISSUER,
  CASDOOR_CLIENT_ID: process.env.CASDOOR_CLIENT_ID,
  CASDOOR_CLIENT_SECRET: process.env.CASDOOR_CLIENT_SECRET,
});

export const { auth, handlers, signIn, signOut } = NextAuth({
  providers: [
    {
      id: "casdoor",
      name: "Access Console",
      type: "oidc",
      issuer: konfigurasiCasdoor.issuer,
      clientId: konfigurasiCasdoor.clientId,
      clientSecret: konfigurasiCasdoor.clientSecret,
      authorization: {
        params: {
          scope: "openid profile email",
        },
      },
    },
  ],

  callbacks: {
    async jwt({ token, account }) {
      if (account?.provider === "casdoor") {
        if (!account.access_token) {
          throw new Error("Access token Casdoor tidak tersedia");
        }

        token.casdoorAccessToken = account.access_token;
        token.casdoorExpiresAt = account.expires_at ?? null;

        const PenyimpananCookie = await cookies();

        token.casdoorLoginIntent =
        PenyimpananCookie.get("access_console_casdoor_intent")?.value ?? null;

        token.casdoorLinkIntent =
        PenyimpananCookie.get("access_console_casdoor_link_intent")?.value ?? null;
      }

      return token;
    },
  },

  session: {
    strategy: "jwt",
    maxAge: 8 * 60 * 60,
  },
});
