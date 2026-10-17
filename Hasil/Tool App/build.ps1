# Build CAMEO 5 Rescue Tool -> satu file exe (self-contained)
# Jalankan:  powershell -ExecutionPolicy Bypass -File build.ps1

$ErrorActionPreference = "Stop"
$here = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location -LiteralPath $here

Write-Host "== Membersihkan output lama ==" -ForegroundColor Cyan
Remove-Item -Recurse -Force "$here\publish" -ErrorAction SilentlyContinue

Write-Host "== Publish (self-contained, single file) ==" -ForegroundColor Cyan
dotnet publish "$here\CameoRescue.csproj" -c Release -r win-x64 `
    -p:PublishSingleFile=true `
    -p:SelfContained=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:EnableCompressionInSingleFile=true `
    -o "$here\publish"

if ($LASTEXITCODE -ne 0) { throw "Publish gagal (exit $LASTEXITCODE)" }

$exe = Join-Path $here "publish\CameoRescue.exe"
if (Test-Path $exe) {
    $size = [math]::Round((Get-Item $exe).Length / 1MB, 1)
    Write-Host "== Selesai ==" -ForegroundColor Green
    Write-Host "Exe   : $exe"
    Write-Host "Ukuran: $size MB"
} else {
    throw "CameoRescue.exe tidak ditemukan di folder publish."
}
