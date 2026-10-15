# Model Face Recognition

Taruh dua file berikut di folder ini (jangan di-commit):

1. `face_recognition_sface_2021dec.onnx`
   - Unduh dari: https://github.com/opencv/opencv_zoo → folder `models/face_recognition_sface`

2. `haarcascade_frontalface_default.xml`
   - Unduh dari: https://github.com/opencv/opencv → `data/haarcascades/haarcascade_frontalface_default.xml` (klik Raw)

Foto referensi wajah sales (nama file = nama tampil) ditaruh di folder
`reference_faces/` di samping executable aplikasi Mobile
(`src/AbsensiSales.Mobile/bin/Debug/net10.0-windows10.0.19041.0/win-x64/`).

Jika file model belum tersedia, aplikasi otomatis memakai **mode simulasi**
(verifikasi wajah dianggap cocok dengan skor ±94%).
