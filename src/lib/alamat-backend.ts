/*
 * Access Console
 * Module : Frontend / API Configuration
 *
 * Provides the backend API address for server-side requests
 */

export function dapatkanAlamatBackend(): string | null {
  const alamat = process.env.ACCESS_CONSOLE_API_URL?.trim();

  if (alamat) {
    return alamat;
  }

  return process.env.NODE_ENV === "development"
    ? "http://127.0.0.1:5080"
    : null;
}
