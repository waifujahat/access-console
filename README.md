# Access Console
Access Console adalah proyek fullstack untuk membangun sistem manajemen pengguna dan hak akses menggunakan Next.js, TypeScript, C# dan ASP.NET Core

## Teknologi
- Next.js 16 dan React
- TypeScript
- Tailwind CSS
- C# dan ASP.NET Core 10

## Status
- Frontend Next.js
- Backend ASP.NET Core
- Endpoint health check
- Konfigurasi ESLint dan TypeScript
- Standar formatting melalui EditorConfig

## Struktur
- `src/` - Frontend Next.js
- `backend/AccessConsole.Api/` - Backend ASP.NET Core
- `public/` - Aset statis frontend

## Run Frontend
Node.js dan npm
```powershell
npm.cmd ci
npm.cmd run dev
```
http://localhost:3000

## Run Backend
.NET 10 SDK
```powershell
dotnet run --project backend/AccessConsole.Api/AccessConsole.Api.csproj --launch-profile http
```
