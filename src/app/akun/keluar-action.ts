/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Actions
 * Module    : AccountLogout
 *
 * Handles user logout by removing the authentication cookie and redirecting the user to the login page
 */

"use server";

import { cookies } from "next/headers";
import { redirect } from "next/navigation";

export async function keluarAkun(): Promise<void> {
  const penyimpananCookie = await cookies();

  penyimpananCookie.delete("access_console_token");

  redirect("/login");
}
