using System.Diagnostics;
using System.Globalization;
using AbsensiSales.Core.Services;
using AbsensiSales.Face;
using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class VerifikasiWajahViewModel : ObservableObject
{
    private readonly SessionService _session;
    private readonly FaceService _face;
    private readonly AbsensiFlowState _flow;

    public VerifikasiWajahViewModel(SessionService session, FaceService face, AbsensiFlowState flow)
    {
        _session = session;
        _face = face;
        _flow = flow;
    }

    [ObservableProperty]
    private string _namaToko = string.Empty;

    [ObservableProperty]
    private string _identitasTerdeteksi = string.Empty;

    [ObservableProperty]
    private string _waktuVerifikasi = string.Empty;

    [ObservableProperty]
    private string _ringkasanSkor = string.Empty;

    [ObservableProperty]
    private bool _hasilCocok;

    [ObservableProperty]
    private bool _hasilDiproses;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LabelModeVerifikasi))]
    private bool _sudahDiperiksa;

    [ObservableProperty]
    private string _pesanHasil = string.Empty;

    [ObservableProperty]
    private ImageSource? _pratinjauWajah;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TeksTombolKamera))]
    [NotifyPropertyChangedFor(nameof(LabelKamera))]
    private bool _fotoSiap;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(LabelModeVerifikasi))]
    private bool _modeSimulasi;

    [ObservableProperty]
    private string _pesanKamera = string.Empty;

    /// <summary>Menampilkan tombol menuju pengaturan izin aplikasi.</summary>
    [ObservableProperty]
    private bool _tampilkanPengaturan;

    [ObservableProperty]
    private double _skorProgress;

    /// <summary>Teks tombol utama kamera, berubah sesuai ada/tidaknya foto.</summary>
    public string TeksTombolKamera => FotoSiap ? "Ambil ulang foto" : "Ambil foto wajah";

    /// <summary>Badge di sudut pratinjau kamera.</summary>
    public string LabelKamera => FotoSiap ? "FOTO SIAP" : "KAMERA PERANGKAT";

    /// <summary>Badge mode verifikasi; kosong sebelum pemindaian pertama.</summary>
    public string LabelModeVerifikasi => !SudahDiperiksa
        ? string.Empty
        : ModeSimulasi ? "MODE SIMULASI" : "SFACE AKTIF";

    public void OnAppearing()
    {
        NamaToko = _flow.TokoTerpilih?.NamaToko ?? "Toko";
        ModeSimulasi = _face.ModeSimulasi;

        HasilDiproses = false;
        SudahDiperiksa = false;
        HasilCocok = false;
        SkorProgress = 0;
        RingkasanSkor = string.Empty;
        PesanHasil = string.Empty;
        PesanKamera = string.Empty;
        IdentitasTerdeteksi = string.Empty;
        WaktuVerifikasi = string.Empty;

        // Tampilkan kembali foto wajah dari sesi ini (bila ada).
        if (_flow.FotoWajah is not { Length: > 0 } foto)
        {
            PratinjauWajah = null;
            FotoSiap = false;
            return;
        }

        PratinjauWajah = ImageSource.FromStream(() => new MemoryStream(foto));
        FotoSiap = true;

        // Pulihkan hasil pemindaian sebelumnya (mis. saat kembali dari halaman foto).
        if (_flow.SkorWajah <= 0)
            return;

        var sales = _session.SalesAktif;
        ModeSimulasi = _flow.WajahSimulasi;
        SudahDiperiksa = true;
        HasilCocok = true;
        SkorProgress = Math.Clamp(_flow.SkorWajah, 0, 1);
        RingkasanSkor = $"Kecocokan {_flow.SkorWajah * 100:F0}%";
        WaktuVerifikasi = _flow.WaktuVerifikasiWajah;
        IdentitasTerdeteksi = sales is null ? "Tidak dikenal" : $"{sales.Nama} - {sales.Nip}";
        PesanHasil = "Hasil tersimpan dari pemindaian sebelumnya.";
    }

    [RelayCommand]
    private async Task PindaiWajahAsync()
    {
        if (HasilDiproses)
            return;

        var sales = _session.SalesAktif;
        if (sales is null)
        {
            PesanKamera = "Sesi sales tidak ditemukan. Silakan masuk ulang.";
            return;
        }

        PesanKamera = string.Empty;
        PesanHasil = string.Empty;
        HasilDiproses = true;
        SudahDiperiksa = false;

#if ANDROID
        try
        {
            // Diagnostik: izin tidak lagi dipakai sebagai gerbang.
            // MediaPicker yang meminta izin bila belum aktif.
            var izin = await Permissions.CheckStatusAsync<Permissions.Camera>();
            Debug.WriteLine($"[VerifikasiWajah] Izin kamera sebelum ambil foto: {izin}");

            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                var izinSimpan = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                Debug.WriteLine($"[VerifikasiWajah] Izin penyimpanan sebelum ambil foto: {izinSimpan}");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VerifikasiWajah] Gagal membaca status izin: {ex}");
        }
