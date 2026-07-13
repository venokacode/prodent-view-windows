param(
    [string]$CertificateThumbprint = "B96114D7FABC8B80D595ED83C3F54C6BE8D5DA4E",
    [string]$TimestampUrl = "http://timestamp.sectigo.com",
    [string]$InnoSetupPath = "$env:LOCALAPPDATA\Programs\Inno Setup 6\ISCC.exe",
    [string]$SignToolPath = "C:\Program Files (x86)\Windows Kits\10\bin\10.0.26100.0\x64\signtool.exe"
)

$ErrorActionPreference = "Stop"

$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptRoot
$publishScript = Join-Path $scriptRoot "publish-exe.ps1"
$appExe = Join-Path $repoRoot "artifacts\windows-exe\win-x64\ProDENT View.exe"
$installerScript = Join-Path $repoRoot "installer\ProDENTViewStore.iss"
$installerExe = Join-Path $repoRoot "installer\Output\ProDENTView-1.0.0.0-Store-Setup.exe"
$hashPath = "$installerExe.sha256"

foreach ($requiredPath in @($publishScript, $InnoSetupPath, $SignToolPath, $installerScript)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required file was not found: $requiredPath"
    }
}

& $publishScript -Runtime win-x64 -Configuration Release -NoZip
if ($LASTEXITCODE -ne 0) {
    throw "Self-contained publish failed with exit code $LASTEXITCODE."
}

& $SignToolPath sign /fd SHA256 /tr $TimestampUrl /td SHA256 /sha1 $CertificateThumbprint $appExe
if ($LASTEXITCODE -ne 0) {
    throw "Application signing failed with exit code $LASTEXITCODE."
}

& $InnoSetupPath $installerScript
if ($LASTEXITCODE -ne 0) {
    throw "Installer compilation failed with exit code $LASTEXITCODE."
}

& $SignToolPath sign /fd SHA256 /tr $TimestampUrl /td SHA256 /sha1 $CertificateThumbprint $installerExe
if ($LASTEXITCODE -ne 0) {
    throw "Installer signing failed with exit code $LASTEXITCODE."
}

foreach ($signedFile in @($appExe, $installerExe)) {
    & $SignToolPath verify /pa /all /v $signedFile
    if ($LASTEXITCODE -ne 0) {
        throw "Signature verification failed: $signedFile"
    }
}

$hash = Get-FileHash -Algorithm SHA256 $installerExe
"$($hash.Hash)  $(Split-Path -Leaf $installerExe)" | Set-Content -Encoding ASCII $hashPath

Write-Host "Store installer: $installerExe"
Write-Host "SHA256: $($hash.Hash)"
Write-Host "Hash file: $hashPath"
