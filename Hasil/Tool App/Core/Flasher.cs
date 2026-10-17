using System;
using System.Threading;

namespace CameoRescue.Core
{
    public sealed class FlashResult
    {
        public bool Ok;
        public int BytesSent;
        public int TotalBytes;
        public string Error;
    }

    public static class Flasher
    {
        public const int DefaultChunk = 850;
        public const int DefaultDelayMs = 2;

        /// <summary>
        /// Kirim firmware satu arah (tanpa menunggu ack) ke perangkat melalui interface USBPRINT.
        /// Urutan: 1B 12 (masuk mode update) -> data .S per blok -> 1B 05 (finalize).
        /// </summary>
        public static FlashResult Flash(
            string devicePath,
            byte[] firmware,
            int chunk,
            int delayMs,
            bool dryRun,
            Action<int, string> report,   // (percent, statusText)
            CancellationToken ct)
        {
            var res = new FlashResult { TotalBytes = firmware.Length };
            if (chunk <= 0) chunk = DefaultChunk;

            if (dryRun)
            {
                for (int p = 0; p <= 100; p += 5)
                {
                    ct.ThrowIfCancellationRequested();
                    report?.Invoke(p, "Simulasi " + p + "%");
                    Thread.Sleep(40);
                }
                res.Ok = true;
                res.BytesSent = firmware.Length;
                return res;
            }

            IntPtr h = UsbDevice.Open(devicePath, out int err);
            if (h == IntPtr.Zero || err != 0)
            {
                res.Error = "Gagal membuka perangkat (err=" + err + ").";
                return res;
            }

            try
            {
                report?.Invoke(0, "Masuk mode update (1B 12)...");
                if (!UsbDevice.Write(h, new byte[] { 0x1B, 0x12 }, 0, 2, out err))
                {
                    res.Error = "Gagal kirim perintah mode update (err=" + err + ").";
                    return res;
                }
                Thread.Sleep(150);

                int sent = 0;
                for (int off = 0; off < firmware.Length; off += chunk)
                {
                    ct.ThrowIfCancellationRequested();
                    int len = Math.Min(chunk, firmware.Length - off);
                    if (!UsbDevice.Write(h, firmware, off, len, out err))
                    {
                        res.Error = "Gagal menulis di offset " + off + " (err=" + err + ").";
                        return res;
                    }
                    sent += len;
                    if (delayMs > 0) Thread.Sleep(delayMs);
                    if ((sent % (chunk * 100)) < chunk)
                    {
                        int pct = (int)((long)sent * 100 / firmware.Length);
                        report?.Invoke(pct, "Mengirim " + sent + " / " + firmware.Length + " byte");
                    }
                }
                res.BytesSent = sent;
                report?.Invoke(99, "Finalize (1B 05)...");
                Thread.Sleep(200);
                UsbDevice.Write(h, new byte[] { 0x1B, 0x05 }, 0, 2, out _);
                Thread.Sleep(200);
                report?.Invoke(100, "Selesai");
                res.Ok = true;
                return res;
            }
            finally
            {
                UsbDevice.Close(h);
            }
        }
    }
}
