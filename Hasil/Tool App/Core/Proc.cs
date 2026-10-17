using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Text;

namespace CameoRescue.Core
{
    public static class Proc
    {
        public static bool IsAdmin()
        {
            try
            {
                using var id = WindowsIdentity.GetCurrent();
                return new WindowsPrincipal(id).IsInRole(WindowsBuiltInRole.Administrator);
            }
            catch { return false; }
        }

        public static (int code, string output) Run(string exe, string args, int timeoutMs = 30000)
        {
            try
            {
                var psi = new ProcessStartInfo(exe, args)
                {
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                };
                using var p = Process.Start(psi);
                string stdout = p.StandardOutput.ReadToEnd();
                string stderr = p.StandardError.ReadToEnd();
                if (!p.WaitForExit(timeoutMs)) { try { p.Kill(); } catch { } return (-1, "timeout"); }
                string outp = string.IsNullOrWhiteSpace(stdout) ? stderr : stdout;
                return (p.ExitCode, outp.Trim());
            }
            catch (Exception ex)
            {
                return (-1, ex.Message);
            }
        }

        /// <summary>Jalankan skrip PowerShell lewat file sementara.</summary>
        public static (int code, string output) PowerShell(string script, int timeoutMs = 30000)
        {
            string tmp = Path.Combine(Path.GetTempPath(), "cameorescue-" + Guid.NewGuid().ToString("N") + ".ps1");
            try
            {
                File.WriteAllText(tmp, script, new UTF8Encoding(false));
                return Run("powershell.exe",
                    "-NoProfile -NonInteractive -ExecutionPolicy Bypass -File \"" + tmp + "\"", timeoutMs);
            }
            finally
            {
                try { File.Delete(tmp); } catch { }
            }
        }

        /// <summary>Jalankan skrip PowerShell dengan elevasi (prompt UAC).</summary>
        public static (bool started, int code, string output) PowerShellElevated(string script, int timeoutMs = 60000)
        {
            string tmp = Path.Combine(Path.GetTempPath(), "cameorescue-admin-" + Guid.NewGuid().ToString("N") + ".ps1");
            try
            {
                File.WriteAllText(tmp, script, new UTF8Encoding(false));
                var psi = new ProcessStartInfo("powershell.exe",
                    "-NoProfile -ExecutionPolicy Bypass -File \"" + tmp + "\"")
                {
                    UseShellExecute = true,
                    Verb = "runas",
                };
                using var p = Process.Start(psi);
                if (!p.WaitForExit(timeoutMs)) { try { p.Kill(); } catch { } return (true, -1, "timeout"); }
                return (true, p.ExitCode, "");
            }
            catch (Exception ex)
            {
                return (false, -1, ex.Message);   // biasanya user menolak UAC
            }
            finally
            {
                try { File.Delete(tmp); } catch { }
            }
        }
    }
}
