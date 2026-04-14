
![Logo](https://github.com/MachineDisease/YoutubeGUI/blob/main/Windows/Icon.png?raw=true)



# YoutubeGUI

### "The most mid thing since gravity" - Me

### "Probably the worst thing I've ever used" - New York Times

## NOTICE: MAC AND LINUX VERSIONS ARE NOT RELEASED YET.
## Screenshots

![App Screenshot](https://i.ibb.co/j9JX7PyV/806619-A9-1110-47-A6-96-EE-97113589852-F.png)
![App Screenshot](https://i.ibb.co/F4DqcCpc/793208-C9-4409-4-F4-D-A55-C-E6-A6543309-AA.png)




## Downloads

![GitHub Downloads (all assets, latest release)](https://img.shields.io/github/downloads/MachineDisease/YoutubeGUI/latest/total?style=plastic&logo=Github&logoColor=%23101411&label=Releases&color=%23B6BFB8&link=https%3A%2F%2Fgithub.com%2FMachineDisease%2FYoutubeGUI%2Freleases)

(For some reason download emblem is broken, click on releases)

## Installation

### Linux
Install Dependencies
```bash
  #Dependencies - FFMPEG, Python, yt-dlp (via Python)
  sudo apt install FFMPEG
  && sudo apt install python3
  && sudo apt install python3-pip
  && pip install yt-dlp
```
Test if all is installed correctly.
 ```bash
    #Check for proper install
    python --version
    yt-dlp --version
    ffmpeg --version
```
If any commands return an unknown command, check your install!
If all are installed correctly, download the .deb!

### Windows

- Run the dependency installer included in the release zip
    - If it fails, check your internet or install them manually!
    ```bash
  #Manual install <- Requires leveraged PowerShell! (Admin)
  winget install ffmpeg
  winget install python
  pip install yt-dlp
    ```
- Run YoutubeGUI!


## Troubleshooting

### Mac 

 ```bash
¯\_(ツ)_/¯
```
Refer to Linux, some things may be similar.
### Windows

- Instant crash/No Launch at all 
    - Missing dependencies
        - Missing .NET runtime (Comes with most Windows computers, can be downloaded from here if needed https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

        - Missing FFMPEG, Python, or yt-dlp, refer to installation!

- No download (Python console doesn't open) 
    - Python most likely not installed (or installed incorrectly/corrupted)
    - Check internet connection (May be failing to contact Youtube)
    - Console closes instantly after pressing "Download" invalid link/invalid download options (Causing the download console to istantly crash.) Check download options.
- Other issues with Python console 
    - Run download.py manually with desired download options, this will prevent the console from closing after an error, do not double click it
        - Use cmd to launch download.py, as running it via cmd will cause the error to stay in the window until closed, unlike running it directly.
