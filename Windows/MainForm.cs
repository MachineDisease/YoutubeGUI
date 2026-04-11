using System;
using System.Diagnostics;
using System.Linq;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows.Forms;
using System.Drawing;
using Microsoft.VisualBasic.ApplicationServices;


namespace YouTubeGui
{
    public class MainForm : Form
    {
        TextBox urlBox;
        // legacy format selector removed (deprecated)
        ComboBox qualityBox;
        Label sampleRateLabel;
        ComboBox sampleRateBox;
        RadioButton audioRadio;
        RadioButton videoRadio;
        ComboBox audioFormatBox;
        ComboBox videoFormatBox;
        TextBox outBox;
        CheckBox playlistBox;
        CheckBox newFolderBox;
        TextBox playlistNameBox;
        Button downloadBtn;
        private string settingsPath;
        private void Application_Startup(Object sender, StartupEventArgs e) {
    DarkMode.DarkMode.SetAppTheme(DarkMode.DarkMode.Theme.SYSTEM);
}

        private class Settings
        {
            public string OutputFolder { get; set; }
        }
[DllImport("darkmode.dll", CharSet = CharSet.Unicode)]
        private static extern bool EnableDarkMode(IntPtr hWnd, bool enable);
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public MainForm()
        {
            Text = "YouTubeGUI";
            Width = 600;
            Height = 250;
            // Make the window fixed-size and disable maximize to avoid layout issues
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = true;
            StartPosition = FormStartPosition.CenterScreen;

            var lblUrl = new Label() { Text = "YouTube link:", Left = 10, Top = 20, Width = 80 };
            urlBox = new TextBox() { Left = 160, Top = 18, Width = 400 };


            // format selector removed — user selects Audio or Video instead
            // Sample rate selector (replaces deprecated format selector space)
            sampleRateLabel = new Label() { Text = "Sample Rate:", Left = 10, Top = 58, Width = 100 };
            sampleRateBox = new ComboBox() { Left = 120, Top = 56, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            sampleRateBox.Items.AddRange(new string[] {
                "Default",
                "8000 Hz",
                "11025 Hz",
                "16000 Hz",
                "22050 Hz",
                "32000 Hz",
                "44100 Hz",
                "48000 Hz",
                "88200 Hz",
                "96000 Hz",
                "176400 Hz",
                "192000 Hz"
            });
            sampleRateBox.SelectedIndex = 0;
            sampleRateBox.Enabled = false; // apply only for audio

            audioRadio = new RadioButton() { Text = "Audio", Left = 20, Top = 90, Width = 75 };
            videoRadio = new RadioButton() { Text = "Video", Left = 20, Top = 115, Width = 75 };

            var lblQuality = new Label() { Text = "Quality:", Left = 380, Top = 60, Width = 60 };
            // Define separate quality sets for video and audio
            var vidoeQualities = new Size(120, 30);
            var videoQualities = new string[] { "Highest", "2160P", "1440P", "1080P", "720P", "480P", "360P", "240P", "144P"};
            var audioQualities = new string[] { "Highest", "320kbps", "256kbps", "192kbps", "128kbps", "64kbps" };
            qualityBox = new ComboBox() { Left = 440, Top = 58, Width = 120, DropDownStyle = ComboBoxStyle.DropDownList };
            // default to video qualities
            qualityBox.Items.AddRange(videoQualities);
            qualityBox.SelectedIndex = 0;
            qualityBox.Visible = false; // hidden until audio/video selected

            // Hidden format selectors for audio/video modes
            audioFormatBox = new ComboBox() { Left = 100, Top = 88, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            audioFormatBox.Items.AddRange(new string[] { "MP3", "AAC", "FLAC", "AIFF", "WAV", "OGG" });
            audioFormatBox.SelectedIndex = 0;

            videoFormatBox = new ComboBox() { Left = 100, Top = 115, Width = 150, DropDownStyle = ComboBoxStyle.DropDownList, Visible = false };
            videoFormatBox.Items.AddRange(new string[] { "MP4", "MOV", "MKV", "AVI" });
            videoFormatBox.SelectedIndex = 0;

            audioRadio.CheckedChanged += (s, e) => {
                // RadioButtons are mutually exclusive by default in the same container.
                audioFormatBox.Visible = audioRadio.Checked;
                // Ensure video format visibility follows videoRadio
                videoFormatBox.Visible = videoRadio.Checked;
                // Show quality selector when a mode is selected
                if (audioRadio.Checked || videoRadio.Checked)
                {
                    qualityBox.Visible = true;
                }
                else
                {
                    qualityBox.Visible = false;
                }
                // Show/enable playlist checkbox only when a mode is selected
                playlistBox.Visible = (audioRadio.Checked || videoRadio.Checked);
                playlistBox.Enabled = (audioRadio.Checked || videoRadio.Checked);
                // show/hide new-folder controls alongside the playlist checkbox
                newFolderBox.Visible = playlistBox.Visible;
                newFolderBox.Enabled = false;
                playlistNameBox.Visible = playlistBox.Visible;
                playlistNameBox.Enabled = false;
                // show/hide new-folder controls alongside the playlist checkbox
                newFolderBox.Visible = playlistBox.Visible;
                newFolderBox.Enabled = false;
                playlistNameBox.Visible = playlistBox.Visible;
                playlistNameBox.Enabled = false;
                // Switch quality options to audio when audio mode selected
                if (audioRadio.Checked)
                {
                    qualityBox.Items.Clear();
                    qualityBox.Items.AddRange(audioQualities);
                    qualityBox.SelectedIndex = 0;
                    sampleRateBox.Enabled = true;
                }
                else if (videoRadio.Checked)
                {
                    qualityBox.Items.Clear();
                    qualityBox.Items.AddRange(videoQualities);
                    qualityBox.SelectedIndex = 0;
                    sampleRateBox.Enabled = false;
                }
            };
            videoRadio.CheckedChanged += (s, e) => {
                videoFormatBox.Visible = videoRadio.Checked;
                // Ensure audio format visibility follows audioRadio
                audioFormatBox.Visible = audioRadio.Checked;
                // Show/hide quality selector
                if (audioRadio.Checked || videoRadio.Checked) qualityBox.Visible = true; else qualityBox.Visible = false;
                // Show/enable playlist checkbox only when a mode is selected
                playlistBox.Visible = (audioRadio.Checked || videoRadio.Checked);
                playlistBox.Enabled = (audioRadio.Checked || videoRadio.Checked);
                // Switch quality options to video when video mode selected
                if (videoRadio.Checked)
                {
                    qualityBox.Items.Clear();
                    qualityBox.Items.AddRange(videoQualities);
                    qualityBox.SelectedIndex = 0;
                    sampleRateBox.Enabled = false;
                }
                else if (audioRadio.Checked)
                {
                    qualityBox.Items.Clear();
                    qualityBox.Items.AddRange(audioQualities);
                    qualityBox.SelectedIndex = 0;
                    sampleRateBox.Enabled = true;
                }
            };

            var lblOut = new Label() { Text = "Output", Left = 10, Top = 180, Width = 100 };
            outBox = new TextBox() { Left = 120, Top = 178, Width = 340, Text = "Downloads Folder (Made by program)" };
            var browseBtn = new Button() { Text = "Browse", Left = 235, Top = 152, Width = 90, Size = new Size(120, 30)  };
            // Place playlist checkbox to the right of the radio buttons / format selectors so it doesn't overlap
            playlistBox = new CheckBox() { Text = "Playlist (download all)", Left = 260, Top = 96, Width = 180 };
            // start hidden and disabled; it will appear and be enabled once user picks Audio or Video
            playlistBox.Visible = false;
            playlistBox.Enabled = false;
            // Checkbox to indicate that playlist should be placed in a new folder
            newFolderBox = new CheckBox() { Text = "Is in new folder", Left = 260, Top = 122, Width = 130, Visible = false, Enabled = false };
            // Textbox for optional custom playlist folder name. Appears when playlist selected but remains disabled
            // until "Is in new folder" is checked.
            playlistNameBox = new TextBox() { Left = 395, Top = 120, Width = 165, Visible = false, Enabled = false };
            browseBtn.Click += (s, e) => { using (var d = new FolderBrowserDialog()) { if (d.ShowDialog() == DialogResult.OK) { outBox.Text = d.SelectedPath; SaveSettings(); } } };
            outBox.Leave += (s, e) => SaveSettings();

            downloadBtn = new Button() { Text = "Download", Left = 470, Top = 175, Width = 100,Size = new Size(120, 30) };
            downloadBtn.Click += DownloadBtn_Click;

            // When playlist checkbox toggles, enable the new-folder option and show the name textbox
            playlistBox.CheckedChanged += (s, e) => {
                var isChecked = playlistBox.Checked;
                newFolderBox.Enabled = isChecked;
                playlistNameBox.Visible = isChecked;
                if (!isChecked)
                {
                    newFolderBox.Checked = false;
                    playlistNameBox.Enabled = false;
                }
            };
            newFolderBox.CheckedChanged += (s, e) => {
                playlistNameBox.Enabled = newFolderBox.Checked;
            };

            // Settings and icon load
            try
            {
                var exeDir = AppDomain.CurrentDomain.BaseDirectory;
                settingsPath = Path.Combine(exeDir, "settings.json");
                LoadSettings();

                var iconPath = Path.Combine(exeDir, "icon.ico");
                if (File.Exists(iconPath))
                {
                    this.Icon = new System.Drawing.Icon(iconPath);
                }
            }
            catch
            {
                // ignore icon/load errors
            }

            // Set placeholder (cue banner) for the URL textbox
            try
            {
                var placeholder = "https://youtube.com...";
                var handle = urlBox.Handle; // ensure handle created
                SendMessage(handle, EM_SETCUEBANNER, (IntPtr)1, placeholder);
            }
            catch
            {
                // ignore if cue banner not supported
            }

            Controls.Add(lblUrl);
            Controls.Add(urlBox);
            Controls.Add(sampleRateLabel);
            Controls.Add(sampleRateBox);
            // legacy format controls removed
            Controls.Add(lblQuality);
            Controls.Add(qualityBox);
            Controls.Add(lblOut);
            Controls.Add(outBox);
            Controls.Add(browseBtn);
            Controls.Add(playlistBox);
            Controls.Add(newFolderBox);
            Controls.Add(playlistNameBox);
            Controls.Add(audioRadio);
            Controls.Add(videoRadio);
            Controls.Add(audioFormatBox);
            Controls.Add(videoFormatBox);
            Controls.Add(downloadBtn);
        }

        private void DownloadBtn_Click(object? sender, EventArgs e)
        {
            var url = urlBox.Text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                MessageBox.Show("Please enter a YouTube URL.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var quality = qualityBox.SelectedItem?.ToString() ?? "Highest Quality";
            string qualityArg;
            if (quality == "Highest Quality")
            {
                qualityArg = "best";
            }
            else
            {
                // extract digits from values like "144P" or "320kbps"
                var digits = new string(quality.Where(char.IsDigit).ToArray());
                qualityArg = string.IsNullOrEmpty(digits) ? "best" : digits;
            }
            var samplerate = sampleRateBox.SelectedItem?.ToString() ?? "Default";
            string samplerateArg = null;
            if (audioRadio.Checked)
            {
                if (samplerate != "Default")
                {
                    var sdigits = new string(samplerate.Where(char.IsDigit).ToArray());
                    if (!string.IsNullOrEmpty(sdigits)) samplerateArg = sdigits;
                }
            }
            var outdir = outBox.Text.Trim();
            if (string.IsNullOrEmpty(outdir)) outdir = "Downloads)";

            // When published, download.py is copied to the same folder as the exe (see csproj ItemGroup)
            var exeDir = AppDomain.CurrentDomain.BaseDirectory;
            var scriptPath = Path.Combine(exeDir, "download.py");
            scriptPath = Path.GetFullPath(scriptPath);

            if (!File.Exists(scriptPath))
            {
                MessageBox.Show($"download.py not found at {scriptPath}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Find a python executable to run: try `python`, then `py`.
            string pythonExe = null;
            try
            {
                ProcessStartInfo check = new ProcessStartInfo("python", "--version") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
                var p = Process.Start(check);
                p.WaitForExit(2000);
                if (!p.HasExited || p.ExitCode == 0) pythonExe = "python";
            }
            catch { }
            if (pythonExe == null)
            {
                try
                {
                    ProcessStartInfo check2 = new ProcessStartInfo("py", "--version") { UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true };
                    var p2 = Process.Start(check2);
                    p2.WaitForExit(2000);
                    if (!p2.HasExited || p2.ExitCode == 0) pythonExe = "py";
                }
                catch { }
            }

            if (pythonExe == null)
            {
                MessageBox.Show("Python was not found on PATH. Please install Python and yt-dlp (run install_deps.bat), then try again.", "Python Not Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Build python argument list(s) (we'll start python directly rather than via cmd.exe)
            var tasksArgs = new System.Collections.Generic.List<string>();
            if (videoRadio.Checked)
            {
                var vfmt = (videoFormatBox.SelectedItem?.ToString() ?? "MP4").ToLowerInvariant();
                var args = $"\"{scriptPath}\" \"{url}\" --format mp4 --quality {qualityArg} --out \"{outdir}\" --out-format {vfmt}";
                if (playlistBox.Checked) {
                    args += " --playlist";
                    if (newFolderBox.Checked) {
                        var pname = playlistNameBox.Text.Trim();
                        if (!string.IsNullOrEmpty(pname)) args += $" --playlist-folder \"{pname}\"";
                        else args += " --playlist-folder";
                    }
                }
                tasksArgs.Add(args);
            }
            if (audioRadio.Checked)
            {
                var afmt = (audioFormatBox.SelectedItem?.ToString() ?? "MP3").ToLowerInvariant();
                var args = $"\"{scriptPath}\" \"{url}\" --format mp3 --quality {qualityArg} --out \"{outdir}\" --out-format {afmt}";
                if (!string.IsNullOrEmpty(samplerateArg)) args += $" --samplerate {samplerateArg}";
                if (playlistBox.Checked) {
                    args += " --playlist";
                    if (newFolderBox.Checked) {
                        var pname = playlistNameBox.Text.Trim();
                        if (!string.IsNullOrEmpty(pname)) args += $" --playlist-folder \"{pname}\"";
                        else args += " --playlist-folder";
                    }
                }
                tasksArgs.Add(args);
            }
            if (!videoRadio.Checked && !audioRadio.Checked)
            {
                // default to mp4 when no explicit selection
                var args = $"\"{scriptPath}\" \"{url}\" --format mp4 --quality {qualityArg} --out \"{outdir}\"";
                if (playlistBox.Checked) {
                    args += " --playlist";
                    if (newFolderBox.Checked) {
                        var pname = playlistNameBox.Text.Trim();
                        if (!string.IsNullOrEmpty(pname)) args += $" --playlist-folder \"{pname}\"";
                        else args += " --playlist-folder";
                    }
                }
                tasksArgs.Add(args);
            }

            try
            {
                foreach (var args in tasksArgs)
                {
                    var psi = new ProcessStartInfo(pythonExe, args)
                    {
                        UseShellExecute = true,
                        WindowStyle = ProcessWindowStyle.Minimized,
                        WorkingDirectory = Path.GetDirectoryName(scriptPath) ?? exeDir
                    };
                    Process.Start(psi);
                }
                MessageBox.Show("Download(s) started in console window(s).", "Started", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Failed to start download: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SaveSettings()
        {
            try
            {
                if (string.IsNullOrEmpty(settingsPath)) return;
                var s = new Settings()
                {
                    OutputFolder = outBox?.Text ?? ""
                };
                var opts = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(s, opts);
                File.WriteAllText(settingsPath, json);
            }
            catch
            {
                // ignore save errors
            }
        }

        private void LoadSettings()
        {
            try
            {
                if (string.IsNullOrEmpty(settingsPath)) return;
                if (!File.Exists(settingsPath)) return;
                var json = File.ReadAllText(settingsPath);
                var s = JsonSerializer.Deserialize<Settings>(json);
                if (s != null && !string.IsNullOrEmpty(s.OutputFolder))
                {
                    outBox.Text = s.OutputFolder;
                }
            }
            catch
            {
                // ignore load errors
            }
        }
    }
}
