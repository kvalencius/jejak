namespace AbsensiSales.Face;

/// <summary>
/// Hasil verifikasi wajah. <see cref="Skor"/> adalah cosine similarity
/// (0-1) antara wajah di foto dan template tersimpan.
/// </summary>
public record FaceVerifyResult(bool Cocok, double Skor, string Pesan)
{
    /// <summary>Jumlah wajah yang terdeteksi pada foto verifikasi.</summary>
    public int JumlahWajah { get; init; }
}
