# Access Console

<div align="center">

<a href="https://tenor.com/view/alya-roshidere-anxiety-embarrassed-gif-18434340723034717783">
  <img
    src="https://media1.tenor.com/m/_9PvN-EAflcAAAAC/alya-roshidere.gif"
    alt="Alya dari Roshidere, animasi tersipu"
    width="260"
  />
</a>


**Identity & Access Management · Full-stack Project**

![Next.js](https://img.shields.io/badge/Next.js-16-18181B?style=flat-square&logo=nextdotjs&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET_Core-10-512BD4?style=flat-square&logo=dotnet&logoColor=white)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?style=flat-square&logo=postgresql&logoColor=white)
![TypeScript](https://img.shields.io/badge/TypeScript-3178C6?style=flat-square&logo=typescript&logoColor=white)

</div>

Access Console adalah proyek full-stack untuk mengimplementasikan **manajemen identitas dan akses**, mulai dari registrasi akun dan autentikasi hingga otorisasi, persistensi data, pengujian dan keamanan API

## Contents

1. [Overview](#1-overview)
2. [Technology Stack](#2-technology-stack)
3. [Architecture](#3-architecture)
4. [Authentication & Account API](#4-authentication--account-api)
5. [Database & Persistence](#5-database--persistence)
6. [Getting Started](#6-getting-started)
7. [Testing & Code Quality](#7-testing--code-quality)

---

## 1. Overview

Frontend dikembangkan dengan **Next.js** dan backend dengan **ASP.NET Core**, modul akun menggunakan **ASP.NET Core Identity** untuk autentikasi dan **Entity Framework Core** untuk akses data PostgreSQL

### Implemented backend capabilities

- Registrasi akun melalui email, nama, dan kata sandi
- Login dengan Identity Bearer Token dan hashing kata sandi melalui ASP.NET Core Identity
- Validasi input dan kebijakan kekuatan kata sandi
- Pencegahan email duplikat tanpa membedakan huruf besar dan kecil
- Otorisasi akses akun berdasarkan identitas pemilik
- Account lockout setelah sejumlah kegagalan login
- Rate limiting terpisah untuk registrasi dan login
- Penyimpanan akun domain dan pengguna Identity dalam satu transaksi database
- Database migrations menggunakan Entity Framework Core
- Pengujian lokal untuk domain, persistence, Identity, dan HTTP API

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

Stack menggunakan Next.js untuk frontend dan ASP.NET Core untuk API. PostgreSQL menyimpan data akun, sementara Docker Compose menyediakan database lokal.

## 3. Architecture

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

---

## 4. Authentication & Account API

Endpoint autentikasi bersifat publik, sedangkan endpoint pengambilan akun memerlukan Bearer Token dan validasi kepemilikan.

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

ID pada contoh respons hanya ilustrasi.

Registrasi menerapkan validasi domain dan kebijakan password dari ASP.NET Core Identity

Kebijakan password saat ini:

- Minimum 6 karakter
- Mengandung angka
- Mengandung huruf kecil
- Mengandung huruf besar
- Mengandung karakter non-alfanumerik

Email duplikat dicegah melalui validasi Identity dan constraint database, respons validasi atau konflik bergantung pada kondisi kegagalan

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

### 4.4 Get Account

Endpoint berikut memerlukan Bearer Token:

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

## 5. Database & Persistence

Access Console menggunakan **Entity Framework Core 10** dengan provider PostgreSQL **Npgsql**

### 5.1 Domain Accounts

Tabel `accounts` memiliki kolom:

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

Skema database dikelola melalui Entity Framework Core migrations.

- `InitialAccounts` — schema akun domain
- `AddIdentityTables` — schema ASP.NET Core Identity

Validasi domain, mekanisme Identity, dan constraint database digunakan bersama untuk menjaga integritas data

## 6. Getting Started

Instruksi berikut menggunakan Windows PowerShell dari root repository. Jalankan frontend, database, dan backend pada terminal terpisah sesuai kebutuhan.

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

Jalankan database:

```powershell
docker compose --env-file .env.postgres up -d postgres
```

PostgreSQL tersedia secara lokal melalui port `5433`

### 6.3 Backend

Siapkan connection string pada sesi PowerShell:

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

Pulihkan .NET tools:

```powershell
dotnet tool restore
```

Terapkan migration:

```powershell
dotnet ef database update --project backend/AccessConsole.Api --startup-project backend/AccessConsole.Api --context AccessConsoleDbContext
```

Jalankan backend:

```powershell
dotnet run --project backend/AccessConsole.Api/AccessConsole.Api.csproj --no-launch-profile --urls http://127.0.0.1:5080
```

API: http://127.0.0.1:5080

Health check: http://127.0.0.1:5080/health

### 6.4 Reverse Proxy

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

## 7. Testing & Code Quality

Pengujian backend menggunakan **xUnit** dan **WebApplicationFactory**.

> **Devlog :** Direktori `tests/` dikecualikan oleh `.gitignore` dan tidak disertakan pada repository publik. Instruksi pengujian di bawah berlaku untuk lingkungan pengembangan lokal yang memiliki proyek pengujian tersebut. Hasil pengujian tidak dapat direproduksi hanya dari clone publik

### 7.1 Unit & Integration Tests

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

**Hasil pengujian lokal yang didokumentasikan pada 7 November 2026**

```text
Total:     39
Succeeded: 39
Failed:     0
Skipped:    0
```

### 7.4 Code Quality

Verifikasi format backend:

```powershell
dotnet format backend/AccessConsole.Api/AccessConsole.Api.csproj --verify-no-changes
```

Verifikasi format pengujian:

```powershell
dotnet format tests/AccessConsole.Api.Tests/AccessConsole.Api.Tests.csproj --verify-no-changes
```

Periksa whitespace Git:

```powershell
git diff --check
```

---

<p align="center">
  <sub>
    Access Console · Developed by
    <a href="https://github.com/waifujahat">waifujahat</a>
  </sub>
</p>