#endif

        try
        {
            string? jalurFoto = null;
            byte[]? dataFoto = null;

            if (MediaPicker.Default.IsCaptureSupported)
            {
                try
                {
                    var file = await MediaPicker.Default.CapturePhotoAsync(new MediaPickerOptions
                    {
                        Title = "Ambil foto wajah"
                    });

                    if (file is null)
                    {
                        PesanKamera = "Pengambilan foto dibatalkan. Coba lagi saat wajah siap.";
                        return;
                    }

                    using var stream = await file.OpenReadAsync();
                    using var buffer = new MemoryStream();
                    await stream.CopyToAsync(buffer);
                    dataFoto = buffer.ToArray();
                    jalurFoto = file.FullPath;
                }
                catch (OperationCanceledException)
                {
                    PesanKamera = "Pengambilan foto dibatalkan. Coba lagi saat wajah siap.";
                    return;
                }
                catch (PermissionException ex)
                {
                    // MediaPicker sudah meminta izin sendiri; sampai di sini
                    // berarti masih ada izin yang belum aktif.
                    Debug.WriteLine($"[VerifikasiWajah] PermissionException: {ex}");
                    TampilkanPengaturan = true;
                    PesanKamera = await BuatPesanIzinAsync();
                    HasilDiproses = false;
                    await TawarkanPengaturanAsync();
                    return;
                }
                catch (FeatureNotSupportedException)
                {
                    PesanKamera = "Kamera perangkat tidak tersedia. Verifikasi memakai mode simulasi.";
                }
                catch (Exception)
                {
                    PesanKamera = "Kamera perangkat tidak dapat dibuka. Verifikasi memakai mode simulasi.";
                }
            }
            else
            {
                PesanKamera = "Kamera perangkat tidak tersedia di platform ini. Verifikasi memakai mode simulasi.";
            }

            if (dataFoto is { Length: > 0 })
            {
                _flow.FotoWajah = dataFoto;
                PratinjauWajah = ImageSource.FromStream(() => new MemoryStream(dataFoto));
                FotoSiap = true;
            }

            // Foto dari kamera perangkat diverifikasi lewat file;
            // bila kamera tidak tersedia, pakai jalur webcam/simulasi lama.
            var hasil = jalurFoto is not null
                ? _face.VerifikasiDariFoto(sales.Nama, jalurFoto)
                : _face.Verifikasi(sales.Nama);

            TerapkanHasil(sales.Nama, sales.Nip, hasil);
        }
        catch (Exception)
        {
            PesanKamera = "Verifikasi wajah gagal diproses. Silakan ambil foto ulang.";
        }
        finally
        {
            HasilDiproses = false;
        }
    }

    [RelayCommand]
    private async Task LanjutFotoAsync()
    {
        if (!HasilCocok)
        {
            await Shell.Current.DisplayAlertAsync("Verifikasi gagal",
                "Wajah belum cocok. Ulangi pemindaian sebelum melanjutkan.", "OK");
            return;
        }

        await Shell.Current.GoToAsync("foto-toko");
    }

    private void TerapkanHasil(string namaSales, string nip, FaceVerifyResult hasil)
    {
        ModeSimulasi = hasil.Simulasi;
        SudahDiperiksa = true;
        HasilCocok = hasil.Cocok;
        SkorProgress = Math.Clamp(hasil.Skor, 0, 1);

        IdentitasTerdeteksi = hasil.Cocok ? $"{namaSales} - {nip}" : "Tidak dikenal";
        WaktuVerifikasi = DateTime.Now.ToString("dd MMM yyyy - HH.mm.ss 'WIB'", new CultureInfo("id-ID"));
        RingkasanSkor = $"Kecocokan {(hasil.Skor * 100):F0}%";
        PesanHasil = hasil.Pesan;

        if (hasil.Cocok)
        {
            _flow.SkorWajah = hasil.Skor;
            _flow.WaktuVerifikasiWajah = WaktuVerifikasi;
            _flow.WajahSimulasi = hasil.Simulasi;
        }
        else
        {
            // Hasil lama tidak boleh dianggap sah setelah pemindaian gagal.
            _flow.SkorWajah = 0;
            _flow.WaktuVerifikasiWajah = string.Empty;
            _flow.WajahSimulasi = false;
        }
    }

    /// <summary>Membuka pengaturan aplikasi agar izin kamera bisa diaktifkan manual.</summary>
    [RelayCommand]
    private void BukaPengaturanIzin()
    {
        try
        {
            AppInfo.Current.ShowSettingsUI();
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VerifikasiWajah] Pengaturan tidak dapat dibuka: {ex}");
        }
    }

    private async Task TawarkanPengaturanAsync()
    {
        var buka = await Shell.Current.DisplayAlertAsync("Izin diperlukan",
            "Izinkan kamera dan penyimpanan untuk Jejak Sales di pengaturan Android, lalu kembali ke aplikasi.",
            "Buka pengaturan", "Nanti");

        if (buka)
            BukaPengaturanIzin();
    }

    /// <summary>Menentukan izin mana yang masih kurang untuk pesan yang akurat.</summary>
    private async Task<string> BuatPesanIzinAsync()
    {
#if ANDROID
        try
        {
            var kamera = await Permissions.CheckStatusAsync<Permissions.Camera>();
            if (kamera != PermissionStatus.Granted)
                return "Izin kamera belum aktif. Aktifkan lewat pengaturan aplikasi, lalu coba lagi.";

            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
            {
                var simpan = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();
                if (simpan != PermissionStatus.Granted)
                    return "Izin penyimpanan belum aktif. Aktifkan lewat pengaturan aplikasi, lalu coba lagi.";
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VerifikasiWajah] Gagal memeriksa izin: {ex}");
        }
#else
        await Task.CompletedTask;
#endif

        return "Izin aplikasi belum aktif. Aktifkan lewat pengaturan aplikasi, lalu coba lagi.";
    }

    /// <summary>
    /// Dipanggil saat aplikasi aktif kembali (mis. setelah dari pengaturan izin)
    /// agar pesan "ditolak" yang sudah basi dibersihkan.
    /// </summary>
    public async Task PeriksaIzinSetelahPengaturanAsync()
    {
#if ANDROID
        if (!TampilkanPengaturan)
            return;

        try
        {
            var kamera = await Permissions.CheckStatusAsync<Permissions.Camera>();
            var simpan = PermissionStatus.Granted;
            if (!OperatingSystem.IsAndroidVersionAtLeast(33))
                simpan = await Permissions.CheckStatusAsync<Permissions.StorageWrite>();

            Debug.WriteLine($"[VerifikasiWajah] Izin setelah kembali - kamera: {kamera}, penyimpanan: {simpan}");

            if (kamera != PermissionStatus.Granted || simpan != PermissionStatus.Granted)
                return;

            TampilkanPengaturan = false;
            PesanKamera = string.Empty;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[VerifikasiWajah] Gagal memeriksa izin kamera: {ex}");
        }
#else
        await Task.CompletedTask;
#endif
    }
}
