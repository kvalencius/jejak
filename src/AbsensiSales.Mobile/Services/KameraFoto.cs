namespace AbsensiSales.Mobile.Services;

/// <summary>
/// Ambil satu foto dari kamera perangkat dan kembalikan byte-nya.
/// Dipakai halaman pendaftaran maupun verifikasi wajah supaya penanganan
/// izin kamera dan pembacaan file tidak ditulis dua kali.
/// </summary>
internal static class KameraFoto
{
    /// <summary>
    /// Buka kamera lalu baca hasilnya. <c>Data</c> null berarti gagal atau
    /// dibatalkan; <c>Pesan</c> berisi alasan yang bisa ditampilkan ke pengguna.
    /// </summary>
    public static async Task<(byte[]? Data, string? Pesan)> AmbilAsync()
    {
        if (!MediaPicker.Default.IsCaptureSupported)
        {
            return (null, "Kamera tidak tersedia di perangkat ini. Verifikasi wajah memerlukan kamera.");
        }

        if (!await PastikanIzinAsync())
        {
            return (null, "Izin kamera belum aktif. Buka pengaturan aplikasi untuk mengizinkan kamera.");
        }

        FileResult? file;
        try
        {
            file = await MediaPicker.Default.CapturePhotoAsync();
        }
        catch (Exception)
        {
            return (null, "Kamera gagal dibuka. Silakan coba lagi.");
        }

        if (file is null)
        {
            return (null, "Pengambilan foto dibatalkan.");
        }

        try
        {
            using var stream = await file.OpenReadAsync();
            using var buffer = new MemoryStream();
            await stream.CopyToAsync(buffer);
            return (buffer.ToArray(), null);
        }
        catch (Exception)
        {
            return (null, "Foto gagal dibaca. Silakan ambil ulang.");
        }
    }

    private static async Task<bool> PastikanIzinAsync()
    {
        try
        {
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (status != PermissionStatus.Granted)
            {
                status = await Permissions.RequestAsync<Permissions.Camera>();
            }

            return status == PermissionStatus.Granted;
        }
        catch (FeatureNotSupportedException)
        {
            // Sebagian platform tidak punya dialog izin kamera.
            return true;
        }
        catch
        {
            return true;
        }
    }
}
