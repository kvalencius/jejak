using AbsensiSales.Face;

namespace AbsensiSales.Mobile.Services;

/// <summary>
/// Singleton layanan verifikasi wajah. Model ONNX ditaruh di folder
/// "models" di samping executable (lihat README). Jika belum tersedia,
/// otomatis memakai mode simulasi.
/// </summary>
public class FaceService : IDisposable
{
    private readonly FaceVerificationService _service;

    public FaceService()
    {
        var baseFolder = AppContext.BaseDirectory;
        _service = new FaceVerificationService(baseFolder);
    }

    public bool ModeSimulasi => _service.ModeSimulasi;

    /// <summary>Muat model & data referensi sekali di awal aplikasi.</summary>
    public void Inisialisasi() => _service.Inisialisasi();

    public FaceVerifyResult Verifikasi(string namaSales) =>
        _service.VerifikasiDariKamera(namaSales);

    /// <summary>Verifikasi dari file foto hasil kamera perangkat.</summary>
    public FaceVerifyResult VerifikasiDariFoto(string namaSales, string path) =>
        _service.VerifikasiDariGambar(namaSales, path);

    public void Dispose() => _service.Dispose();
}
