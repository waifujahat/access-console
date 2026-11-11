# Access Console

<div align="center">

<a href="https://tenor.com/view/alya-roshidere-anxiety-embarrassed-gif-18434340723034717783">
  <img
    src="https://media1.tenor.com/m/_9PvN-EAflcAAAAC/alya-roshidere.gif"
    alt="Animasi Alya dari Roshidere"
    width="260"
  />
</a>

<p><strong>Identity and Access Management | Fullstack Application</strong></p>

<p>
  <img src="https://img.shields.io/badge/Next.js-16-18181B?style=flat-square&logo=nextdotjs&logoColor=white" alt="Next.js 16" />
  <img src="https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=flat-square&logo=dotnet&logoColor=white" alt="ASP.NET Core 10" />
  <img src="https://img.shields.io/badge/PostgreSQL-17-4169E1?style=flat-square&logo=postgresql&logoColor=white" alt="PostgreSQL 17" />
  <img src="https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white" alt="TypeScript" />
</p>

[![Access Console CI](https://github.com/waifujahat/access-console/actions/workflows/ci.yml/badge.svg)](https://github.com/waifujahat/access-console/actions/workflows/ci.yml)

</div>

Access Console adalah aplikasi fullstack untuk mengimplementasikan **Identity and Access Management**. Proyek ini menggabungkan registrasi akun, autentikasi berbasis token, otorisasi kepemilikan data, persistensi PostgreSQL dan pengamanan API dalam arsitektur frontend dan backend yang terpisah.

| Layanan | Teknologi | Tautan |
| --- | --- | --- |
| Frontend | Next.js 16 (Vercel) | [Buka aplikasi](https://access-console-five.vercel.app/) |
| Registrasi | Next.js | [Buat akun](https://access-console-five.vercel.app/daftar) |
| Login | Next.js | [Masuk](https://access-console-five.vercel.app/login) |
| Backend API | ASP.NET Core 10 (Vercel) | [Health check](https://access-console-api.vercel.app/health) |
| Database readiness | PostgreSQL 17 (Neon) | [Readiness check](https://access-console-api.vercel.app/health/ready) |
| Source code | GitHub | [Repository](https://github.com/waifujahat/access-console) |

## Fitur

### Autentikasi Akun

- Registrasi menggunakan nama, email, dan kata sandi dengan validasi pada frontend, domain dan ASP.NET Core Identity.
- Login menggunakan **ASP.NET Core Identity Bearer Token**, bukan JWT yang diterbitkan secara manual.
- Penyimpanan token akses dalam cookie `HttpOnly`, `Secure` pada production dan `SameSite=Lax` melalui Next.js Server Actions.
- Halaman akun terlindungi pada `/akun`, termasuk pengambilan profil melalui `GET /api/accounts/me`.
- Otorisasi berbasis identitas pemilik pada `GET /api/accounts/{id}`.
- Logout melalui API yang mencatat hash token yang dicabut di PostgreSQL dan menolak penggunaan ulang token tersebut.
- Penyimpanan akun domain dan pengguna Identity dalam satu transaksi database.

### Keamanan

- Hash kata sandi dikelola oleh ASP.NET Core Identity.
- Pencegahan email duplikat menggunakan validasi Identity serta unique index PostgreSQL bertipe `CITEXT`.
- Account lockout setelah 5 kegagalan login selama 15 menit.
- Fixed-window rate limiting untuk registrasi, login per IP dan login per akun terdaftar.
- Header `Cache-Control: no-store` pada endpoint akun dan autentikasi.
- Persistensi Data Protection keys di PostgreSQL, dengan perlindungan sertifikat pada lingkungan selain Development.
- Health check untuk proses API dan pemeriksaan kesiapan database.
- Pemeriksaan frontend, backend, migrasi, dan alur autentikasi dasar melalui GitHub Actions.

## Teknologi

| Lapisan | Teknologi |
| --- | --- |
| Frontend | Next.js 16, React 19, TypeScript |
| Styling | Tailwind CSS 4 |
| Backend | C#, ASP.NET Core 10 |
| Identity | ASP.NET Core Identity, Identity Bearer Token |
| Database | PostgreSQL 17, Neon untuk deployment |
| Persistence | Entity Framework Core 10, Npgsql |
| Infrastruktur lokal | Docker Compose |
| Pengujian | GitHub Actions, API smoke tests, xUnit untuk pengujian lokal |
| Kualitas kode | ESLint, EditorConfig, `dotnet format` |

## Arsitektur

```text
access-console/
├── .github/workflows/ci.yml           # Verifikasi otomatis
├── backend/AccessConsole.Api/
│   ├── Application/Accounts/          # Kontrak permintaan dan repositori
│   ├── Contracts/Accounts/            # Model respons API
│   ├── Domain/                        # Aturan domain akun dan autentikasi
│   ├── Endpoints/AccountEndpoints.cs  # Endpoint dan otorisasi akun
│   ├── Infrastructure/
│   │   ├── Authentication/            # Pencabutan token akses
│   │   ├── Configuration/             # Pembatasan permintaan
│   │   ├── Identity/                  # ASP.NET Core Identity
│   │   └── Persistence/               # EF Core dan migrasi database
│   └── Program.cs                     # Konfigurasi dan endpoint autentikasi
├── public/                            # Aset frontend
├── scripts/run-backend.ps1            # Menjalankan backend lokal
├── src/                               # Halaman, komponen, dan utilitas Next.js
├── compose.yaml                       # PostgreSQL lokal
└── dotnet-tools.json                  # Manifest alat .NET
```

```text
Browser
   |
   v
Next.js (Server Actions)
   |
   v
ASP.NET Core API
   |               \
   v                v
Identity          Account domain
   |                |
   +--------+-------+
            |
            v
   Entity Framework Core
            |
            v
        PostgreSQL
```

## Referensi API

| Metode | Endpoint | Fungsi | Akses |
| --- | --- | --- | --- |
| `GET` | `/health` | Memeriksa proses API | Publik |
| `GET` | `/health/ready` | Memeriksa kesiapan database | Publik |
| `POST` | `/api/auth/register` | Mendaftarkan akun | Publik |
| `POST` | `/api/auth/login` | Login dan memperoleh access token | Publik |
| `POST` | `/api/auth/logout` | Mencabut access token | Bearer token |
| `GET` | `/api/accounts/me` | Mengambil profil pengguna aktif | Bearer token |
| `GET` | `/api/accounts/{id}` | Mengambil akun milik pengguna | Bearer token |
| `POST` | `/api/accounts` | Endpoint lama, mengembalikan `410 Gone` | Publik |

### Registrasi

```http
POST /api/auth/register
Content-Type: application/json
```

```json
{
  "email": "developer@example.com",
  "displayName": "Developer",
  "kataSandi": "Contoh1!"
}
```

Registrasi yang berhasil mengembalikan status `201 Created` beserta ID, email dan nama akun. Kata sandi harus berisi minimal 6 karakter, termasuk huruf kecil, huruf besar, angka dan karakter nonalfanumerik.

### Login

```http
POST /api/auth/login
Content-Type: application/json
```

```json
{
  "email": "developer@example.com",
  "kataSandi": "Contoh1!"
}
```

Respons login yang berhasil memuat `accessToken`. Token tersebut digunakan untuk mengakses API terproteksi :

```http
GET /api/accounts/me
Authorization: Bearer <access-token>
```

Untuk mengakhiri sesi pada backend :

```http
POST /api/auth/logout
Authorization: Bearer <access-token>
```

Logout yang berhasil mengembalikan status `204 No Content`. Token yang dicabut akan ditolak apabila digunakan kembali.

### Kebijakan pembatasan permintaan

| Mekanisme | Batas |
| --- | --- |
| Registrasi per IP | 10 permintaan per menit |
| Login per IP | 120 permintaan per menit |
| Login per akun terdaftar | 10 permintaan per menit |
| Account lockout | 5 kegagalan, selama 15 menit |

Pembatasan menggunakan fixed window. Ketika batas tercapai, API mengembalikan `429 Too Many Requests`. Pembatasan saat ini berada di memori masing-masing instance API, bukan kuota terdistribusi lintas instance.

## Local Run

### 1. Clone repository

```powershell
git clone https://github.com/waifujahat/access-console.git
cd access-console
```

### 2. Siapkan PostgreSQL

Buat file `.env.postgres` pada root proyek :

```dotenv
ACCESS_CONSOLE_DB_PASSWORD=replace-with-a-secure-password
```

Jalankan container database :

```powershell
docker compose --env-file .env.postgres up -d postgres
```

Database lokal tersedia pada `127.0.0.1:5433`.

### 3. Jalankan migrasi database

```powershell
$Baris = Get-Content .env.postgres |
    Where-Object { $_.StartsWith("ACCESS_CONSOLE_DB_PASSWORD=") } |
    Select-Object -First 1

if (-not $Baris) {
    throw "Password PostgreSQL tidak ditemukan"
}

$Sandi = $Baris.Substring("ACCESS_CONSOLE_DB_PASSWORD=".Length).Trim()

$env:ConnectionStrings__AccessConsole = "Host=127.0.0.1;Port=5433;Database=access_console;Username=access_console;Password=$Sandi"

Remove-Variable Sandi
```

Terapkan migrasi :

```powershell
dotnet tool restore --tool-manifest dotnet-tools.json

dotnet ef database update `
    --project backend/AccessConsole.Api `
    --startup-project backend/AccessConsole.Api `
    --context AccessConsoleDbContext
```

Migrasi proyek mencakup akun domain, tabel Identity, daftar token yang dicabut dan penyimpanan Data Protection keys.

### 4. Jalankan backend

```powershell
.\scripts\run-backend.ps1
```
### 5. Jalankan frontend

```powershell
npm.cmd ci
npm.cmd run dev
```

## Catatan

| Variabel | Kegunaan |
| --- | --- |
| `ConnectionStrings__AccessConsole` | Koneksi PostgreSQL untuk backend |
| `ACCESS_CONSOLE_API_URL` | Alamat API untuk request server-side Next.js |
| `ReverseProxy__TrustedProxy` | Alamat IP reverse proxy yang diizinkan mengirim forwarded headers |
| `DataProtection__CertificateBase64` | Sertifikat untuk melindungi Data Protection keys di luar Development |
| `DataProtection__CertificatePassword` | Kata sandi sertifikat Data Protection |

Jangan memasukkan connection string berisi kata sandi, file `.env`, sertifikat atau access token ke dalam Git. Gunakan HTTPS dan reverse proxy tepercaya untuk deployment production.
Access Console merupakan proyek IAM untuk demonstrasi arsitektur dan praktik keamanan aplikasi. Penerapan production lebih luas memerlukan evaluasi lanjutan, termasuk penyimpanan rate limit terdistribusi, pengelolaan operasional token serta sertifikat dan lain-lain.

---

<p align="center">
  <sub>
    Developed by <a href="https://github.com/waifujahat">waifujahat</a>
  </sub>
</p>
