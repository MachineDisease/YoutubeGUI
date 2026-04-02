@echo off
rem Build the C# WinForms launcher using dotnet
dotnet publish -c Release -r win-x64 -p:PublishSingleFile=true -p:PublishTrimmed=false -o publish
if %errorlevel% neq 0 (
  echo Build failed.
  pause
  exit /b %errorlevel%
)
echo Build succeeded. Exe is in publish folder.
rem Attempt to create a shortcut in the parent ytproject folder
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0..\create_shortcut.ps1"
pause
