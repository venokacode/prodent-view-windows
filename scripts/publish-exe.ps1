param(
    [ValidateSet("win-x64", "win-arm64")]
    [string]$Runtime = "win-x64",

    [ValidateSet("Release", "Debug")]
    [string]$Configuration = "Release",

    [switch]$NoZip,

    [string]$CertificatePath = "",

    [string]$CertificatePassword = "",

    [string]$TimestampUrl = "http://timestamp.digicert.com"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
$projectPath = Join-Path $repoRoot "src\ProDentView.Win\ProDentView.Win.csproj"
$artifactsRoot = Join-Path $repoRoot "artifacts\windows-exe"
$publishDir = Join-Path $artifactsRoot $Runtime
$zipPath = Join-Path $artifactsRoot "ProDENT-View-Windows-$Runtime.zip"
$hashPath = "$zipPath.sha256"
$exePath = Join-Path $publishDir "ProDENT View.exe"

if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    throw "dotnet was not found. Install the .NET 8 SDK before publishing."
}

New-Item -ItemType Directory -Force -Path $publishDir | Out-Null

dotnet restore $projectPath
dotnet publish $projectPath `
    -c $Configuration `
    -r $Runtime `
    --self-contained true `
    -o $publishDir `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -p:DebugType=none `
    -p:DebugSymbols=false

if (-not (Test-Path $exePath)) {
    throw "Publish completed but expected EXE was not found: $exePath"
}

Copy-Item (Join-Path $repoRoot "LICENSE") (Join-Path $publishDir "LICENSE") -Force
Copy-Item (Join-Path $repoRoot "THIRD_PARTY_NOTICES.md") (Join-Path $publishDir "THIRD_PARTY_NOTICES.md") -Force
Copy-Item (Join-Path $repoRoot "README.md") (Join-Path $publishDir "README-Windows.md") -Force

if ($CertificatePath -ne "") {
    $signtool = Get-Command signtool.exe -ErrorAction SilentlyContinue
    if (-not $signtool) {
        throw "signtool.exe was not found. Install the Windows SDK or run from a Developer PowerShell."
    }

    $signtoolPath = $signtool.Source
    if ($CertificatePassword -eq "") {
        & $signtoolPath sign /fd SHA256 /tr $TimestampUrl /td SHA256 /f $CertificatePath $exePath
    } else {
        & $signtoolPath sign /fd SHA256 /tr $TimestampUrl /td SHA256 /f $CertificatePath /p $CertificatePassword $exePath
    }
}

if (-not $NoZip) {
    if (Test-Path $zipPath) {
        Remove-Item $zipPath -Force
    }
    if (Test-Path $hashPath) {
        Remove-Item $hashPath -Force
    }

    Compress-Archive -Path (Join-Path $publishDir "*") -DestinationPath $zipPath -Force
    $hash = Get-FileHash -Algorithm SHA256 $zipPath
    "$($hash.Hash)  $(Split-Path -Leaf $zipPath)" | Set-Content -Encoding ASCII $hashPath
}

Write-Host "Published EXE: $exePath"
if (-not $NoZip) {
    Write-Host "Published ZIP: $zipPath"
    Write-Host "SHA256: $hashPath"
}
