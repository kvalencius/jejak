# Model Face Recognition

Aplikasi membutuhkan **dua** file model ONNX di folder ini.
Keduanya tidak di-commit (lihat `.gitignore`).

1. `version-RFB-320.onnx` — deteksi wajah (UltraFace)
   - Unduh dari: https://github.com/Linzaer/Ultra-Light-Fast-Generic-Face-Detector-1MB
     → `models/onnx/version-RFB-320.onnx`
   - Ukuran ±1,2 MB

2. `face_recognition_sface_2021dec.onnx` — embedding wajah (SFace)
   - Unduh dari: https://github.com/opencv/opencv_zoo → `models/face_recognition_sface`
   - Ukuran ±38 MB

## Cara memasang

Letakkan kedua file langsung di folder `models/` (bukan di subfolder).
File `.csproj` aplikasi Mobile menyalinnya otomatis:

- **Windows** → ke `models/` di samping executable.
- **Android/iOS** → sebagai `MauiAsset` di dalam paket aplikasi.

## Perilaku tanpa model

Tidak ada mode simulasi. Jika model tidak tersedia:

- Halaman verifikasi wajah menampilkan **"Model wajah belum tersedia"** dan
  tombol pindai tidak meloloskan apa pun.
- Verifikasi selalu gagal dengan pesan jelas, sehingga tidak ada absensi yang
  tercatat tanpa benar-benar mencocokkan wajah.

## Token wajah

Foto wajah **tidak** disimpan. Aplikasi mengubah wajah menjadi embedding
(128 nilai float32), lalu mengirimnya sebagai token Base64 ke `wajah.php`
untuk disimpan di `tbl_sales.face_data`.

Verifikasi membandingkan embedding wajah baru dengan token tersimpan memakai
cosine similarity, dengan ambang batas di `src/AbsensiSales.Core/Services/FaceScore.cs`.

## Kalibrasi

Nilai ambang `0.363` berasal dari cosine similarity model SFace dan sudah diuji
pada foto nyata:

| Perbandingan | Skor |
|---|---|
| Orang sama (foto berbeda) | 0,65 – 0,95 |
| Orang berbeda | 0,27 – 0,32 |

Saat mengubah model atau preprocessing, kalibrasi ulang ambang ini dengan data
nyata sebelum dipakai di produksi.
