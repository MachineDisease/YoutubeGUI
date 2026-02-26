# Build DependenciesSetup.exe (WiX Burn bundle for Python, FFmpeg, yt-dlp)
# Requires: .NET 6+ SDK, WiX Toolset v3 (e.g. winget install WiXToolset.WiXToolset)
$ErrorActionPreference = "Stop"
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
Set-Location $ScriptDir
$OutputExe = Join-Path $ScriptDir "DependenciesSetup.exe"

Write-Host "Working directory: $ScriptDir" -ForegroundColor Gray
Write-Host "Build output will be: $OutputExe" -ForegroundColor Cyan
Write-Host ""

$BuildTemp = Join-Path $ScriptDir "_buildtemp"

try {
    # 1. Publish SetupHelper to a temp folder (no bin/obj left in script folder)
    Write-Host "Publishing SetupHelper (single exe)..."
    $setupHelperDir = Join-Path $ScriptDir "SetupHelper"
    if (Test-Path $BuildTemp) { Remove-Item $BuildTemp -Recurse -Force }
    dotnet publish $setupHelperDir -c Release -o $BuildTemp
    if ($LASTEXITCODE -ne 0) { throw "SetupHelper publish failed." }
    # Copy only the exe into SetupHelper dir so WiX can find it
    $helperExe = Join-Path $BuildTemp "SetupHelper.exe"
    if (-not (Test-Path $helperExe)) { throw "SetupHelper.exe was not produced in publish output." }
    Copy-Item $helperExe -Destination $setupHelperDir -Force

# 2. Find WiX 3 (candle + light)
$wixPaths = @(
    "${env:ProgramFiles(x86)}\WiX Toolset v3.11\bin",
    "${env:ProgramFiles}\WiX Toolset v3.11\bin"
)
$wixBin = $null
foreach ($p in $wixPaths) {
    if (Test-Path (Join-Path $p "candle.exe")) {
        $wixBin = $p
        break
    }
}
if (-not $wixBin) {
    # Fall back to PATH
    $candleCmd = Get-Command candle -ErrorAction SilentlyContinue
    if ($candleCmd) { $wixBin = Split-Path $candleCmd.Source -Parent }
}
if (-not $wixBin -or -not (Test-Path (Join-Path $wixBin "candle.exe"))) {
        throw "WiX Toolset v3.11 not found. Make sure it's installed (candle.exe not found)."
    }
Write-Host "Using WiX v3 at: $wixBin" -ForegroundColor Gray

$candle = Join-Path $wixBin "candle.exe"
$light  = Join-Path $wixBin "light.exe"

# 3. Compile Bundle.wxs
Write-Host "Compiling Bundle.wxs with candle..."
& $candle -ext WixBalExtension Bundle.wxs
if ($LASTEXITCODE -ne 0) { throw "candle failed." }

# 4. Link to DependenciesSetup.exe
Write-Host "Linking DependenciesSetup.exe with light..."
& $light -ext WixBalExtension -out $OutputExe Bundle.wixobj
if ($LASTEXITCODE -ne 0) { throw "light failed." }

    # Cleanup: temp folder, wixobj, and SetupHelper bin/obj (only DependenciesSetup.exe + source + SetupHelper.exe remain)
    Remove-Item $BuildTemp -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $setupHelperDir "bin") -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $setupHelperDir "obj") -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host ""
    Write-Host "Done. Single installer created (no bin/obj left behind):" -ForegroundColor Green
    Write-Host "  $OutputExe" -ForegroundColor Green
    if (Test-Path $OutputExe) {
        Write-Host "  (File exists, $(Get-Item $OutputExe | Select-Object -ExpandProperty Length) bytes)" -ForegroundColor Gray
    }
}
catch {
    Write-Error $_.Exception.Message
    # Clean up any partial build artifacts
    Remove-Item $BuildTemp -Recurse -Force -ErrorAction SilentlyContinue
    Remove-Item (Join-Path $ScriptDir "Bundle.wixobj") -Force -ErrorAction SilentlyContinue
    Read-Host -Prompt "Press Enter to close."
    exit 1
}

Read-Host -Prompt "Press Enter to close."
