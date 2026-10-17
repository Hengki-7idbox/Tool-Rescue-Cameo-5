using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace CameoRescue.Core
{
    /// <summary>Hasil enumerasi satu perangkat Silhouette lewat interface USBPRINT.</summary>
    public sealed class CameoDevice
    {
        public string Path;          // \\?\usb#vid_0b4d&pid_1140#<serial>#{guid}
        public string Vid;           // 0B4D
        public string Pid;           // 1140
        public string Serial;        // contoh: serial bootloader / serial normal
        public string Model;         // Silhouette CAMEO 5

        public bool LooksLikeBootloader =>
            !string.IsNullOrEmpty(Serial) && Serial.StartsWith("00000", StringComparison.OrdinalIgnoreCase);

        public string ModeKey => string.IsNullOrEmpty(Serial)
            ? "mode.unknown"
            : (LooksLikeBootloader ? "mode.bootloader" : "mode.normal");
    }

    /// <summary>
    /// Komunikasi USB ke CAMEO 5 (interface USBPRINT, tanpa WinUSB).
    /// Berbasis kode yang sudah terbukti: SetupDi* -> CreateFile -> WriteFile.
    /// </summary>
    public static class UsbDevice
    {
        const uint GENERIC_READ = 0x80000000, GENERIC_WRITE = 0x40000000;
        const uint FILE_SHARE_READ = 1, FILE_SHARE_WRITE = 2;
        const uint OPEN_EXISTING = 3, FILE_ATTRIBUTE_NORMAL = 0x80;
        const uint DIGCF_PRESENT = 0x2, DIGCF_DEVICEINTERFACE = 0x10;
        static readonly IntPtr INVALID_HANDLE_VALUE = new IntPtr(-1);

        // USBPRINT
        static readonly Guid USBPRINT = new Guid("28d78fad-5a12-11d1-ae5b-0000f803a8c2");

        [StructLayout(LayoutKind.Sequential)]
        struct SP_DEVICE_INTERFACE_DATA
        {
            public int cbSize;
            public Guid InterfaceClassGuid;
            public int Flags;
            public IntPtr Reserved;
        }

        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr SetupDiGetClassDevsW(IntPtr ClassGuid, string Enumerator, IntPtr hwndParent, uint Flags);
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiEnumDeviceInterfaces(IntPtr set, IntPtr devinfo, IntPtr guid, int idx, ref SP_DEVICE_INTERFACE_DATA ifd);
        [DllImport("setupapi.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern bool SetupDiGetDeviceInterfaceDetailW(IntPtr set, ref SP_DEVICE_INTERFACE_DATA ifd, IntPtr detail, int detailSize, out int req, IntPtr did);
        [DllImport("setupapi.dll", SetLastError = true)]
        static extern bool SetupDiDestroyDeviceInfoList(IntPtr set);
        [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        static extern IntPtr CreateFileW(string name, uint access, uint share, IntPtr sec, uint disp, uint flags, IntPtr templ);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool CloseHandle(IntPtr h);
        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteFile(IntPtr h, byte[] buf, uint n, out uint written, IntPtr ov);

        static IntPtr AllocGuid(Guid g) { IntPtr p = Marshal.AllocHGlobal(16); Marshal.Copy(g.ToByteArray(), 0, p, 16); return p; }

        static string ExtractPath(IntPtr buf, int req)
        {
            byte[] all = new byte[req];
            Marshal.Copy(buf, all, 0, req);
            byte[] needle = { 0x75, 0x00, 0x73, 0x00, 0x62, 0x00, 0x23, 0x00 }; // "usb#"
            for (int i = 0; i + needle.Length <= req; i += 2)
            {
                bool m = true;
                for (int j = 0; j < needle.Length; j++) if (all[i + j] != needle[j]) { m = false; break; }
                if (m) return "\\\\?\\" + Encoding.Unicode.GetString(all, i, req - i).TrimEnd('\0');
            }
            return null;
        }

        static string ParseSerial(string path)
        {
            try
            {
                int a = path.IndexOf('#');
                if (a < 0) return null;
                int b = path.IndexOf('#', a + 1);
                if (b < 0) return null;
                int c = path.IndexOf('#', b + 1);
                if (c < 0) return null;
                string vidpid = path.Substring(a + 1, b - a - 1);
                string serial = path.Substring(b + 1, c - b - 1);
                if (!vidpid.StartsWith("vid_0b4d", StringComparison.OrdinalIgnoreCase)) return null;
                return serial;
            }
            catch { return null; }
        }

        /// <summary>Enumerasi semua perangkat dengan VID_0B4D melalui interface USBPRINT.</summary>
        public static List<CameoDevice> Enumerate()
        {
            var list = new List<CameoDevice>();
            IntPtr pg = AllocGuid(USBPRINT);
            IntPtr set = SetupDiGetClassDevsW(pg, null, IntPtr.Zero, DIGCF_PRESENT | DIGCF_DEVICEINTERFACE);
            if (set == INVALID_HANDLE_VALUE) { Marshal.FreeHGlobal(pg); return list; }
            try
            {
                int idx = 0;
                while (true)
                {
                    var ifd = new SP_DEVICE_INTERFACE_DATA();
                    ifd.cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DATA));
                    if (!SetupDiEnumDeviceInterfaces(set, IntPtr.Zero, pg, idx, ref ifd)) break;
                    idx++;
                    int req = 0;
                    SetupDiGetDeviceInterfaceDetailW(set, ref ifd, IntPtr.Zero, 0, out req, IntPtr.Zero);
                    if (req <= 0) continue;
                    IntPtr buf = Marshal.AllocHGlobal(req);
                    Marshal.WriteInt32(buf, IntPtr.Size == 8 ? 8 : 6);
                    if (SetupDiGetDeviceInterfaceDetailW(set, ref ifd, buf, req, out req, IntPtr.Zero))
                    {
                        string p = ExtractPath(buf, req);
                        string serial = p == null ? null : ParseSerial(p);
                        if (serial != null)
                        {
                            list.Add(new CameoDevice
                            {
                                Path = p,
                                Vid = "0B4D",
                                Pid = "1140",
                                Serial = serial.ToUpperInvariant(),
                                Model = "Silhouette CAMEO 5"
                            });
                        }
                    }
                    Marshal.FreeHGlobal(buf);
                }
            }
            finally
            {
                Marshal.FreeHGlobal(pg);
                SetupDiDestroyDeviceInfoList(set);
            }
            return list;
        }

        /// <summary>Ambil perangkat pertama yang cocok (biasanya hanya satu).</summary>
        public static CameoDevice Find() => Enumerate().Count > 0 ? Enumerate()[0] : null;

        public static IntPtr Open(string path, out int error)
        {
            IntPtr h = CreateFileW(path, GENERIC_READ | GENERIC_WRITE, FILE_SHARE_READ | FILE_SHARE_WRITE,
                IntPtr.Zero, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, IntPtr.Zero);
            error = h == INVALID_HANDLE_VALUE ? Marshal.GetLastWin32Error() : 0;
            return h;
        }

        public static void Close(IntPtr h) { if (h != INVALID_HANDLE_VALUE) CloseHandle(h); }

        public static bool Write(IntPtr h, byte[] buf, int off, int len, out int error)
        {
            var tmp = new byte[len];
            Array.Copy(buf, off, tmp, 0, len);
            uint wr;
            bool ok = WriteFile(h, tmp, (uint)len, out wr, IntPtr.Zero) && wr == len;
            error = ok ? 0 : Marshal.GetLastWin32Error();
            return ok;
        }
    }
}
