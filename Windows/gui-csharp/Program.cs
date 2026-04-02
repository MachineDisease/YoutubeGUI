using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace YouTubeGui
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            
            ApplicationConfiguration.Initialize();
            Application.Run(new MainForm());
        }
    }
}
