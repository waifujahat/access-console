interface VariabelCasdoor {
    CASDOOR_ISSUER?: string;
    CASDOOR_CLIENT_ID?: string;
    CASDOOR_CLIENT_SECRET?: string;
}

export function bacaKonfigurasiCasdoor(variabel: VariabelCasdoor) {
    const issuer = variabel.CASDOOR_ISSUER?.trim();
    const clientId = variabel.CASDOOR_CLIENT_ID?.trim();
    const clientSecret = variabel.CASDOOR_CLIENT_SECRET;

    if (!issuer || !clientId || !clientSecret?.trim()) {
        throw new Error("Konfigurasi Casdoor belum lengkap");
    }

    let alamat: URL;

    try {
        alamat = new URL(issuer);
    } catch {
        throw new Error("Alamat Casdoor tidak valid");
    }

    const alamatLokal =
        alamat.hostname === "localhost" ||
        alamat.hostname === "127.0.0.1";

    const protokolValid =
        alamat.protocol === "https:" ||
        (alamatLokal && alamat.protocol === "http:");

    if (
        !protokolValid ||
        alamat.username ||
        alamat.password ||
        alamat.search ||
        alamat.hash
    ) {
        throw new Error("Konfigurasi alamat Casdoor tidak diizinkan");
    }

    return {
        issuer,
        clientId,
        clientSecret
    };
}
