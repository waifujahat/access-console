/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Components.Home
 * Module    : FeatureCard
 *
 * Displays a single core capability on the home page.
 */

import type { FiturBeranda } from "@/data/beranda";

type PropertiKartuFitur = {
  fitur: FiturBeranda;
};

export function KartuFitur({ fitur }: PropertiKartuFitur) {
  return (
    <article className="rounded-2xl border border-slate-200 bg-white p-7 shadow-sm">
      <span className="text-sm font-semibold text-indigo-600">
        {fitur.nomor}
      </span>

      <h3 className="mt-6 text-lg font-semibold">{fitur.judul}</h3>

      <p className="mt-3 text-sm leading-7 text-slate-600">
        {fitur.deskripsi}
      </p>
    </article>
  );
}
