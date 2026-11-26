using AbsensiSales.Core.Api;
using AbsensiSales.Core.Services;
using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

/// <summary>
/// Pendaftaran wajah: ambil foto, ubah menjadi token, lalu simpan ke backend.
/// Foto tidak pernah dikirim atau disimpan; hanya token Base64 (128 float32)
/// yang masuk ke tbl_sales.face_data lewat wajah.php.
/// </summary>
public partial class DaftarWajahViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly ApiService _api;
    private readonly FaceService _face;
    private readonly NotificationService _notif;

    private string? _tokenSementara;

    public DaftarWajahViewModel(
        SessionService session,
        ApiService api,
        FaceService face,
        NotificationService notif)
    {
        _session = session;
        _api = api;
        _face = face;
        _notif = notif;
    }

    /// <summary>"absensi" berarti setelah tersimpan langsung lanjut verifikasi.</summary>
    [ObservableProperty]
    private string _asal = string.Empty;

    [ObservableProperty]
    private string _namaSales = string.Empty;

    [ObservableProperty]
    private ImageSource? _pratinjauFoto;

    [ObservableProperty]
    private bool _adaFoto;

    /// <summary>True bila ada token baru yang belum disimpan ke server.</summary>
    [ObservableProperty]
    private bool _siapDisimpan;

    [ObservableProperty]
    private bool _sedangMemproses;

    [ObservableProperty]
    private string _judulStatus = "Ambil foto wajah Anda";

    [ObservableProperty]
    private string _pesanStatus = "Hadapkan wajah lurus ke kamera, tanpa masker atau penutup wajah.";

    public async void OnAppearing()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        NamaSales = sales.Nama;

        // Jika sudah punya token, jangan paksa daftar ulang; tapi tetap
        // izinkan ambil foto lagi untuk memperbarui.
        if (!string.IsNullOrWhiteSpace(sales.FaceData))
        {
            JudulStatus = "Wajah sudah terdaftar";
            PesanStatus = "Wajah Anda sudah tersimpan. Ambil foto lagi hanya jika ingin memperbarui.";
        }
    }

    [RelayCommand]
    private async Task AmbilFotoAsync()
    {
        if (SedangMemproses)
            return;

        SedangMemproses = true;
        try
        {
            var (foto, pesan) = await KameraFoto.AmbilAsync();
            if (foto is null)
            {
                JudulStatus = "Foto belum siap";
                PesanStatus = pesan ?? "Coba lagi.";
                await _notif.ShowErrorAsync(pesan ?? "Foto gagal diambil.");
                return;
            }

            PratinjauFoto = ImageSource.FromStream(() => new MemoryStream(foto));
            AdaFoto = true;

            JudulStatus = "Memproses wajah...";
            PesanStatus = "Mendeteksi wajah dan membuat token.";

            var hasil = await _face.BuatTemplateAsync(foto);
            if (!hasil.Berhasil)
            {
                _tokenSementara = null;
                SiapDisimpan = false;
                JudulStatus = "Wajah belum bisa diproses";
                PesanStatus = hasil.Pesan;
                await _notif.ShowErrorAsync(hasil.Pesan);
                return;
            }

            _tokenSementara = hasil.Template;
            SiapDisimpan = true;
            JudulStatus = "Wajah siap didaftarkan";
            PesanStatus = $"Token wajah berhasil dibuat ({hasil.JumlahWajah} wajah terdeteksi). Tekan Simpan wajah.";
            await _notif.ShowSuccessAsync("Wajah terbaca. Tekan Simpan wajah.");
        }
        finally
        {
            SedangMemproses = false;
        }
    }

    [RelayCommand]
    private async Task SimpanAsync()
    {
        if (SedangMemproses)
            return;

        if (string.IsNullOrWhiteSpace(_tokenSementara))
        {
            await _notif.ShowErrorAsync("Ambil foto wajah terlebih dahulu.");
            return;
        }

        SedangMemproses = true;
        try
        {
            JudulStatus = "Menyimpan token wajah...";
            PesanStatus = "Menghubungi server.";

            var hasil = await _api.SimpanWajahAsync(_tokenSementara);
            if (!hasil.Sukses)
            {
                JudulStatus = "Gagal menyimpan";
                PesanStatus = hasil.Pesan;
                await _notif.ShowErrorAsync(hasil.Pesan);
                return;
            }

            if (_session.SalesAktif is { } sales)
            {
                sales.FaceData = _tokenSementara;
            }

            SiapDisimpan = false;
            JudulStatus = "Wajah berhasil terdaftar";
            PesanStatus = "Token wajah tersimpan di akun Anda. Foto tidak disimpan.";
            await _notif.ShowSuccessAsync("Pendaftaran wajah berhasil.");

            await Task.Delay(700);
            await LanjutAsync();
        }
        finally
        {
            SedangMemproses = false;
        }
    }

    [RelayCommand]
    private async Task LanjutAsync()
    {
        if (string.Equals(Asal, "absensi", StringComparison.OrdinalIgnoreCase))
        {
            await Shell.Current.GoToAsync("verifikasi-wajah");
            return;
        }

        await Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task KembaliAsync() => await Shell.Current.GoToAsync("..");
}
