@echo off
setlocal enabledelayedexpansion
echo Building MSI installer for Connor's Ultimate Video Downloader

REM Paths
set PROJECT_DIR=%~dp0
set PUBLISH_DIR=%PROJECT_DIR%publish
set INSTALLER_DIR=%PROJECT_DIR%installer
set BUILD_DIR=%INSTALLER_DIR%\build
set WXS=%INSTALLER_DIR%\YouTubeGui.wxs
set OUTPUT_MSI=%PROJECT_DIR%YouTubeGui.msi

if not exist "%PUBLISH_DIR%" (
  echo Publish folder not found: "%PUBLISH_DIR%"
  echo Please run `dotnet publish` first to create the single-file exe in the publish folder.
  pause
  exit /b 1
)

if not exist "%WXS%" (
  echo WiX source file not found: "%WXS%"
  pause
  exit /b 1
)

REM Find WiX tools (candle.exe and light.exe)
where candle.exe >nul 2>&1
if errorlevel 1 (
  echo "candle.exe" not found in PATH. Please install WiX Toolset (https://wixtoolset.org/) and ensure its bin folder is on PATH.
  pause
  exit /b 1
)

where light.exe >nul 2>&1
if errorlevel 1 (
  echo "light.exe" not found in PATH. Please install WiX Toolset (https://wixtoolset.org/) and ensure its bin folder is on PATH.
  pause
  exit /b 1
)

REM Prepare build folder and copy published files (you must publish first)





























endlocalpausepopd
necho MSI built: %OUTPUT_MSI%)  exit /b 1  pause  popd  echo light.exe failed.if errorlevel 1 (light.exe -ext WixUIExtension -out "%OUTPUT_MSI%" "YouTubeGui.wixobj"
necho Running light.exe...)  exit /b 1  pause  popd  echo candle.exe failed.if errorlevel 1 (candle.exe -arch x64 -out "YouTubeGui.wixobj" "YouTubeGui.wxs"
necho Running candle.exe...pushd "%INSTALLER_DIR%"REM Change to installer folder to run WiX commandsxcopy "%PUBLISH_DIR%\*" "%BUILD_DIR%\" /Y /E >nulecho Copying published files to build folder...mkdir "%BUILD_DIR%"rmdir /s /q "%BUILD_DIR%" 2>nulndel /q "%BUILD_DIR%\*" 2>nul