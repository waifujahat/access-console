$ErrorActionPreference = "Stop"

$AkarProyek = Split-Path -Parent $PSScriptRoot
$FileEnv = Join-Path $AkarProyek ".env.postgres"

if (-not (Test-Path $FileEnv)) {
    throw "File .env.postgres tidak ditemukan"
}

$Baris = Get-Content $FileEnv |
    Where-Object {
        $_.StartsWith("ACCESS_CONSOLE_DB_PASSWORD=")
    } |
    Select-Object -First 1

if (-not $Baris) {
    throw "Password PostgreSQL belum dikonfigurasi"
}

$Sandi = $Baris.Substring(
    "ACCESS_CONSOLE_DB_PASSWORD=".Length
).Trim()

if ([string]::IsNullOrWhiteSpace($Sandi)) {
    throw "Password PostgreSQL tidak boleh kosong"
}

$KoneksiSebelumnya = $env:ConnectionStrings__AccessConsole

try {
    $env:ConnectionStrings__AccessConsole = "Host=127.0.0.1;Port=5433;Database=access_console;Username=access_console;Password=$Sandi"

    Remove-Variable Sandi

    Push-Location $AkarProyek

    try {
        Write-Host "Menjalankan Access Console Backend" -ForegroundColor Cyan

        dotnet run `
            --project backend/AccessConsole.Api/AccessConsole.Api.csproj `
            --no-launch-profile `
            --urls http://127.0.0.1:5080
    }
    finally {
        Pop-Location
    }
}
finally {
    $env:ConnectionStrings__AccessConsole = $KoneksiSebelumnya
}
