using System;
using System.Windows.Forms;

namespace CameoRescue
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            try { Application.SetHighDpiMode(HighDpiMode.PerMonitorV2); } catch { }
            Application.Run(new MainForm());
        }
    }
}
