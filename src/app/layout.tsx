/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.App
 * Module    : RootLayout
 *
 * Defines the document shell, fonts and metadata shared by all pages
 */

import type { Metadata } from "next";
import { Geist_Mono } from "next/font/google";
import "./globals.css";

const geistMono = Geist_Mono({
  variable: "--font-geist-mono",
  subsets: ["latin"],
  preload: false,
});

export const metadata: Metadata = {
  title: "Access Console | Identity & Access Management",
  description:
    "Frontend Access Console",
};

export default function RootLayout({ children }: LayoutProps<"/">) {
  return (
    <html
      lang="id"
      className={`${geistMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col">
        {children}
      </body>
    </html>
  );
}
