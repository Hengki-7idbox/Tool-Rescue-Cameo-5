using System;
using System.Collections.Generic;

namespace CameoRescue
{
    /// <summary>Kamus teks Indonesia / English. Semua teks UI diambil dari sini.</summary>
    public static class Lang
    {
        public static string Current { get; private set; } = "id";
        public static event Action Changed;

        public static void Set(string lang)
        {
            if (lang != "id" && lang != "en") lang = "id";
            if (Current == lang) return;
            Current = lang;
            Changed?.Invoke();
        }

        public static string T(string key)
        {
            var d = Current == "en" ? En : Id;
            return d.TryGetValue(key, out var v) ? v : key;
        }

        public static string T(string key, params object[] args)
        {
            try { return string.Format(T(key), args); }
            catch { return T(key); }
        }

        static readonly Dictionary<string, string> Id = new()
        {
            ["app.title"] = "CAMEO 5 Rescue Tool",
            ["lang.label"] = "Bahasa:",

            ["tab.flash"] = "Flashing",
            ["tab.usb"] = "Perbaikan USB",
            ["tab.guide"] = "Panduan",

            ["btn.refresh"] = "Segarkan",
            ["btn.pick"] = "Pilih file .S ...",
            ["btn.useEmbedded"] = "Pakai firmware bawaan",
            ["btn.start"] = "Mulai Perbaikan",
            ["btn.verify"] = "Verifikasi Hasil",
            ["btn.saveLog"] = "Simpan Log",
            ["btn.copyLog"] = "Salin Log",
            ["btn.clearLog"] = "Bersihkan Log",

            ["flash.grp.device"] = "Perangkat",
            ["flash.grp.firmware"] = "Firmware",
            ["flash.grp.process"] = "Proses",
            ["lbl.model"] = "Model",
            ["lbl.serial"] = "Serial",
            ["lbl.mode"] = "Mode",
            ["lbl.none"] = "-",
            ["mode.bootloader"] = "Bootloader (siap update)",
            ["mode.normal"] = "Normal (siap dipakai)",
            ["mode.unknown"] = "Tidak diketahui",

            ["fw.source"] = "Sumber",
            ["fw.name"] = "Nama",
            ["fw.size"] = "Ukuran",
            ["fw.sha"] = "SHA256",
            ["fw.header"] = "Header",
            ["fw.compat"] = "Kompatibilitas",
            ["compat.checking"] = "memeriksa...",
            ["compat.ok"] = "COCOK (CAMEO 5)",
            ["compat.bad"] = "TIDAK COCOK",

            ["chk.dry"] = "Mode uji (dry-run) - tidak menulis ke perangkat",

            ["status.ready"] = "Siap.",
            ["status.nodevice"] = "Perangkat tidak terdeteksi.",
            ["status.device"] = "Terdeteksi: {0} - serial {1} - {2}",
            ["status.selected"] = "Firmware dipilih: {0}",
            ["status.flashing"] = "Mengirim firmware...",
            ["status.done"] = "Selesai. Lakukan power-cycle lalu klik Verifikasi Hasil.",
            ["status.failed"] = "Gagal: {0}",
            ["status.verified.ok"] = "Berhasil! Perangkat sekarang Normal (serial {0}).",
            ["status.verified.still"] = "Perangkat masih di mode Bootloader. Coba lagi / power-cycle.",

            ["msg.info"] = "Informasi",
            ["msg.warn"] = "Peringatan",
            ["msg.error"] = "Kesalahan",
            ["msg.question"] = "Konfirmasi",

            ["msg.nodevice.text"] =
                "Perangkat CAMEO 5 tidak terdeteksi.\n\n" +
                "1. Hubungkan kabel USB (langsung ke PC, bukan hub bila bisa).\n" +
                "2. Masukkan mode update: TAHAN tombol power, COLOK kabel power, lepas tombol saat logo tengah berkedip.\n" +
                "3. Klik Segarkan lalu coba lagi.",
            ["msg.fwbad.text"] =
                "Firmware yang dipilih bukan untuk CAMEO 5 (header tidak mengandung 'CAMEO5').\n" +
                "Pilih file .S yang benar atau gunakan firmware bawaan.",
            ["msg.confirm.text"] =
                "Kirim firmware ke perangkat?\n\nModel : {0}\nSerial: {1}\nMode  : {2}\nFile  : {3}\n\n" +
                "JANGAN cabut kabel USB/power selama proses. Lanjutkan?",
            ["msg.done.text"] =
                "Firmware terkirim{0}.\n\nSekarang:\n" +
                "1. Cabut kabel POWER cutter, tunggu 5 detik.\n" +
                "2. Colok kembali (tanpa menahan tombol) dan nyalakan.\n" +
                "3. Klik 'Verifikasi Hasil' untuk memastikan serial berubah.",
            ["msg.sim.done"] = " (SIMULASI - tidak ada data yang ditulis)",
            ["msg.logsaved"] = "Log tersimpan: {0}",

            // ---- USB tab ----
            ["usb.grp.devices"] = "Perangkat Silhouette",
            ["usb.grp.actions"] = "Aksi Perbaikan",
            ["usb.grp.report"] = "Laporan Diagnostik",
            ["usb.btn.scan"] = "Scan ulang USB",
            ["usb.btn.restart"] = "Restart perangkat",
            ["usb.btn.resethub"] = "Reset Hub USB",
            ["usb.btn.devmgr"] = "Buka Device Manager",
            ["usb.btn.admin"] = "Jalankan sebagai Admin",
            ["usb.btn.diag"] = "Diagnosa sekarang",
            ["usb.lbl.admin.yes"] = "Status: Administrator",
            ["usb.lbl.admin.no"] = "Status: bukan Administrator (beberapa aksi butuh admin)",
            ["usb.scan.done"] = "Scan ulang selesai.",
            ["usb.restart.none"] = "Tidak ada perangkat VID_0B4D yang aktif untuk di-restart.",
            ["usb.restart.done"] = "Perintah restart dikirim untuk {0}.",
            ["usb.resethub.none"] = "Tidak ada hub USB yang ditemukan.",
            ["usb.resethub.done"] = "Hub di-reset: {0}",
            ["usb.admin.launched"] = "Aplikasi admin diluncurkan.",
            ["usb.admin.declined"] = "Elevasi dibatalkan.",
            ["usb.hint"] =
                "Tips: jika muncul 'USB device not recognized' (Code 43), lakukan urutan ini:\n" +
                "  matikan cutter -> cabut kabel USB DAN power -> tunggu 30 detik -> colok power -> nyalakan -> colok USB.\n" +
                "Colok USB hanya SETELAH cutter selesai menyala.",

            // ---- Guide tab ----
            ["guide.title"] = "Panduan Singkat",
            ["guide.text"] =
                "TUJUAN\n" +
                "Memulihkan CAMEO 5 yang 'brick' (gagal nyala) akibat update firmware terputus, dengan cara mengirim ulang firmware lewat USB.\n\n" +
                "SEBELUM MULAI\n" +
                "- Sambungkan cutter ke PC dengan kabel USB (sebaiknya langsung ke PC).\n" +
                "- Firmware bawaan sudah tersedia di dalam aplikasi.\n\n" +
                "CARA PAKAI (TAB FLASHING)\n" +
                "1. Pastikan perangkat terdeteksi (panel Perangkat).\n" +
                "2. Masuk mode update: TAHAN tombol power, COLOK kabel power, LEPAS tombol saat logo tengah berkedip.\n" +
                "3. Klik Segarkan. Mode harus 'Bootloader'.\n" +
                "4. Periksa firmware (harus berlabel COCOK).\n" +
                "5. Klik 'Mulai Perbaikan' lalu setujui konfirmasi.\n" +
                "6. Setelah selesai: cabut kabel POWER, tunggu 5 detik, colok lagi, nyalakan.\n" +
                "7. Klik 'Verifikasi Hasil'. Mode harus berubah menjadi 'Normal'.\n\n" +
                "JIKA GAGAL\n" +
                "- Buka tab 'Perbaikan USB' dan jalankan Diagnosa / Scan ulang / Reset Hub.\n" +
                "- Pastikan kabel USB bagus dan tidak lewat hub yang lemah.\n" +
                "- Ulangi urutan power-cycle (USB dilepas lebih dulu, dicolok terakhir).\n\n" +
                "PENTING\n" +
                "- Jangan cabut kabel saat proses pengiriman berjalan.\n" +
                "- Gunakan firmware untuk CAMEO 5 saja.\n",
        };

