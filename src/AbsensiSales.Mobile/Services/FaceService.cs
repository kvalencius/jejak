using AbsensiSales.Face;

namespace AbsensiSales.Mobile.Services;

/// <summary>
/// Pintu masuk tunggal ke engine verifikasi wajah untuk aplikasi Mobile.
///
/// Model ONNX dimuat sekali (lazy) karena ukurannya besar:
/// - Windows: folder <c>models</c> di samping executable.
/// - Android: aset paket aplikasi (Resources/Raw).
///
/// Tidak ada mode simulasi. Kalau model tidak tersedia, <see cref="PastikanSiapAsync"/>
/// mengembalikan false dan pemanggil wajib menampilkan pesan, bukan meloloskan absensi.
/// </summary>
public class FaceService : IDisposable
{
    private readonly SemaphoreSlim _kunci = new(1, 1);
    private FaceVerificationService? _service;

    /// <summary>True jika engine sudah dimuat dan siap dipakai.</summary>
    public bool Siap => _service?.EngineSiap == true;

    /// <summary>Alasan engine belum siap (null jika siap).</summary>
    public string? Pesan { get; private set; }

    /// <summary>
    /// Muat model bila belum. Aman dipanggil berkali-kali dan dari banyak thread.
    /// Hasil false berarti verifikasi wajah tidak boleh dijalankan.
    /// </summary>
    public async Task<bool> PastikanSiapAsync()
    {
        if (_service is not null)
            return _service.EngineSiap;

        await _kunci.WaitAsync();
        try
        {
            if (_service is not null)
                return _service.EngineSiap;

            // Membuat InferenceSession untuk model 38 MB butuh waktu;
            // jalankan di thread latar agar UI tidak membeku.
            var service = await Task.Run(BuatServiceAsync);
            _service = service;
            Pesan = service?.EngineSiap == true
                ? null
                : service?.PesanInisialisasi ?? "Model wajah tidak ditemukan di perangkat ini.";

            return service?.EngineSiap == true;
        }
        finally
        {
            _kunci.Release();
        }
    }

    private async Task<FaceVerificationService?> BuatServiceAsync()
    {
        var folder = CariFolderModel();
        if (folder is not null)
        {
            var dariFolder = new FaceVerificationService(folder);
            dariFolder.Inisialisasi();
            if (dariFolder.EngineSiap)
                return dariFolder;

            dariFolder.Dispose();
        }

        var detektor = await BacaAsetAsync("version-RFB-320.onnx");
        var sface = await BacaAsetAsync("face_recognition_sface_2021dec.onnx");
        if (detektor is not null && sface is not null)
        {
            var dariAset = new FaceVerificationService(detektor, sface);
            dariAset.Inisialisasi();
            return dariAset;
        }

        return null;
    }

    /// <summary>
    /// Cari folder yang memiliki kedua model. Dicek di samping executable
    /// dan di beberapa level di atasnya (saat dijalankan dari folder build).
    /// </summary>
    private static string? CariFolderModel()
    {
        var kandidat = new List<string> { AppContext.BaseDirectory };

        var naik = new DirectoryInfo(AppContext.BaseDirectory);
        for (int i = 0; i < 6 && naik?.Parent is not null; i++)
        {
            naik = naik.Parent;
            kandidat.Add(Path.Combine(naik.FullName, "models"));
        }

        foreach (var folder in kandidat)
        {
            var detektor = Path.Combine(folder, "models", "version-RFB-320.onnx");
            var sface = Path.Combine(folder, "models", "face_recognition_sface_2021dec.onnx");
            if (File.Exists(detektor) && File.Exists(sface))
                return folder;
        }

        return null;
    }

    private static async Task<byte[]?> BacaAsetAsync(string nama)
    {
        try
        {
            using var stream = await FileSystem.OpenAppPackageFileAsync(nama);
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            return buffer.ToArray();
        }
        catch
        {
            return null;
        }
    }

    /// <summary>Buat token wajah dari foto (JPEG/PNG). Foto tidak disimpan.</summary>
    public async Task<FaceTemplateResult> BuatTemplateAsync(byte[] foto)
    {
        if (!await PastikanSiapAsync())
            return new FaceTemplateResult(null, 0, Pesan ?? "Engine wajah belum siap.");

        return await Task.Run(() => _service!.BuatTemplate(foto));
    }

    /// <summary>Verifikasi foto terhadap token wajah tersimpan.</summary>
    public async Task<FaceVerifyResult> VerifikasiAsync(byte[] foto, string template)
    {
        if (!await PastikanSiapAsync())
            return new FaceVerifyResult(false, 0, Pesan ?? "Engine wajah belum siap.");

        return await Task.Run(() => _service!.Verifikasi(foto, template));
    }

    public void Dispose()
    {
        _service?.Dispose();
        _kunci.Dispose();
    }
}
