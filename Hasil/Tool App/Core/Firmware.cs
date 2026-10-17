using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace CameoRescue.Core
{
    public sealed class FirmwareInfo
    {
        public string Name;
        public string Source;          // "bawaan" / path
        public byte[] Bytes;
        public string Sha256;
        public string Header;          // isi record S0
        public bool IsSRecord;
        public bool CompatibleCameo5;

        public int Size => Bytes?.Length ?? 0;

        public string SizeText => Size >= 1024 ? (Size / 1024) + " KB (" + Size + " byte)" : Size + " byte";
    }

    public static class Firmware
    {
        public const string EmbeddedMain = "CameoRescue.fw.cameo5_v105";
        public const string EmbeddedAlpha = "CameoRescue.fw.cameo5alpha_v101";

        public static List<(string key, string label)> EmbeddedList() => new()
        {
            (EmbeddedMain,  "CAMEO 5 v1.05 (V105_R0007)"),
            (EmbeddedAlpha, "CAMEO 5 Alpha v1.01 (V101_R0002)"),
        };

        public static byte[] ReadEmbedded(string key)
        {
            var asm = Assembly.GetExecutingAssembly();
            using var s = asm.GetManifestResourceStream(key);
            if (s == null) return null;
            using var ms = new MemoryStream();
            s.CopyTo(ms);
            return ms.ToArray();
        }

        public static FirmwareInfo FromEmbedded(string key)
        {
            byte[] b = ReadEmbedded(key);
            if (b == null) return null;
            string label = key == EmbeddedAlpha ? "CAMEO 5 Alpha v1.01 (V101_R0002)" : "CAMEO 5 v1.05 (V105_R0007)";
            var info = Analyze(b);
            info.Name = label;
            info.Source = "bawaan";
            return info;
        }

        public static FirmwareInfo FromFile(string path)
        {
            byte[] b = File.ReadAllBytes(path);
            var info = Analyze(b);
            info.Name = Path.GetFileName(path);
            info.Source = path;
            return info;
        }

        public static FirmwareInfo Analyze(byte[] bytes)
        {
            var info = new FirmwareInfo { Bytes = bytes };
            if (bytes == null || bytes.Length == 0) return info;

            using (var sha = SHA256.Create())
                info.Sha256 = BitConverter.ToString(sha.ComputeHash(bytes)).Replace("-", "");

            // Cek record S0 (baris pertama, ASCII "S0")
            info.IsSRecord = bytes.Length >= 2 && bytes[0] == (byte)'S' && bytes[1] == (byte)'0';
            if (info.IsSRecord)
            {
                int nl = Array.IndexOf(bytes, (byte)'\n');
                int end = nl < 0 ? Math.Min(bytes.Length, 128) : Math.Min(nl, 128);
                info.Header = Encoding.ASCII.GetString(bytes, 0, end).TrimEnd('\r', '\n');
            }
            else
            {
                int end = Math.Min(bytes.Length, 64);
                info.Header = Encoding.ASCII.GetString(bytes, 0, end);
            }

            string h = (info.Header ?? "").ToUpperInvariant();
            info.CompatibleCameo5 = info.IsSRecord && (h.Contains("CAMEO5") || h.Contains("CAMEO 5"));
            return info;
        }
    }
}
