/*
 * Access Console
 * Author    : waifujahat
 * Namespace : Waifujahat.AccessConsole.Frontend.Components
 * Module    : FormSubmitButton
 *
 * Provides reusable submit button that displays a pending state during form submission
 */

"use client";

import { useFormStatus } from "react-dom";

type PropertiTombolFormulir = {
  teks: string;
  teksMemproses: string;
  className: string;
};

export function TombolFormulir({
  teks,
  teksMemproses,
  className,
}: PropertiTombolFormulir) {
  const { pending } = useFormStatus();

  return (
    <button
      type="submit"
      disabled={pending}
      aria-busy={pending}
      className={`${className} disabled:cursor-wait disabled:opacity-60`}
    >
      {pending ? teksMemproses : teks}
    </button>
  );
}
