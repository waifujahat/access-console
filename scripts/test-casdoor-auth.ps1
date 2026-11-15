$ErrorActionPreference = "Stop"
$AlamatBackend = "http://127.0.0.1:5080"

function DapatkanStatus {
    param(
        [string]$Metode,
        [string]$Endpoint,
        [string]$Token = ""
    )

    $Header = @{}

    if ($Token) {
        $Header["Authorization"] = "Bearer $Token"
    }

    try {
        $Parameter = @{
            Uri             = "$AlamatBackend$Endpoint"
            Method          = $Metode
            Headers         = $Header
            UseBasicParsing = $true
            ErrorAction     = "Stop"
        }

        if ($Metode -eq "POST") {
            $Parameter["ContentType"] = "application/json"
            $Parameter["Body"] = '{"kataSandi":null}'
        }

        $Respons = Invoke-WebRequest @Parameter

        return [int]$Respons.StatusCode
    }
    catch {
        if ($_.Exception.Response) {
            return [int]$_.Exception.Response.StatusCode
        }

        throw
    }
}

function UjiStatus {
    param(
        [string]$Nama,
        [int]$Aktual,
        [int]$Harapan
    )

    if ($Aktual -ne $Harapan) {
        throw "[FAIL] $Nama - HTTP $Aktual, expected $Harapan"
    }

    Write-Host "[PASS] $Nama - HTTP $Aktual" -ForegroundColor Green
}

UjiStatus "Backend health" `
    (DapatkanStatus "GET" "/health") 200

UjiStatus "Pengaitan tanpa autentikasi" `
    (DapatkanStatus "POST" "/api/auth/casdoor/link") 401

$Email = Read-Host "Email akun uji"
$Rahasia = Read-Host "Kata sandi akun uji" -AsSecureString

$KataSandi = [System.Net.NetworkCredential]::new(
    "",
    $Rahasia
).Password

try {
    $Sesi = Invoke-RestMethod `
        -Uri "$AlamatBackend/api/auth/login" `
        -Method POST `
        -ContentType "application/json" `
        -Body (@{
            email = $Email
            kataSandi = $KataSandi
        } | ConvertTo-Json)

    $Token = $Sesi.accessToken
}
finally {
    Remove-Variable KataSandi -ErrorAction SilentlyContinue
}

if (-not $Token) {
    throw "Backend tidak mengembalikan access token"
}

try {
    UjiStatus "Sesi lokal valid" `
        (DapatkanStatus "GET" "/api/accounts/me" $Token) 200

    UjiStatus "Pengaitan tanpa token Casdoor" `
        (DapatkanStatus "POST" "/api/auth/casdoor/link" $Token) 401

    Write-Host "Menunggu batas Recent Authentication..."

    Start-Sleep -Seconds 310

    UjiStatus "Autentikasi ulang diperlukan" `
        (DapatkanStatus "POST" "/api/auth/casdoor/link" $Token) 428
}
finally {
    $null = DapatkanStatus "POST" "/api/auth/logout" $Token
}

UjiStatus "Token setelah logout" `
    (DapatkanStatus "GET" "/api/accounts/me" $Token) 401

Write-Host "Seluruh pengujian berhasil" -ForegroundColor Green