        static readonly Dictionary<string, string> En = new()
        {
            ["app.title"] = "CAMEO 5 Rescue Tool",
            ["lang.label"] = "Language:",

            ["tab.flash"] = "Flashing",
            ["tab.usb"] = "USB Repair",
            ["tab.guide"] = "Guide",

            ["btn.refresh"] = "Refresh",
            ["btn.pick"] = "Choose .S file ...",
            ["btn.useEmbedded"] = "Use built-in firmware",
            ["btn.start"] = "Start Repair",
            ["btn.verify"] = "Verify Result",
            ["btn.saveLog"] = "Save Log",
            ["btn.copyLog"] = "Copy Log",
            ["btn.clearLog"] = "Clear Log",

            ["flash.grp.device"] = "Device",
            ["flash.grp.firmware"] = "Firmware",
            ["flash.grp.process"] = "Process",
            ["lbl.model"] = "Model",
            ["lbl.serial"] = "Serial",
            ["lbl.mode"] = "Mode",
            ["lbl.none"] = "-",
            ["mode.bootloader"] = "Bootloader (ready to update)",
            ["mode.normal"] = "Normal (ready to use)",
            ["mode.unknown"] = "Unknown",

            ["fw.source"] = "Source",
            ["fw.name"] = "Name",
            ["fw.size"] = "Size",
            ["fw.sha"] = "SHA256",
            ["fw.header"] = "Header",
            ["fw.compat"] = "Compatibility",
            ["compat.checking"] = "checking...",
            ["compat.ok"] = "MATCH (CAMEO 5)",
            ["compat.bad"] = "NOT COMPATIBLE",

            ["chk.dry"] = "Test mode (dry-run) - does not write to the device",

            ["status.ready"] = "Ready.",
            ["status.nodevice"] = "Device not detected.",
            ["status.device"] = "Detected: {0} - serial {1} - {2}",
            ["status.selected"] = "Firmware selected: {0}",
            ["status.flashing"] = "Sending firmware...",
            ["status.done"] = "Done. Power-cycle the cutter, then click Verify Result.",
            ["status.failed"] = "Failed: {0}",
            ["status.verified.ok"] = "Success! Device is now Normal (serial {0}).",
            ["status.verified.still"] = "Device is still in Bootloader mode. Retry / power-cycle.",

            ["msg.info"] = "Information",
            ["msg.warn"] = "Warning",
            ["msg.error"] = "Error",
            ["msg.question"] = "Confirm",

            ["msg.nodevice.text"] =
                "CAMEO 5 device not detected.\n\n" +
                "1. Connect the USB cable (directly to the PC, not via a hub if possible).\n" +
                "2. Enter update mode: HOLD power, PLUG the power cable, release the button when the center logo blinks.\n" +
                "3. Click Refresh and try again.",
            ["msg.fwbad.text"] =
                "The selected firmware is not for CAMEO 5 (header does not contain 'CAMEO5').\n" +
                "Choose the correct .S file or use the built-in firmware.",
            ["msg.confirm.text"] =
                "Send firmware to the device?\n\nModel : {0}\nSerial: {1}\nMode  : {2}\nFile  : {3}\n\n" +
                "DO NOT unplug USB/power during the process. Continue?",
            ["msg.done.text"] =
                "Firmware sent{0}.\n\nNow:\n" +
                "1. Unplug the cutter POWER cable, wait 5 seconds.\n" +
                "2. Plug it back (without holding the button) and power on.\n" +
                "3. Click 'Verify Result' to confirm the serial changed.",
            ["msg.sim.done"] = " (SIMULATION - nothing was written)",
            ["msg.logsaved"] = "Log saved: {0}",

            ["usb.grp.devices"] = "Silhouette devices",
            ["usb.grp.actions"] = "Repair actions",
            ["usb.grp.report"] = "Diagnostic report",
            ["usb.btn.scan"] = "Rescan USB",
            ["usb.btn.restart"] = "Restart device",
            ["usb.btn.resethub"] = "Reset USB Hub",
            ["usb.btn.devmgr"] = "Open Device Manager",
            ["usb.btn.admin"] = "Run as Administrator",
            ["usb.btn.diag"] = "Diagnose now",
            ["usb.lbl.admin.yes"] = "Status: Administrator",
            ["usb.lbl.admin.no"] = "Status: not Administrator (some actions need admin)",
            ["usb.scan.done"] = "Rescan complete.",
            ["usb.restart.none"] = "No active VID_0B4D device to restart.",
            ["usb.restart.done"] = "Restart command sent for {0}.",
            ["usb.resethub.none"] = "No USB hub found.",
            ["usb.resethub.done"] = "Hub reset: {0}",
            ["usb.admin.launched"] = "Admin app launched.",
            ["usb.admin.declined"] = "Elevation cancelled.",
            ["usb.hint"] =
                "Tip: if you see 'USB device not recognized' (Code 43), do this sequence:\n" +
                "  turn off cutter -> unplug BOTH USB and power -> wait 30s -> plug power -> power on -> plug USB.\n" +
                "Plug USB only AFTER the cutter has fully booted.",

            ["guide.title"] = "Quick Guide",
            ["guide.text"] =
                "GOAL\n" +
                "Recover a 'bricked' CAMEO 5 (won't boot) caused by an interrupted firmware update, by re-sending firmware over USB.\n\n" +
                "BEFORE YOU START\n" +
                "- Connect the cutter to the PC with a USB cable (preferably directly to the PC).\n" +
                "- The built-in firmware is already included in this app.\n\n" +
                "HOW TO USE (FLASHING TAB)\n" +
                "1. Make sure the device is detected (Device panel).\n" +
                "2. Enter update mode: HOLD power, PLUG power cable, RELEASE the button when the center logo blinks.\n" +
                "3. Click Refresh. Mode must be 'Bootloader'.\n" +
                "4. Check firmware (must show MATCH).\n" +
                "5. Click 'Start Repair' and accept the confirmation.\n" +
                "6. When done: unplug POWER cable, wait 5 s, plug back in, power on.\n" +
                "7. Click 'Verify Result'. Mode must become 'Normal'.\n\n" +
                "IF IT FAILS\n" +
                "- Open the 'USB Repair' tab and run Diagnose / Rescan / Reset Hub.\n" +
                "- Use a good USB cable and avoid weak hubs.\n" +
                "- Repeat the power-cycle sequence (unplug USB first, plug it last).\n\n" +
                "IMPORTANT\n" +
                "- Never unplug cables during the transfer.\n" +
                "- Use CAMEO 5 firmware only.\n",
        };
    }
}
