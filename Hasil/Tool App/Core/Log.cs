using System;
using System.IO;
using System.Text;

namespace CameoRescue.Core
{
    public static class Log
    {
        static readonly object gate = new object();
        static readonly StringBuilder buffer = new StringBuilder();
        static string file;

        public static string Dir => Path.Combine(AppContext.BaseDirectory, "logs");

        public static void Init()
        {
            try
            {
                Directory.CreateDirectory(Dir);
                file = Path.Combine(Dir, "cameorescue-" + DateTime.Now.ToString("yyyyMMdd-HHmmss") + ".log");
                Write("=== CAMEO 5 Rescue Tool ===");
                Write("waktu: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            }
            catch { }
        }

        public static void Write(string msg)
        {
            string line = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + msg;
            lock (gate)
            {
                buffer.AppendLine(line);
                try { if (file != null) File.AppendAllText(file, line + Environment.NewLine, Encoding.UTF8); } catch { }
            }
        }

        public static string Text { get { lock (gate) return buffer.ToString(); } }

        public static string CurrentFile => file;

        public static void SaveTo(string path)
        {
            lock (gate) File.WriteAllText(path, buffer.ToString(), Encoding.UTF8);
        }
    }
}
