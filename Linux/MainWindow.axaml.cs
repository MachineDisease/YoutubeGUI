using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace GuiAvalonia
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AudioFormat.Items = new[] { "mp3", "aac", "flac", "aiff", "wav", "ogg" };
            VideoFormat.Items = new[] { "mp4", "mov", "mkv", "avi" };
            QualityBox.Items = new[] { "Highest", "2160", "1440", "1080", "720", "480", "360", "240", "144" };
            QualityBox.SelectedIndex = 0;
            SampleRateBox.Items = new[] { "Default", "8000", "11025", "16000", "22050", "32000", "44100", "48000", "88200", "96000", "176400", "192000" };
            SampleRateBox.SelectedIndex = 0;

            BrowseBtn.Click += BrowseBtn_Click;
            DownloadBtn.Click += DownloadBtn_Click;
        }

        private string? FindYtProjectDir()
        {
            var dir = AppContext.BaseDirectory;
            var di = new DirectoryInfo(dir);
            for (int i = 0; i < 12; i++)
            {
                var candidate = Path.Combine(di.FullName, "..", "..", "..", "ytproject");
                var full = Path.GetFullPath(candidate);
                if (Directory.Exists(full)) return full;
                if (di.Parent == null) break;
                di = di.Parent;
            }
            var cwdCandidate = Path.Combine(Directory.GetCurrentDirectory(), "ytproject");
            if (Directory.Exists(cwdCandidate)) return cwdCandidate;
            return null;
        }

        private void BrowseBtn_Click(object? sender, RoutedEventArgs e)
        {
            var dlg = new OpenFolderDialog();
            var result = dlg.ShowAsync(this);
            result.ContinueWith(t =>
            {
                if (!t.IsFaulted && !string.IsNullOrEmpty(t.Result))
                {
                    Dispatcher.UIThread.Post(() => OutBox.Text = t.Result);
                }
            });
        }

        private void DownloadBtn_Click(object? sender, RoutedEventArgs e)
        {
            var url = UrlBox.Text?.Trim() ?? "";
            if (string.IsNullOrEmpty(url))
            {
                var diag = new Window { Content = new TextBlock { Text = "Please enter a YouTube URL." }, Width = 300, Height = 120 };
                diag.ShowDialog(this);
                return;
            }

            var quality = QualityBox.SelectedItem?.ToString() ?? "Highest";
            string qualityArg = quality == "Highest" ? "best" : new string(quality.Where(char.IsDigit).ToArray());
            if (string.IsNullOrEmpty(qualityArg)) qualityArg = "best";

            var samplerate = SampleRateBox.SelectedItem?.ToString() ?? "Default";
            string? samplerateArg = null;
            if (samplerate != "Default") samplerateArg = new string(samplerate.Where(char.IsDigit).ToArray());

            var outdir = OutBox.Text?.Trim();
            if (string.IsNullOrEmpty(outdir)) outdir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");

            var ytDir = FindYtProjectDir();
            if (ytDir == null)
            {
                var dlg = new Window { Content = new TextBlock { Text = "Could not locate ytproject folder. Run app from repository root." }, Width = 400, Height = 140 };
                dlg.ShowDialog(this);
                return;
            }

            var scriptPath = Path.Combine(ytDir, "download.py");
            if (!File.Exists(scriptPath))
            {
                var dlg = new Window { Content = new TextBlock { Text = $"download.py not found at {scriptPath}" }, Width = 400, Height = 140 };
                dlg.ShowDialog(this);
                return;
            }

            // Build arguments
            var tasks = new System.Collections.Generic.List<string>();
            if (VideoRadio.IsChecked == true)
            {
                var vfmt = VideoFormat.SelectedItem?.ToString() ?? "mp4";
                var vargs = $"\"{scriptPath}\" \"{url}\" --format mp4 --quality {qualityArg} --out \"{outdir}\" --out-format {vfmt}";
                // If URL looks like a playlist, allow playlist downloading
                if (url.Contains("list=") || url.ToLower().Contains("playlist")) vargs += " --playlist";
                tasks.Add(vargs);
            }
            if (AudioRadio.IsChecked == true)
            {
                var afmt = AudioFormat.SelectedItem?.ToString() ?? "mp3";
                var args = $"\"{scriptPath}\" \"{url}\" --format mp3 --quality {qualityArg} --out \"{outdir}\" --out-format {afmt}";
                // If URL looks like a playlist, allow playlist downloading
                if (url.Contains("list=") || url.ToLower().Contains("playlist")) args += " --playlist";
                if (!string.IsNullOrEmpty(samplerateArg)) args += $" --samplerate {samplerateArg}";
                tasks.Add(args);
            }
            if (tasks.Count == 0)
            {
                tasks.Add($"\"{scriptPath}\" \"{url}\" --format mp4 --quality {qualityArg} --out \"{outdir}\"");
            }

            // Try python3
            var python = "python3";
            try
            {
                var check = new ProcessStartInfo(python, "--version") { RedirectStandardOutput = true, RedirectStandardError = true, UseShellExecute = false };
                using var p = Process.Start(check);
                p.WaitForExit(2000);
                if (p.ExitCode != 0) python = "python";
            }
            catch { python = "python"; }

            try
            {
                foreach (var args in tasks)
                {
                    var psi = new ProcessStartInfo(python, args)
                    {
                        UseShellExecute = false,
                        WorkingDirectory = ytDir
                    };
                    Process.Start(psi);
                }

                var ok = new Window { Content = new TextBlock { Text = "Downloads started." }, Width = 300, Height = 120 };
                ok.ShowDialog(this);
            }
            catch (Exception ex)
            {
                var dlg = new Window { Content = new TextBlock { Text = "Failed to start download: " + ex.Message }, Width = 400, Height = 160 };
                dlg.ShowDialog(this);
            }
        }
    }
}
