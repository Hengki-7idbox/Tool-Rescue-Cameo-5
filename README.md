# Tool Rescue CAMEO 5

Aplikasi desktop Windows untuk **memulihkan Silhouette CAMEO 5 yang "brick"**
(gagal nyala setelah update firmware terputus) dengan cara mengirim ulang firmware
lewat **USB**.

Dibuat dari kasus nyata: CAMEO 5 brick akibat listrik mati saat update firmware via
Bluetooth, lalu dipulihkan lewat **skrip USB mentah** (`Flash.exe`) — bukan lewat
aplikasi GUI ini.

---

## ⚠️ Status Pengujian / Testing Status

### 🇮🇩 Bahasa Indonesia

> **Penting:** jalur **pengiriman firmware (flashing)** di aplikasi ini **belum diuji**
> ke device CAMEO 5 yang benar-benar brick. Yang sudah terbukti berhasil adalah **cara
> mentah** (skrip `Flash.exe` / `Flash.cs`), sedangkan aplikasi GUI ini dibuat dari
> logika yang sama namun **belum pernah dijalankan ke device brick**.

**Yang berhasil kemarin (17 Oktober 2026)** — pemulihan dilakukan **langsung via skrip
mentah**, bukan lewat aplikasi ini:
kirim firmware **satu arah** ke interface USBPRINT → `1B 12` (masuk mode update) →
isi file `.S` (850 byte/blok) → `1B 05` (finalize) → **cabut-colok power**.

| Bagian | Status |
|---|---|
| Deteksi perangkat & mode | ✅ Teruji (device nyata) |
| Validasi firmware (header + SHA256) | ✅ Teruji |
| Muat firmware bawaan | ✅ Teruji |
| Ganti bahasa ID/EN, log | ✅ Teruji |
| **Kirim firmware (flashing lewat GUI)** | ⚠️ **Belum diuji di device brick** |
| Tab Perbaikan USB | ⚠️ Belum diuji menyeluruh |

> **Kesimpulan:** aplikasi ini **belum terverifikasi end-to-end**. Gunakan dengan sadar;
> logika intinya identik dengan cara yang sudah berhasil, sehingga risikonya rendah.

### 🇬🇧 English

> **Important:** the **firmware flashing** path in this app has **not been tested** on a
> truly bricked CAMEO 5. What is proven is the **raw method** (the `Flash.exe` /
> `Flash.cs` script). This GUI app is built from the same logic but **has never been run
> against a bricked device**.

**What worked (17 Oct 2026)** — the recovery was done **directly via the raw script**,
not through this app:
send firmware **one-way** to the USBPRINT interface → `1B 12` (enter update mode) →
the `.S` file bytes (850 bytes/chunk) → `1B 05` (finalize) → **power-cycle**.

| Part | Status |
|---|---|
| Device & mode detection | ✅ Tested (real device) |
| Firmware validation (header + SHA256) | ✅ Tested |
| Built-in firmware loading | ✅ Tested |
| Language switch ID/EN, logging | ✅ Tested |
| **Flashing via the GUI** | ⚠️ **Not tested on a bricked device** |
| USB Repair tab | ⚠️ Not fully tested |

> **Conclusion:** this app is **not yet verified end-to-end**. Use it knowingly; the core
> logic is identical to the method that already worked, so the risk is low.

---

## Unduh & Pakai (tanpa install)

1. Unduh **`Hasil/Tool App/publish/CameoRescue.exe`** (satu file, mandiri).
2. Double-click. Tidak perlu install .NET — runtime sudah terbundel.
3. Ikuti tab **Flashing** atau **Perbaikan USB**.

> Exe ini **self-contained**: jalan di Windows 10/11 64-bit tanpa syarat tambahan.

## Fitur

- **Tab Flashing** – deteksi perangkat, validasi firmware (header + SHA256), wizard
  pengiriman firmware (progress + log), verifikasi hasil. Ada **mode dry-run** (uji
  tanpa menulis).
- **Tab Perbaikan USB** – diagnosa, scan ulang USB, restart perangkat, reset hub USB,
  buka Device Manager (aksi tertentu butuh Administrator).
- **Tab Panduan** – panduan singkat di dalam aplikasi.
- **Dua bahasa** – Indonesia & English (bisa diganti langsung).
- Firmware CAMEO 5 **sudah tertanam** di dalam aplikasi.

## Cara Pemulihan Singkat

1. Hubungkan cutter ke PC via kabel USB (sebaiknya langsung ke PC, bukan hub).
2. Masukkan mode update: **TAHAN** tombol power → **COLOK** kabel power → **LEPAS**
   tombol saat logo tengah berkedip.
3. Buka tab **Flashing**, klik **Segarkan** (mode harus `Bootloader`).
4. Klik **Mulai Perbaikan** → setujui konfirmasi.
5. Setelah selesai: cabut kabel **POWER**, tunggu 5 detik, colok lagi, nyalakan.
6. Klik **Verifikasi Hasil** (mode harus berubah menjadi `Normal`).

Detail langkah lengkap juga tersedia di tab **Panduan** di dalam aplikasi.

## Struktur Repo

```
Hasil/
  Tool App/
    publish/CameoRescue.exe   <- aplikasi siap pakai (satu file)
    *.cs, Core/, Tabs/        <- source code
    Resources/                <- firmware .S (tertanam ke exe)
    build.ps1                 <- skrip rebuild
firmware/                     <- salinan firmware CAMEO 5
```

## Build dari Source

Butuh .NET SDK 10.

```powershell
cd "Hasil\Tool App"
powershell -ExecutionPolicy Bypass -File build.ps1
```

Hasil: `Hasil\Tool App\publish\CameoRescue.exe` (satu file, self-contained).

## Cara Kerja (ringkas)

Jalur USB ke perangkat ini bersifat **tulis-saja** (`ReadFile` tidak pernah membalas).
Karena itu firmware dikirim **satu arah**:

```
1B 12                       -> masuk mode update
<isi file .S> 850 byte/blok -> kirim firmware
1B 05                       -> finalize
```

Lalu **cabut-colok power** agar perangkat boot dengan firmware baru.

## Peringatan

- **Jangan cabut kabel** selama proses pengiriman berjalan.
- Gunakan firmware untuk **CAMEO 5** saja.
- Firmware `.S` adalah milik **Silhouette America**. Repo ini untuk keperluan
  **perbaikan perangkat sendiri** (interoperabilitas), bukan untuk redistribusi komersial.

## Lisensi

Kode aplikasi: bebas dipakai untuk keperluan perbaikan pribadi.
Firmware tetap milik pemegang haknya masing-masing.
