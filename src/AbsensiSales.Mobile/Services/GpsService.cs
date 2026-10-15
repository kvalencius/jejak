namespace AbsensiSales.Mobile.Services;

/// <summary>
/// Penyedia lokasi perangkat. Pada mockup berjalan di Windows desktop
/// (tanpa GPS), layanan ini mensimulasikan posisi di sekitar Surabaya
/// dengan simpangan kecil sehingga alur geofencing tetap bisa didemonstrasikan.
/// Saat dijalankan di perangkat Android asli, ganti implementasi dengan
/// Geolocation.Default (lihat https://learn.microsoft.com/dotnet/maui/platform-integration/device/geolocation).
/// </summary>
public class GpsService
{
    private readonly Random _random = new();

    public record Lokasi(double Latitude, double Longitude, double AkurasiMeter);

    /// <summary>Posisi simulasi: sedikit bergeser di sekitar lokasi toko terpilih.</summary>
    public Task<Lokasi?> GetLokasiAsync(double? dekatLat = null, double? dekatLon = null)
    {
        double baseLat = dekatLat ?? -7.2854;
        double baseLon = dekatLon ?? 112.7398;

        // Simpangan +/- ~0.00008 derajat (~ +/-9 meter)
        double offsetLat = (_random.NextDouble() - 0.5) * 0.00016;
        double offsetLon = (_random.NextDouble() - 0.5) * 0.00016;

        var lokasi = new Lokasi(baseLat + offsetLat, baseLon + offsetLon, 8);
        return Task.FromResult<Lokasi?>(lokasi);
    }

    /// <summary>Simulasi posisi yang sengaja berada di luar radius toko (untuk demo penolakan).</summary>
    public Task<Lokasi?> GetLokasiDiLuarRadiusAsync(double tokoLat, double tokoLon)
    {
        // ~0.002 derajat ~ +/-200 meter
        var lokasi = new Lokasi(tokoLat + 0.002, tokoLon + 0.002, 12);
        return Task.FromResult<Lokasi?>(lokasi);
    }
}
