using System.Diagnostics;
using System.Windows.Forms;

string[] cmdArgs = Environment.GetCommandLineArgs();
if (cmdArgs.Length < 2)
{
    // Launched directly (e.g. double-click) – show a friendly message instead of just closing.
    MessageBox.Show(
        "This helper is used internally by the Dependencies installer.\n\n" +
        "To install or check dependencies, run the main installer (DependenciesSetup.exe).",
        "Dependencies Helper",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    return;
}

string mode = cmdArgs[1].ToLowerInvariant();
int exitCode;

if (mode == "winget" && cmdArgs.Length >= 3)
{
    string packageId = cmdArgs[2];

    if (IsPythonPackage(packageId) && IsPythonInstalled())
    {
        MessageBox.Show(
            "Python is already installed.\n\nNo changes were made.",
            "Dependencies Helper",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        exitCode = 0;
    }
    else if (IsFfmpegPackage(packageId) && IsFfmpegInstalled())
    {
        MessageBox.Show(
            "FFmpeg is already installed.\n\nNo changes were made.",
            "Dependencies Helper",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        exitCode = 0;
    }
    else
    {
        exitCode = RunWinget(packageId);
    }
}
else if (mode == "pip" && cmdArgs.Length >= 3)
{
    string package = cmdArgs[2];

    if (IsYtDlpPackage(package) && IsYtDlpInstalled())
    {
        MessageBox.Show(
            "yt-dlp is already installed.\n\nNo changes were made.",
            "Dependencies Helper",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
        exitCode = 0;
    }
    else
    {
        exitCode = RunPip(package);
    }
}
else
{
    MessageBox.Show(
        "Usage:\n  SetupHelper winget <PackageId>\n  SetupHelper pip <package>",
        "Dependencies Helper",
        MessageBoxButtons.OK,
        MessageBoxIcon.Information);
    exitCode = 1;
}

Environment.Exit(exitCode);

static int RunWinget(string packageId)
{
    string wingetArgs = $"install {packageId} --silent --accept-source-agreements --accept-package-agreements";
    return RunProcess("winget", wingetArgs);
}

static int RunPip(string package)
{
    return RunProcess("python", $"-m pip install -U {package}");
}

static bool IsPythonPackage(string packageId) =>
    packageId.Equals("Python.Python.3.12", StringComparison.OrdinalIgnoreCase);

static bool IsFfmpegPackage(string packageId) =>
    packageId.Equals("Gyan.FFmpeg", StringComparison.OrdinalIgnoreCase);

static bool IsYtDlpPackage(string package) =>
    package.Equals("yt-dlp", StringComparison.OrdinalIgnoreCase);

static bool IsPythonInstalled() =>
    RunProcess("python", "--version") == 0;

static bool IsFfmpegInstalled() =>
    RunProcess("ffmpeg", "-version") == 0;

static bool IsYtDlpInstalled() =>
    RunProcess("python", "-m pip show yt-dlp") == 0;

static int RunProcess(string fileName, string arguments)
{
    try
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = false
            }
        };
        process.Start();
        process.WaitForExit((int)TimeSpan.FromMinutes(10).TotalMilliseconds);
        return process.ExitCode;
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Error: {ex.Message}");
        return 1;
    }
}
