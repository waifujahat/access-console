/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Pages
 * Module    : HomeRedirect
 *
 * Redirects visitors from the application root
 * to the account login page
 */

import { redirect } from "next/navigation";

export default function HalamanUtama() {
  redirect("/login");
}
