# Deployment Casdoor

Panduan untuk mengaktifkan login Google, GitHub dan pengaitan akun Access Console melalui Casdoor di lingkungan Production. Frontend dan backend tetap menggunakan deployment Vercel yang sudah ada.

## 1. PostgreSQL Neon

Buat project Neon terpisah untuk Casdoor dengan database `casdoor` dan role khusus yang memiliki database tersebut. Jangan menggunakan database atau kredensial Access Console. Pilih region sedekat mungkin dengan server Render.

Gunakan koneksi langsung (non-pooler), dengan TLS dan validasi sertifikat. Format konfigurasi Casdoor untuk PostgreSQL:

```text
host=<neon-direct-host> port=5432 user=<casdoor-role> password=<password> dbname=casdoor sslmode=verify-full
```

Isi nilai nyata hanya melalui dashboard hosting. Jangan menyimpan connection string atau kredensial di Git. Jika sudah memiliki pengguna Casdoor lokal yang perlu dipertahankan, rencanakan migrasi database dan identitas terpisah sebelum mengubah issuer. Identitas eksternal Access Console menggunakan pasangan issuer dan subject, bukan email.

## 2. Render

Deploy Blueprint `render.yaml` dari repository ini atau buat Web Service dari image yang sama. Blueprint menggunakan image Casdoor standar yang versinya ditentukan secara eksplisit, bukan image all-in-one dengan SQLite.

Paket Free ditujukan untuk demo. Layanan dapat berhenti sementara jika tidak aktif, dan filesystem serta sesi Casdoor lokal tidak persisten saat restart. Database, pengguna dan konfigurasi Casdoor tetap disimpan di Neon. Untuk penggunaan dengan kebutuhan ketersediaan yang lebih tinggi, gunakan compute selalu aktif dan pertimbangkan Redis untuk sesi.

Isi variabel berikut sebelum deployment pertama:

| Variabel | Nilai |
| --- | --- |
| `dataSourceName` | Connection string PostgreSQL Casdoor dengan TLS |
| `origin` | URL HTTPS publik layanan Casdoor di Render |
| `originFrontend` | URL HTTPS publik yang sama |
| `initAdminPassword` | Kata sandi admin awal acak dan kuat |

Pastikan URL `origin` sama dengan URL layanan sesungguhnya (tanpa trailing slash). Jangan mengaktifkan instance publik dengan kredensial admin bawaan atau halaman pendaftaran admin terbuka. Nilai `initAdminPassword` berlaku hanya saat inisialisasi database baru.

Verifikasi `/api/health` dan `/.well-known/openid-configuration`. Periksa bahwa `issuer` dalam discovery document sama persis dengan URL HTTPS Casdoor yang digunakan frontend dan backend.

## 3. Aplikasi dan provider OAuth

Masuk sebagai admin Casdoor dan buat organisasi serta aplikasi khusus Access Console. Atur penandatanganan access token JWT dengan sertifikat aplikasi yang persisten di database, dan pastikan token memuat `sub`, `email`, `email_verified` serta `tokenType=access-token`. Jangan menetapkan `email_verified=true` untuk alamat yang tidak benar-benar diverifikasi.

Gunakan redirect URL aplikasi berikut:

```text
https://access-console-five.vercel.app/api/auth/callback/casdoor
```

Buat provider Google dan GitHub dengan nama yang sama seperti variabel environment frontend. Tambahkan keduanya pada aplikasi Casdoor dan aktifkan sign-in. Pada konfigurasi OAuth Google Cloud dan GitHub, gunakan callback dengan URL Casdoor, bukan URL Vercel:

```text
https://<actual-casdoor-host>/callback
```

Gunakan kredensial OAuth terpisah untuk production dan development jika diperlukan. Jangan mengirim client secret melalui chat atau menyimpannya di repository. Pastikan pengaturan pengaitan identitas di Casdoor tidak menggabungkan akun melalui email yang tidak diverifikasi.

## 4. Environment Vercel

Tambahkan environment variables khusus Production pada kedua project. Pertahankan seluruh environment variables yang sudah ada, termasuk database Access Console dan sertifikat Data Protection backend.

Frontend (`access-console-five.vercel.app`):

| Variabel | Nilai |
| --- | --- |
| `ACCESS_CONSOLE_API_URL` | `https://access-console-api.vercel.app` |
| `AUTH_URL` | `https://access-console-five.vercel.app` |
| `AUTH_SECRET` | Nilai acak minimum 32 byte yang tetap sama antar deployment |
| `CASDOOR_ISSUER` | URL HTTPS Casdoor tanpa trailing slash |
| `CASDOOR_CLIENT_ID` | Client ID aplikasi Casdoor |
| `CASDOOR_CLIENT_SECRET` | Client secret aplikasi Casdoor |
| `CASDOOR_GOOGLE_PROVIDER` | Nama provider Google di Casdoor |
| `CASDOOR_GITHUB_PROVIDER` | Nama provider GitHub di Casdoor |

Backend (`access-console-api.vercel.app`):

| Variabel | Nilai |
| --- | --- |
| `CASDOOR_ISSUER` | Sama persis dengan frontend |
| `CASDOOR_CLIENT_ID` | Sama persis dengan frontend |

Backend tidak memerlukan client secret Casdoor untuk validasi JWT. Backend harus berjalan pada lingkungan Production dengan sertifikat Data Protection yang sudah dikonfigurasi. Jangan menggunakan `Development` untuk mengaktifkan endpoint OAuth.

Pastikan migrasi Access Console telah diterapkan. Deploy backend terlebih dahulu, lalu frontend. Gunakan URL production kanonis, bukan URL preview, untuk pengujian OAuth.

## 5. Verifikasi

- API `/health` dan `/health/ready` mengembalikan `200`.
- POST `/api/auth/casdoor/verify`, `/exchange` dan `/link` tanpa autentikasi mengembalikan `401`, bukan `404`.
- Registrasi, login kata sandi, halaman akun dan logout tetap berfungsi.
- Login Google dan GitHub mengembalikan pengguna ke `/akun` dengan sesi Access Console.
- Login eksternal dengan email yang sudah terdaftar tidak otomatis mengambil alih akun lokal.
- Pengaitan hanya berhasil dengan sesi lokal valid, identitas Casdoor valid, serta autentikasi terbaru atau konfirmasi kata sandi.
- Identitas yang sudah terhubung ke akun lain ditolak.
- Token Access Console yang dicabut melalui logout tidak dapat digunakan kembali.

Jangan menganggap deployment berhasil sampai kedua provider dan pengaitan akun telah diuji dari URL production.

## Referensi

- [Casdoor: Docker](https://casdoor.ai/docs/basic/try-with-docker/)
- [Casdoor: konfigurasi database](https://casdoor.ai/docs/basic/server-installation/)
- [Casdoor: konfigurasi aplikasi](https://casdoor.ai/docs/application/config/)
- [Render: Blueprint specification](https://render.com/docs/blueprint-spec)
- [Render: Free services](https://render.com/docs/free)
