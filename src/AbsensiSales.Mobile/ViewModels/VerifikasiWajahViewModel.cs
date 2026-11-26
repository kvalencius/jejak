using AbsensiSales.Core.Api;
using AbsensiSales.Core.Services;
using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

/// <summary>
/// Verifikasi wajah untuk absensi.
///
/// Alur:
/// 1. Muat token wajah dari server (wajah.php). Bila belum ada, arahkan
///    pengguna ke halaman pendaftaran wajah lebih dulu.
/// 2. Ambil foto dari kamera, ubah menjadi embedding, lalu bandingkan
///    dengan token memakai cosine similarity.
/// 3. Lolos hanya jika skor &gt;= ambang <see cref="FaceScore.Threshold"/>.
///
/// Tidak ada jalur simulasi: tanpa model, verifikasi gagal dengan pesan jelas.
/// </summary>
public partial class VerifikasiWajahViewModel : ObservableObject
{
    /// <summary>Batas percobaan sebelum kembali ke pendaftaran/petunjuk.</summary>
    private const int MaksPercobaan = 3;

    private readonly SessionService _session;
    private readonly ApiService _api;
    private readonly FaceService _face;
    private readonly AbsensiFlowState _flow;
    private readonly NotificationService _notif;

    private string? _token;
    private int _percobaan;
    private bool _sedangBerjalan;

    public VerifikasiWajahViewModel(
        SessionService session,
        ApiService api,
        FaceService face,
        AbsensiFlowState flow,
        NotificationService notif)
    {
        _session = session;
        _api = api;
        _face = face;
        _flow = flow;
        _notif = notif;
    }

    [ObservableProperty]
    private string _namaToko = string.Empty;

    [ObservableProperty]
    private string _judulStatus = "Menyiapkan kamera...";

    [ObservableProperty]
    private string _pesanStatus = "Wajah akan dibandingkan dengan token yang tersimpan.";

    [ObservableProperty]
    private bool _sedangMemindai;

    [ObservableProperty]
    private bool _terverifikasi;

    /// <summary>True bila token wajah belum ada sehingga harus mendaftar dulu.</summary>
    [ObservableProperty]
    private bool _perluDaftar;

    [ObservableProperty]
    private bool _modelTidakSiap;

    /// <summary>Tombol pindai hanya tampil jika token ada dan model siap.</summary>
    public bool BisaPindai
    {
        get => !PerluDaftar && !ModelTidakSiap && !string.IsNullOrWhiteSpace(_token) && !Terverifikasi;
    }

    partial void OnPerluDaftarChanged(bool value) => OnPropertyChanged(nameof(BisaPindai));

    partial void OnModelTidakSiapChanged(bool value) => OnPropertyChanged(nameof(BisaPindai));

    partial void OnTerverifikasiChanged(bool value) => OnPropertyChanged(nameof(BisaPindai));

    /// <summary>Skor kemiripan terakhir, ditampilkan apa adanya ke pengguna.</summary>
    [ObservableProperty]
    private string _labelSkor = string.Empty;

    public async void OnAppearing()
    {
        NamaToko = _flow.TokoTerpilih?.NamaToko ?? "Toko";

        if (_sedangBerjalan || Terverifikasi)
            return;

        _sedangBerjalan = true;
        try
        {
            await SiapkanAsync();
        }
        finally
        {
            _sedangBerjalan = false;
        }
    }

    private async Task SiapkanAsync()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
        {
            JudulStatus = "Sesi tidak ditemukan";
            PesanStatus = "Silakan masuk kembali untuk memverifikasi wajah.";
            return;
        }

        // Engine wajah harus siap sebelum apa pun; tanpa model tidak ada verifikasi.
        JudulStatus = "Memuat model wajah...";
        PesanStatus = "Proses pertama bisa memakan waktu beberapa detik.";

