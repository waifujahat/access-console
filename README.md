# Access Console

<p align="center">
  <a href="https://tenor.com/view/anime-tsundere-pout-petty-upset-gif-8657468">
    <img
      src="https://media1.tenor.com/m/p4vHR4zM_u0AAAAd/anime-tsundere.gif"
      alt="Tsundere Pout"
      width="260"
    />
  </a>
</p>

<p align="center">
  <i>Hmph! It's not like I built this for you or anything... b-baka!</i>
</p>

<p align="center">
  <strong>Identity and Access Management · Fullstack Engineering Project</strong>
</p>

<p align="center">
  <i>Secure identities. Protected accounts. Just a little tsundere energy. ♡</i>
</p>

> **A structured approach to building identity management systems**
> *“Sistem ini cuma memastikan akunmu aman, kok! Jangan salah paham!”* (⁄ ⁄>⁄ ▽ ⁄<⁄ ⁄)

Access Console adalah proyek fullstack berbasis **Next.js, TypeScript, C#, ASP.NET Core, dan PostgreSQL** yang dikembangkan untuk manajemen identitas, autentikasi, otorisasi, arsitektur aplikasi, persistensi data dan pengujian perangkat lunak

---

## Contents

1. [Overview](#1-overview)
2. [Technology Stack](#2-technology-stack)
3. [Architecture](#3-architecture)
4. [Authentication & Account API](#4-authentication--account-api)
5. [Database & Persistence](#5-database--persistence)
6. [Getting Started](#6-getting-started)
7. [Testing](#7-testing)

---

## 1. Overview

Access Console memisahkan aplikasi menjadi frontend Next.js dan backend ASP.NET Core

Implementasi backend saat ini berfokus pada registrasi, login, dan perlindungan akses data akun menggunakan **ASP.NET Core Identity**

**Core capabilities**

- Registrasi akun menggunakan email, nama, dan kata sandi
- Login menggunakan Identity Bearer Token
- Password hashing melalui ASP.NET Core Identity
- Validasi data akun dan kebijakan kata sandi
- Pencegahan email duplikat tanpa membedakan huruf kapital
- Pembatasan akses akun berdasarkan identitas pengguna
- Account lockout setelah beberapa kegagalan login
- Rate limiting untuk endpoint registrasi dan login
- Penyimpanan akun dan pengguna Identity dalam satu transaksi database
- Database migration menggunakan Entity Framework Core
- Pengujian otomatis untuk domain, persistence, Identity, dan HTTP API

> ♡ **Dev note :** Pengguna boleh punya akun sendiri, tapi bukan berarti boleh mengintip akun orang lain *B-bukan karena aku posesif, ini namanya authorization!* (｡•̀ᴗ-)✧

## 2. Technology Stack

| Component | Technology |
|---|---|
| Frontend | Next.js 16, React, TypeScript |
| Styling | Tailwind CSS |
| Backend | C#, ASP.NET Core 10 |
| Authentication | ASP.NET Core Identity, Bearer Token |
| Database | PostgreSQL 17 |
| ORM | Entity Framework Core 10, Npgsql |
| Infrastructure | Docker Compose |
| Testing | xUnit, WebApplicationFactory |
| Code Quality | EditorConfig, ESLint, dotnet format |

*Semua teknologi ini dipilih untuk membangun fondasi yang rapi, bkan untuk pamer, ya... hmph!*

## 3. Architecture

Backend memisahkan domain, kontrak API, abstraksi repository, implementasi persistence dan layanan Identity

```text
access-console/
├── src/                          # Next.js frontend
├── public/                       # Static assets
├── backend/
│   └── AccessConsole.Api/
│       ├── Application/
│       │   └── Accounts/         # Requests, repository, services
│       ├── Contracts/
│       │   └── Accounts/         # API response models
│       ├── Domain/
│       │   └── Accounts/         # Domain entities and validation
│       ├── Infrastructure/
│       │   ├── Identity/         # Identity user and registration
│       │   └── Persistence/      # EF Core, PostgreSQL, migrations
│       └── Program.cs            # API endpoints and middleware
├── tests/
│   └── AccessConsole.Api.Tests/  # Unit and integration tests
├── compose.yaml
└── dotnet-tools.json
```

**Alur registrasi akun**

```text
HTTP Request
     |
     v
POST /api/auth/register
     |
     v
AccountRegistrationService
     |
     v
Database Transaction
     |
     +----> Account Domain
     |           |
     |           v
     |        accounts
     |
     +----> ASP.NET Core Identity
                 |
                 v
             AspNetUsers
     |
     v
Commit / Rollback
```

Registrasi menggunakan satu transaksi database, apabila pembuatan pengguna Identity gagal, perubahan pada akun domain dibatalkan

**Alur akses akun**

```text
GET /api/accounts/{id}
     |
     v
Bearer Authentication
     |
     v
Authorization
     |
     v
Account Ownership Check
     |
     v
IAccountRepository
     |
     v
Entity Framework Core
     |
     v
PostgreSQL
```

<p align="center">
  <a href="https://tenor.com/view/tsundere-blush-lucky-star-blushing-shy-gif-5694397">
    <img
      src="https://media1.tenor.com/m/95OEi6GuDEsAAAAd/tsundere-blush.gif"
      alt="Tsundere Blush"
      width="220"
    />
  </a>
</p>

<p align="center">
  <i>Don't get the wrong idea. Good architecture just matters, okay? ♡</i>
</p>

> *“Domain, Identity dan database itu punya tanggung jawab masing-masing, jangan dicampur sembarangan, b-baka!”*

---

## 4. Authentication & Account API

*“Mau masuk? Tunjukkan identitasmu dulu, lu pikir gua akan percaya begitu saja!”* (¬_¬)

### 4.1 Endpoints

| Method | Endpoint | Description | Authentication |
|---|---|---|---|
| GET | `/health` | API health check | Public |
| POST | `/api/auth/register` | Register account | Public |
| POST | `/api/auth/login` | Authenticate user | Public |
| GET | `/api/accounts/{id}` | Get owned account | Bearer Token |
| POST | `/api/accounts` | Deprecated endpoint (410 Gone) | Public |

### 4.2 Register Account

**Request**

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

Respons keberhasilan: **201 Created**

```json
{
  "id": "00000000-0000-0000-0000-000000000001",
  "email": "developer@example.com",
  "displayName": "Developer"
}
```

ID di atas hanya contoh

Registrasi menerapkan validasi domain dan kebijakan password dari ASP.NET Core Identity

Kebijakan password saat ini :

- Minimum 6 karakter
- Mengandung angka
- Mengandung huruf kecil
- Mengandung huruf besar
- Mengandung karakter non-alfanumerik

Email duplikat dicegah melalui validasi Identity dan constraint database, respons validasi atau konflik bergantung pada kondisi kegagalan

> ♡ *“Satu email untuk satu akun, ngga bisa dong maksa daftar dua kali...”*

### 4.3 Login

**Request**

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

Login menggunakan `SignInManager` dan menghasilkan Bearer Token melalui mekanisme autentikasi ASP.NET Core Identity

Token yang digunakan adalah **Identity Bearer Token**, bukan JWT yang diterbitkan secara manual

Token akses dikirim melalui properti `accessToken` pada respons login

Contoh penggunaannya :

```http
GET /api/accounts/{id}
Authorization: Bearer <access-token>
```

**Perlindungan login**

- Maksimum 5 kegagalan login sebelum lockout
- Durasi lockout 15 menit
- Rate limiting 10 request per menit per IP pada endpoint login
- Rate limiting 10 request per menit per IP pada endpoint registrasi

Rate limiting menggunakan fixed window dan diterapkan secara terpisah untuk masing-masing endpoint

> *“Kalau salah kata sandi terus, aksesmu dikunci sementara, itu namanya keamanan sistem...”*

### 4.4 Get Account

Endpoint berikut memerlukan Bearer Token :

```http
GET /api/accounts/{id}
Authorization: Bearer <access-token>
```

API memeriksa apakah ID akun yang diminta sesuai dengan identitas pengguna yang sudah terautentikasi

Pengguna tidak dapat mengakses data akun milik pengguna lain

### 4.5 HTTP Status Codes

| Status | Meaning |
|---|---|
| 200 OK | Login berhasil atau data akun ditemukan |
| 201 Created | Registrasi akun berhasil |
| 400 Bad Request | Data atau password tidak memenuhi validasi |
| 401 Unauthorized | Token tidak valid atau login gagal |
| 403 Forbidden | Pengguna mencoba mengakses akun lain |
| 404 Not Found | Data akun tidak ditemukan |
| 409 Conflict | Konflik email pada constraint database |
| 410 Gone | Endpoint lama `/api/accounts` tidak digunakan lagi |
| 429 Too Many Requests | Batas permintaan tercapai |

> **HTTP personality check:** `401` — *“Siapa kamu?”* · `403` — *“Aku tahu kamu, tapi ini bukan akunmu!”* · `404` — *“E-eh? Akunnya tidak ditemukan...”*

## 5. Database & Persistence

Access Console menggunakan **Entity Framework Core 10** dengan provider PostgreSQL **Npgsql**

### 5.1 Domain Accounts

Tabel `accounts` memiliki kolom :

| Column | Type |
|---|---|
| `id` | UUID |
| `email` | CITEXT |
| `display_name` | VARCHAR(100) |
| `created_at_utc` | TIMESTAMPTZ |

Email menggunakan tipe `CITEXT` dan unique index untuk mencegah duplikasi tanpa membedakan huruf besar dan kecil

### 5.2 ASP.NET Core Identity

ASP.NET Core Identity menyimpan informasi autentikasi pada tabel Identity, termasuk :

- `AspNetUsers`
- `AspNetRoles`
- `AspNetUserRoles`
- `AspNetUserClaims`
- `AspNetUserLogins`
- `AspNetUserTokens`
- `AspNetRoleClaims`

Password tidak disimpan sebagai teks biasa, penyimpanan password hash ditangani oleh ASP.NET Core Identity

Akun domain dan pengguna Identity menggunakan ID yang sama untuk menghubungkan identitas autentikasi dengan data akun aplikasi

### 5.3 Database Migrations

Schema database dikelola melalui Entity Framework Core migrations

- `InitialAccounts` — schema akun domain
- `AddIdentityTables` — schema ASP.NET Core Identity

Validasi domain, mekanisme Identity, dan constraint database digunakan bersama untuk menjaga integritas data

> *“Jangan mengubah schema sembarangan woi, kalau migration-nya rusak, siapa yang repot? B-bukan aku wkwkwkw”*

## 6. Getting Started

*“Oke, aku tunjukkan cara menjalankannya, perhatikan baik-baik”* ✧

**Requirements**

- Node.js dan npm
- .NET 10 SDK
- Docker Desktop
- Git

Jalankan perintah dari root repository `access-console`

### 6.1 Frontend

```powershell
npm.cmd ci
npm.cmd run dev
```

Frontend: http://localhost:3000

### 6.2 PostgreSQL

Buat file `.env.postgres` pada root proyek

```dotenv
ACCESS_CONSOLE_DB_PASSWORD=replace-with-a-secure-password
```

Jalankan database :

```powershell
docker compose --env-file .env.postgres up -d postgres
```

PostgreSQL tersedia secara lokal melalui port `5433`

### 6.3 Backend

Siapkan connection string pada sesi PowerShell :

```powershell
$Baris = Get-Content ".env.postgres" |
    Where-Object { $_.StartsWith("ACCESS_CONSOLE_DB_PASSWORD=") } |
    Select-Object -First 1

if (-not $Baris) {
    throw "Password PostgreSQL tidak ditemukan"
}

$Sandi = $Baris.Substring("ACCESS_CONSOLE_DB_PASSWORD=".Length)

$env:ConnectionStrings__AccessConsole = "Host=127.0.0.1;Port=5433;Database=access_console;Username=access_console;Password=$Sandi"

Remove-Variable Baris, Sandi
```

Pulihkan .NET tools :

```powershell
dotnet tool restore
```

Terapkan migration :

```powershell
dotnet ef database update --project backend/AccessConsole.Api --startup-project backend/AccessConsole.Api --context AccessConsoleDbContext
```

Jalankan backend :

```powershell
dotnet run --project backend/AccessConsole.Api/AccessConsole.Api.csproj --no-launch-profile --urls http://127.0.0.1:5080
```

API : http://127.0.0.1:5080

Health check : http://127.0.0.1:5080/health

### 6.4 Reverse Proxy

Access Console menyediakan konfigurasi opsional untuk reverse proxy tepercaya

Konfigurasi dapat diberikan melalui environment variable :

```powershell
$env:ReverseProxy__TrustedProxy = "127.0.0.1"
```

Alamat tersebut hanya contoh untuk proxy lokal, gunakan alamat IP reverse proxy yang sebenarnya ketika melakukan deployment

Ketika dikonfigurasi, middleware memproses `X-Forwarded-For` dan `X-Forwarded-Proto` hanya dari proxy yang dipercaya

Tanpa konfigurasi ini, aplikasi menggunakan alamat IP koneksi langsung

**Security**

- Jangan menyimpan password database di repository
- File `.env.postgres` harus tetap diabaikan oleh Git
- Jangan mengunggah token akses atau connection string yang mengandung password
- Gunakan HTTPS dan konfigurasi proxy tepercaya untuk deployment produksi

> 🔐 *“Kredensial itu rahasia, tau! Jangan sampai ter-push ke GitHub”*

## 7. Testing

Pengujian backend menggunakan **xUnit** dan **WebApplicationFactory**

### 7.1 Unit & Integration Tests

Pengujian meliputi :

- Validasi domain akun
- Konfigurasi Entity Framework Core
- Repository dan persistence
- Registrasi akun menggunakan ASP.NET Core Identity
- Validasi dan penolakan email duplikat
- Login menggunakan Bearer Token
- Account lockout
- Otorisasi kepemilikan akun
- HTTP status `200`, `201`, `400`, `401`, `403`, `404`, `409` dan `410`

Sebagian integration test menggunakan database PostgreSQL khusus pengujian

### 7.2 Test Database

Gunakan database terpisah bernama `access_console_test`

Pastikan database tersebut sudah dibuat dan migration sudah diterapkan sebelum menjalankan integration test

Siapkan environment variable pada sesi PowerShell yang sama :

```powershell
$Kontainer = "access-console-postgres-1"

$Pengguna = (docker exec $Kontainer printenv POSTGRES_USER | Out-String).Trim()
$Sandi = (docker exec $Kontainer printenv POSTGRES_PASSWORD | Out-String).Trim()

if ([string]::IsNullOrWhiteSpace($Pengguna) -or
    [string]::IsNullOrWhiteSpace($Sandi)) {
    throw "Kredensial PostgreSQL tidak ditemukan"
}

$env:ACCESS_CONSOLE_TEST_CONNECTION_STRING = "Host=127.0.0.1;Port=5433;Database=access_console_test;Username=$Pengguna;Password=$Sandi"

$env:ConnectionStrings__AccessConsole = $env:ACCESS_CONSOLE_TEST_CONNECTION_STRING

Remove-Variable Sandi
```

### 7.3 Run Tests

```powershell
dotnet test tests/AccessConsole.Api.Tests/AccessConsole.Api.Tests.csproj
```

**Hasil verifikasi lokal terakhir (7 November 2026)**

```text
Total:     39
Succeeded: 39
Failed:     0
Skipped:    0
```

> ♡ **Test report:** *“T-tiga puluh sembilan pengujian berhasil semuanya?! Ya jelas dong! Aku kan sudah memastikan semuanya... hmph!”*

Pengujian integrasi yang menggunakan PostgreSQL dapat membuat dan membersihkan data pengujian, gunakan database terpisah agar tidak memengaruhi data aplikasi

### 7.4 Code Quality

Verifikasi format backend :

```powershell
dotnet format backend/AccessConsole.Api/AccessConsole.Api.csproj --verify-no-changes
```

Verifikasi format pengujian :

```powershell
dotnet format tests/AccessConsole.Api.Tests/AccessConsole.Api.Tests.csproj --verify-no-changes
```

Periksa whitespace Git :

```powershell
git diff --check
```

---

<p align="center">
  <i>“Sampai sini saja. B-bukan berarti aku senang kamu membaca dokumentasinya sampai habis, ya!”</i>
</p>

<p align="center">
  <strong>Built with discipline, protected with Identity, and sprinkled with tsundere energy. ♡</strong>
</p>

<p align="center">
  <sub>
    Access Console · Developed by
    <a href="https://github.com/waifujahat">waifujahat</a>
  </sub>
</p>
