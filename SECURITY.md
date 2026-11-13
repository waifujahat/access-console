# Security Policy

Access Console merupakan proyek demonstrasi Identity and Access Management. Temuan keamanan tetap ditangani secara serius, tetapi proyek ini tidak menyediakan jaminan atau SLA perbaikan untuk penggunaan production.

## Versi yang didukung

Perbaikan keamanan diprioritaskan pada kode terbaru di branch `main`. Tidak ada komitmen pemeliharaan atau backport untuk commit atau versi lama.

## Melaporkan kerentanan

1. Jika tersedia, gunakan **Security > Advisories > Report a vulnerability** pada repository: https://github.com/waifujahat/access-console/security/advisories/new
2. Jika pelaporan privat belum diaktifkan, jangan mengunggah detail eksploitasi atau data sensitif ke issue publik. Buat issue tanpa rincian teknis untuk meminta jalur komunikasi privat.
3. Sertakan ringkasan dampak, langkah reproduksi yang aman, komponen terdampak, dan rekomendasi mitigasi apabila tersedia.

Jangan mengirim kata sandi, access token, connection string, sertifikat, atau data pribadi asli. Gunakan data uji dan lakukan penyuntingan informasi sensitif sebelum mengirim bukti.

## Ruang lingkup

Laporan yang paling relevan mencakup autentikasi, otorisasi kepemilikan akun, pencabutan token, pembatasan permintaan, kebocoran data, dan konfigurasi deployment.

Temuan pada layanan pihak ketiga, seperti penyedia hosting dan database, juga perlu dilaporkan kepada penyedia terkait.