        if (!await _face.PastikanSiapAsync())
        {
            ModelTidakSiap = true;
            JudulStatus = "Model wajah belum tersedia";
            PesanStatus = _face.Pesan
                ?? "Letakkan version-RFB-320.onnx dan face_recognition_sface_2021dec.onnx di folder models.";
            await _notif.ShowErrorAsync("Verifikasi wajah tidak dapat dijalankan tanpa model.");
            return;
        }

        ModelTidakSiap = false;

        // Ambil token terbaru dari server; data login bisa sudah usang.
        JudulStatus = "Memeriksa data wajah...";
        PesanStatus = "Menghubungi server.";

        var status = await _api.GetStatusWajahAsync();
        if (!status.Sukses || status.Data is null)
        {
            JudulStatus = "Data wajah gagal dimuat";
            PesanStatus = status.Pesan;
            await _notif.ShowErrorAsync(status.Pesan);
            return;
        }

        if (!status.Data.Terdaftar || string.IsNullOrWhiteSpace(status.Data.FaceData))
        {
            PerluDaftar = true;
            JudulStatus = "Wajah belum terdaftar";
            PesanStatus = "Daftarkan wajah Anda dulu sebelum melakukan verifikasi.";
            return;
        }

        _token = status.Data.FaceData;
        sales.FaceData = _token;

        PerluDaftar = false;
        OnPropertyChanged(nameof(BisaPindai));
        JudulStatus = "Siap memindai";
        PesanStatus = "Tekan Pindai wajah, lalu hadapkan wajah lurus ke kamera.";
    }

    [RelayCommand]
    private async Task PindaiAsync()
    {
        if (_sedangBerjalan || Terverifikasi)
            return;

        if (string.IsNullOrWhiteSpace(_token))
        {
            await _notif.ShowErrorAsync("Token wajah belum tersedia. Daftarkan wajah terlebih dahulu.");
            return;
        }

        _sedangBerjalan = true;
        SedangMemindai = true;
        try
        {
            var (foto, pesan) = await KameraFoto.AmbilAsync();
            if (foto is null)
            {
                JudulStatus = "Foto belum siap";
                PesanStatus = pesan ?? "Coba lagi.";
                await _notif.ShowInfoAsync(pesan ?? "Verifikasi dibatalkan.");
                return;
            }

            JudulStatus = "Membandingkan wajah...";
            PesanStatus = "Menghitung kemiripan dengan token tersimpan.";

            var hasil = await _face.VerifikasiAsync(foto, _token);

            LabelSkor = $"Kemiripan {hasil.Skor:P0} (batas {FaceScore.Threshold:P0})";

            if (hasil.Cocok)
            {
                Terverifikasi = true;
                JudulStatus = "Wajah terverifikasi";
                PesanStatus = hasil.Pesan;

                _flow.SkorWajah = hasil.Skor;
                _flow.WaktuVerifikasiWajah = DateTime.Now.ToString("dd MMM yyyy - HH.mm.ss 'WIB'");

                await _notif.ShowSuccessAsync("Wajah cocok.");
                await Task.Delay(900);
                await Shell.Current.GoToAsync("foto-toko");
                return;
            }

            _percobaan++;
            JudulStatus = "Wajah tidak cocok";
            PesanStatus = _percobaan >= MaksPercobaan
                ? $"{hasil.Pesan} Sudah {_percobaan}x percobaan. Pastikan cahaya cukup dan hanya wajah Anda yang terlihat, lalu coba lagi."
                : $"{hasil.Pesan} Sudah {_percobaan}x percobaan dari batas {MaksPercobaan}.";

            await _notif.ShowErrorAsync(hasil.Pesan);
        }
        finally
        {
            SedangMemindai = false;
            _sedangBerjalan = false;
        }
    }

    [RelayCommand]
    private async Task DaftarWajahAsync()
        => await Shell.Current.GoToAsync("daftar-wajah?asal=absensi");
}
