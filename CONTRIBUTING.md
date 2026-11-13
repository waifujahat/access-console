# Contributing to Access Console

Terima kasih telah mempertimbangkan kontribusi untuk Access Console. Proyek ini dibuat sebagai contoh aplikasi fullstack Identity and Access Management. Perubahan sebaiknya terarah, dapat diuji, dan mempertahankan perilaku keamanan yang sudah ada.

## Sebelum mengusulkan perubahan

- Baca [README.md](README.md) untuk memahami arsitektur, API, dan lingkungan lokal.
- Periksa issue atau pull request yang sudah ada agar tidak menduplikasi pekerjaan.
- Untuk perubahan besar, jelaskan motivasi dan ruang lingkup melalui issue terlebih dahulu.
- Jangan membuka issue publik untuk kerentanan keamanan. Ikuti [SECURITY.md](SECURITY.md).

## Menyiapkan lingkungan

Gunakan Node.js 24, .NET SDK 10, PostgreSQL 17, dan Docker Compose untuk pengembangan lokal. Petunjuk menjalankan frontend, backend, dan migrasi database tersedia di [README.md](README.md).

## Verifikasi perubahan

Jalankan pemeriksaan yang relevan sebelum membuat pull request:

```powershell
npm ci
npm run lint
npm run build

dotnet restore backend/AccessConsole.Api/AccessConsole.Api.csproj
dotnet format backend/AccessConsole.Api/AccessConsole.Api.csproj --verify-no-changes --no-restore
dotnet build backend/AccessConsole.Api/AccessConsole.Api.csproj --configuration Release

git diff --check
```

GitHub Actions menjalankan pemeriksaan frontend, backend, migrasi database, dan pengujian alur autentikasi dasar. Perubahan pada autentikasi, data pengguna, atau konfigurasi keamanan harus disertai penjelasan bagaimana perilaku tersebut diverifikasi.

## Pedoman kontribusi

- Ikuti konvensi TypeScript, Next.js, C#, dan ASP.NET Core yang sudah digunakan di proyek.
- Pisahkan perubahan berdasarkan tanggung jawab dan hindari refactor yang tidak terkait.
- Jangan menyertakan kredensial, token, sertifikat, connection string produksi, atau data pribadi.
- Perbarui dokumentasi apabila perilaku publik, API, konfigurasi, atau proses menjalankan aplikasi berubah.
- Gunakan judul commit yang singkat dan natural, serta jelaskan alasan perubahan pada pull request.

## Pull request

Jelaskan tujuan, ruang lingkup, pengujian, dan risiko perubahan. Untuk perubahan UI, tambahkan tangkapan layar jika relevan. Kontribusi kode asli mengikuti lisensi proyek pada [LICENSE](LICENSE), kecuali dinyatakan lain dan disepakati sebelum digabungkan.
