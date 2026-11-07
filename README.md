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
  <i>Hmph! It's not like I built this for you or anything...</i>
</p>

**Identity and Access Management · Fullstack Engineering Project**

> A structured approach to building identity management systems

Access Console adalah proyek fullstack berbasis **Next.js, TypeScript, C#, ASP.NET Core, dan PostgreSQL** yang dikembangkan untuk menerapkan manajemen identitas, arsitektur aplikasi, persistensi data serta pengujian perangkat lunak

---

## Contents

1. [Overview](#1-overview)
2. [Technology Stack](#2-technology-stack)
3. [Architecture](#3-architecture)
4. [Account Management API](#4-account-management-api)
5. [Database & Persistence](#5-database--persistence)
6. [Getting Started](#6-getting-started)
7. [Testing](#7-testing)

---

## 1. Overview

Access Console memisahkan aplikasi menjadi frontend Next.js dan backend ASP.NET Core

Implementasi saat ini berfokus pada pengelolaan akun pengguna dengan validasi domain, database PostgreSQL dan REST API

**Core capabilities**

- Pembuatan dan pencarian akun
- Validasi email dan nama pengguna
- Pencegahan email duplikat tanpa membedakan huruf kapital
- Penyimpanan data permanen menggunakan PostgreSQL
- Database migration dengan Entity Framework Core
- Pengujian otomatis untuk domain, persistence, dan HTTP API

## 2. Technology Stack

| Component | Technology |
|---|---|
| Frontend | Next.js 16, React, TypeScript |
| Styling | Tailwind CSS |
| Backend | C#, ASP.NET Core 10 |
| Database | PostgreSQL 17 |
| ORM | Entity Framework Core 10 |
| Infrastructure | Docker Compose |
| Testing | xUnit, WebApplicationFactory |
| Code Quality | EditorConfig, ESLint |

## 3. Architecture

Struktur backend memisahkan domain, kontrak API, abstraksi repository dan implementasi database

```text
access-console/
├── src/                         # Next.js frontend
├── public/                      # Static assets
├── backend/
│   └── AccessConsole.Api/
│       ├── Application/         # Repository interfaces
│       ├── Contracts/           # Request and response models
│       ├── Domain/              # Domain entities and validation
│       ├── Infrastructure/
│       │   └── Persistence/     # EF Core, PostgreSQL, migrations
│       └── Program.cs           # API endpoints and configuration
├── tests/
│   └── AccessConsole.Api.Tests/ # Automated tests
├── compose.yaml
└── dotnet-tools.json
```

Alur pemrosesan akun :

```text
HTTP Request
     |
     v
ASP.NET Core API
     |
     v
Account Domain
     |
     v
IAccountRepository
     |
     v
PostgresAccountRepository
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
  <i>Don't get the wrong idea. Good architecture just matters, okay?</i>
</p>

---

## 4. Account Management API

### 4.1 Endpoints

| Method | Endpoint | Description |
|---|---|---|
| GET | `/health` | API health check |
| POST | `/api/accounts` | Create account |
| GET | `/api/accounts/{id}` | Get account by ID |

### 4.2 Create Account

**Request**

```http
POST /api/accounts
Content-Type: application/json
```

```json
{
  "email": "developer@example.com",
  "displayName": "Developer"
}
```

Respons keberhasilan : **201 Created**.

Jika email sudah terdaftar, API mengembalikan **409 Conflict**

```json
{
  "pesan": "Email sudah terdaftar"
}
```

### 4.3 HTTP Status Codes

| Status | Meaning |
|---|---|
| 200 OK | Account retrieved |
| 201 Created | Account created |
| 400 Bad Request | Invalid account data |
| 404 Not Found | Account not found |
| 409 Conflict | Duplicate email |

## 5. Database & Persistence

Access Console menggunakan **Entity Framework Core** dengan provider Npgsql

Tabel `accounts` memiliki kolom :

| Column | Type |
|---|---|
| `id` | UUID |
| `email` | CITEXT |
| `display_name` | VARCHAR(100) |
| `created_at_utc` | TIMESTAMPTZ |

Email menggunakan tipe `CITEXT` dan unique index untuk mencegah duplikasi tanpa membedakan huruf besar dan kecil

Database schema dikelola melalui migration `InitialAccounts`

Validasi domain dan constraint database digunakan bersama untuk menjaga konsistensi data. Bukan karena terlalu protektif tentu saja

## 6. Getting Started

**Requirements:** Node.js, .NET 10 SDK dan Docker Desktop

### 6.1 Frontend

```powershell
npm.cmd ci
npm.cmd run dev
```

Frontend : http://localhost:3000

### 6.2 PostgreSQL

Buat file `.env.postgres` pada root proyek

```dotenv
ACCESS_CONSOLE_DB_PASSWORD=replace-with-a-secure-password
```

Jalankan database

```powershell
docker compose --env-file .env.postgres up -d postgres
```

Database tersedia secara lokal pada port `5433`

### 6.3 Backend

Siapkan connection string pada sesi PowerShell

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

Terapkan migration

```powershell
dotnet tool restore

dotnet ef database update --project backend/AccessConsole.Api --startup-project backend/AccessConsole.Api --context AccessConsoleDbContext
```

Jalankan backend

```powershell
dotnet run --project backend/AccessConsole.Api/AccessConsole.Api.csproj --no-launch-profile --urls http://127.0.0.1:5080
```

API : http://127.0.0.1:5080

**Security :** File `.env.postgres` diabaikan oleh Git dan tidak boleh dipublikasikan, jangan sampai password database ikut ter-commit, ya

## 7. Testing

Pengujian menggunakan **xUnit** dan **WebApplicationFactory**

Hentikan proses `AccessConsole.Api` sebelum menjalankan pengujian agar executable tidak terkunci

```powershell
dotnet test tests/AccessConsole.Api.Tests/AccessConsole.Api.Tests.csproj
```

Hasil verifikasi terakhir :

**26 passed · 0 failed**

Pengujian meliputi validasi domain akun, repository, konfigurasi EF Core, serta respons HTTP `200`, `201`, `400`, `404` dan `409`

Pengujian HTTP otomatis menggunakan repository pengganti sehingga tidak mengubah data PostgreSQL asli

---

<p align="center">
  <sub>
    Access Console · Developed by
    <a href="https://github.com/waifujahat">waifujahat</a>
  </sub>
</p>
