# Dependencies Installer (Python, FFmpeg, yt-dlp)

Single .exe installer that installs:

1. **Python** (via winget: `Python.Python.3.12`)
2. **FFmpeg** (via winget: `Gyan.FFmpeg`)
3. **yt-dlp** (via pip: `python -m pip install -U yt-dlp`)

Requires internet and [winget](https://learn.microsoft.com/en-us/windows/package-manager/winget/). Option 1 from the plan: no bundled binaries.

## Prerequisites

- **.NET 6 SDK** (or later)
- **WiX Toolset v3**  
  Install: `winget install WiXToolset.WiXToolset`

## Build

From this folder run:

```powershell
.\build.ps1
```

Output: `DependenciesSetup.exe` in the same folder.

## Run

Run `DependenciesSetup.exe`. It will request elevation and then run winget/pip in order. If a dependency is already installed, winget/pip will no-op or upgrade as appropriate.
