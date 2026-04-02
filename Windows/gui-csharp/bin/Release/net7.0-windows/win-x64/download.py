#!/usr/bin/env python3
import argparse
import os
import sys
from yt_dlp import YoutubeDL


def progress_hook(d):
    if d['status'] == 'downloading':
        pct = d.get('_percent_str') or d.get('percent')
        speed = d.get('_speed_str') or d.get('speed')
        eta = d.get('_eta_str') or d.get('eta')
        print(f"Downloading: {pct} {speed} ETA:{eta}", end='\r')
    elif d['status'] == 'finished':
        print('\nDownload finished, post-processing...')


def download(url: str, fmt: str, outdir: str, quality: str = 'best', out_format: str = None, samplerate: int = None):
    import subprocess
    import re

    os.makedirs(outdir, exist_ok=True)
    outtmpl = os.path.join(outdir, '%(title).200s.%(ext)s')

    audio_exts = {'mp3', 'aac', 'flac', 'aiff', 'wav', 'ogg'}
    video_exts = {'mp4', 'mov', 'mkv', 'avi'}

    # Decide whether we are targeting audio or video
    target = None
    if out_format:
        of = out_format.lower()
        if of in audio_exts:
            target = 'audio'
        elif of in video_exts:
            target = 'video'
        else:
            target = None

    # Choose download format
    if target == 'video' or (target is None and fmt == 'mp4'):
        # Download best video+audio
        if quality == 'best':
            fmt_str = 'bestvideo+bestaudio/best'
        else:
            fmt_str = f"bestvideo[height<={quality}]+bestaudio/best"

        ydl_opts = {
            'format': fmt_str,
            'outtmpl': outtmpl,
            'noplaylist': True,
            'progress_hooks': [progress_hook],
        }

    else:
        # audio
        ydl_opts = {
            'format': 'bestaudio/best',
            'outtmpl': outtmpl,
            'noplaylist': True,
            'progress_hooks': [progress_hook],
        }
        # If an audio conversion is requested, let yt-dlp perform extraction
        # via the FFmpegExtractAudio postprocessor so the original is removed
        # and only the requested audio file remains.
        if out_format and out_format.lower() in audio_exts:
            # determine preferred quality digits from provided quality (e.g., '320kbps' -> '320')
            pref = None
            if quality and quality != 'best':
                m = re.search(r"(\d+)", str(quality))
                if m:
                    pref = m.group(1)
            post = {
                'key': 'FFmpegExtractAudio',
                'preferredcodec': out_format.lower(),
            }
            if pref:
                post['preferredquality'] = pref
            ydl_opts['postprocessors'] = [post]
            # prefer ffmpeg when available
            ydl_opts['prefer_ffmpeg'] = True

    with YoutubeDL(ydl_opts) as ydl:
        info = ydl.extract_info(url, download=True)
        # Determine downloaded file path
        try:
            downloaded = ydl.prepare_filename(info)
        except Exception:
            # fallback: construct from title and ext
            title = info.get('title', 'video')
            ext = info.get('ext', 'mp4')
            downloaded = os.path.join(outdir, f"{title}.{ext}")

    # If out_format is not specified, no conversion requested
    if not out_format:
        return

    out_format = out_format.lower()
    base, _ = os.path.splitext(downloaded)
    dest = os.path.join(outdir, f"{os.path.basename(base)}.{out_format}")

    # If already the desired format, nothing to do
    if downloaded.lower().endswith('.' + out_format):
        return

    # If yt-dlp postprocessor already created the desired audio file, remove
    # the original download. If a samplerate was requested, resample the
    # produced file via ffmpeg so only the final requested file remains.
    if out_format in audio_exts:
        if os.path.exists(dest):
            try:
                if os.path.exists(downloaded) and os.path.abspath(downloaded) != os.path.abspath(dest):
                    os.remove(downloaded)
            except Exception:
                pass
            # If samplerate requested, resample dest
            if samplerate:
                codec_map = {'mp3': 'libmp3lame', 'aac': 'aac', 'flac': 'flac', 'wav': 'pcm_s16le', 'ogg': 'libvorbis', 'aiff': 'pcm_s16le'}
                codec = codec_map.get(out_format, None)
                tmp = dest + '.tmp'
                cmd = ['ffmpeg', '-y', '-i', dest, '-ar', str(samplerate)]
                if codec:
                    cmd += ['-c:a', codec]
                cmd += [tmp]
                try:
                    subprocess.run(cmd, check=True)
                    os.replace(tmp, dest)
                except FileNotFoundError:
                    raise RuntimeError('ffmpeg not found on PATH; install ffmpeg to enable sample-rate conversion')
                except subprocess.CalledProcessError as e:
                    raise RuntimeError(f'ffmpeg failed during sample-rate conversion: {e}')
            return

    # Convert using ffmpeg; require ffmpeg on PATH
    # For audio, strip video streams
    if out_format in audio_exts:
        if samplerate:
            cmd = ['ffmpeg', '-y', '-i', downloaded, '-vn', '-ar', str(samplerate), dest]
        else:
            cmd = ['ffmpeg', '-y', '-i', downloaded, '-vn', dest]
    else:
        # video conversion: re-encode to H.264 + AAC for broad compatibility
        cmd = ['ffmpeg', '-y', '-i', downloaded, '-c:v', 'libx264', '-c:a', 'aac', dest]

    try:
        subprocess.run(cmd, check=True)
        # after successful conversion, remove the original file if different
        try:
            if os.path.exists(downloaded) and os.path.abspath(downloaded) != os.path.abspath(dest):
                os.remove(downloaded)
        except Exception:
            pass
    except FileNotFoundError:
        raise RuntimeError('ffmpeg not found on PATH; install ffmpeg to enable format conversion')
    except subprocess.CalledProcessError as e:
        raise RuntimeError(f'ffmpeg failed: {e}')


def main():
    parser = argparse.ArgumentParser(description='Download a YouTube URL as MP4 or extract MP3 audio')
    parser.add_argument('url', nargs='?', help='YouTube video URL')
    parser.add_argument('--format', '-f', choices=['mp4', 'mp3'], default='mp4', help='Output format')
    parser.add_argument('--quality', '-q', default='best', help='Video quality (best, 1080, 720, 480, 360, 240)')
    parser.add_argument('--out', '-o', default='downloads', help='Output directory')
    parser.add_argument('--out-format', '-F', dest='out_format', help='Convert output to this format (e.g. mp3, mp4, aac)')
    parser.add_argument('--samplerate', type=int, help='Sample rate in Hz (e.g., 44100, 48000)')

    args = parser.parse_args()

    if not args.url:
        args.url = input('Enter YouTube video URL: ').strip()
        if not args.url:
            print('No URL provided, exiting.')
            sys.exit(1)

    try:
        download(args.url, args.format, args.out, args.quality, args.out_format, args.samplerate)
        print('Done.')
    except Exception as e:
        print('Error:', e)
        sys.exit(1)


if __name__ == '__main__':
    main()
