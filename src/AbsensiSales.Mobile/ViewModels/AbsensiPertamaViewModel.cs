using System.IO;
using AbsensiSales.Core.Api;
using AbsensiSales.Core.Services;
using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Dispatching;

namespace AbsensiSales.Mobile.ViewModels;

public partial class AbsensiPertamaViewModel : ObservableObject
{
    private const int TargetKunjunganHarian = 8;

    private readonly SessionService _session;
    private readonly ApiService _api;
    private readonly GpsService _gps;
    private readonly NotificationService _notif;
    private readonly AbsensiFlowState _flow;

    private double _latitude;
    private double _longitude;
    private double _akurasi;
    private IDispatcherTimer? _jamTimer;

    public AbsensiPertamaViewModel(
        SessionService session,
        ApiService api,
        GpsService gps,
        NotificationService notif,
        AbsensiFlowState flowState)
    {
        _session = session;
        _api = api;
        _gps = gps;
        _notif = notif;
        _flow = flowState;
    }

    [ObservableProperty]
    private string _waktuSekarang = string.Empty;

    [ObservableProperty]
    private string _tanggalManusia = string.Empty;

    [ObservableProperty]
    private string _statusAbsensi = "Belum absen";

    [ObservableProperty]
    private int _kunjunganTercatat;

    [ObservableProperty]
    private string _alamatLokasi = "Lokasi belum tersedia";

    [ObservableProperty]
    private string _akurasiLokasi = "-";

    [ObservableProperty]
    private string _labelLokasi = "Lokasi perangkat";

    public WebViewSource? PetaUrl
    {
        get => _petaUrl;
        private set => SetProperty(ref _petaUrl, value);
    }

    private WebViewSource? _petaUrl;

    [ObservableProperty]
    private bool _gpsSiap;

    [ObservableProperty]
    private bool _kameraSiap = true;

    public double Progress => Math.Min(1, (double)KunjunganTercatat / TargetKunjunganHarian);

    public string LabelProgress => $"{KunjunganTercatat} dari {TargetKunjunganHarian} toko";

    /// <summary>Nomor absensi yang akan dicatat berikutnya (1-8).</summary>
    public int NomorAbsensi => Math.Min(TargetKunjunganHarian, KunjunganTercatat + 1);

    public bool TargetTercapai => KunjunganTercatat >= TargetKunjunganHarian;

    public bool BisaAbsen => !TargetTercapai;

    public string JudulAbsensi => TargetTercapai
        ? "Target harian tercapai"
        : KunjunganTercatat == 0
            ? "Absensi pertama"
            : $"Absensi ke-{NomorAbsensi}";

    public string LabelTombol => KunjunganTercatat == 0
        ? "Catat absensi pertama"
        : $"Catat absensi ke-{NomorAbsensi}";

    partial void OnKunjunganTercatatChanged(int value)
    {
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(LabelProgress));
        OnPropertyChanged(nameof(NomorAbsensi));
        OnPropertyChanged(nameof(TargetTercapai));
        OnPropertyChanged(nameof(BisaAbsen));
        OnPropertyChanged(nameof(JudulAbsensi));
        OnPropertyChanged(nameof(LabelTombol));
    }

    public async void OnAppearing()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        TanggalManusia = ApiMapper.Tanggal(DateTime.Now);
        WaktuSekarang = DateTime.Now.ToString("HH:mm:ss");
        MulaiJam();

        await MuatKunjunganHariIniAsync();

        // Simpan jam absensi pertama hari ini; jangan ditimpa saat kunjungan berikutnya.
        if (string.IsNullOrWhiteSpace(_flow.WaktuAbsensiAwal)
            || _flow.Tanggal != _session.TanggalHariIni)
        {
            _flow.WaktuAbsensiAwal = WaktuSekarang;
            _flow.Tanggal = _session.TanggalHariIni;
        }

        var lokasi = await _gps.GetLokasiAsync();
        if (lokasi is null)
        {
            GpsSiap = false;
            AlamatLokasi = "Lokasi perangkat belum tersedia";
            AkurasiLokasi = "Aktifkan izin lokasi untuk melanjutkan absensi.";
            LabelLokasi = "GPS tidak siap";
            MuatPetaDefault();
            return;
        }

        await PerbaruiLokasiAsync(lokasi);
    }

    private async Task PerbaruiLokasiAsync(GpsService.Lokasi lokasi)
    {
        GpsSiap = true;
        LabelLokasi = $"GPS +/-{lokasi.AkurasiMeter:F0} m";
        AkurasiLokasi = $"Akurasi +/-{lokasi.AkurasiMeter:F0} meter";
        AlamatLokasi = "Mencari alamat...";
        PerbaruiPeta(lokasi.Latitude, lokasi.Longitude, lokasi.AkurasiMeter);

        var alamat = await _gps.GetAlamatAsync(lokasi.Latitude, lokasi.Longitude);
        AlamatLokasi = string.IsNullOrWhiteSpace(alamat)
            ? "Alamat tidak tersedia"
            : alamat;
    }

    private void MulaiJam()
    {
        PerbaruiJam();

        if (_jamTimer is null)
        {
            var dispatcher = Application.Current?.Dispatcher;
            if (dispatcher is null)
            {
                return;
            }

            _jamTimer = dispatcher.CreateTimer();
            _jamTimer.Interval = TimeSpan.FromSeconds(1);
            _jamTimer.Tick += (_, _) => PerbaruiJam();
        }

        _jamTimer.Start();
    }

    /// <summary>Menghentikan pembaruan jam saat halaman tidak lagi tampil.</summary>
    public void HentikanJam() => _jamTimer?.Stop();

    private void PerbaruiJam()
    {
        var sekarang = DateTime.Now;
        WaktuSekarang = sekarang.ToString("HH:mm:ss");
        TanggalManusia = ApiMapper.Tanggal(sekarang);
    }

    private async void PerbaruiPeta(double lat, double lon, double akurasi)
    {
        _latitude = lat;
        _longitude = lon;
        _akurasi = akurasi;

        try
        {
            var html = await BuatHtmlPetaAsync(lat, lon, akurasi);
            PetaUrl = new HtmlWebViewSource { Html = html };
        }
        catch
        {
            PetaUrl = null;
        }
    }

    private async Task<string> BuatHtmlPetaAsync(double lat, double lon, double akurasi)
    {
        using var stream = await FileSystem.OpenAppPackageFileAsync("peta.html");
        using var reader = new StreamReader(stream);
        var html = await reader.ReadToEndAsync();

        var ci = System.Globalization.CultureInfo.InvariantCulture;
        return html
            .Replace("{LAT}", lat.ToString("F4", ci))
            .Replace("{LON}", lon.ToString("F4", ci))
            .Replace("{ZOOM}", "16")
            .Replace("{AKURASI}", akurasi.ToString("F0", ci));
    }

    /// <summary>Menampilkan peta default (Jakarta) saat GPS belum tersedia, agar WebView tidak kosong.</summary>
    private async void MuatPetaDefault()
    {
        try
        {
            // Koordinat pusat Jakarta
            var html = await BuatHtmlPetaAsync(-6.2088, 106.8456, 0);
            PetaUrl = new HtmlWebViewSource { Html = html };
        }
        catch
        {
            PetaUrl = null;
        }
    }

    private async Task MuatKunjunganHariIniAsync()
    {
        var hasil = await _api.GetKunjunganAsync(tanggal: _session.TanggalHariIni);
        if (!hasil.Sukses)
        {
            KunjunganTercatat = 0;
            StatusAbsensi = "Belum absen";
            await _notif.ShowErrorAsync(hasil.Pesan);
            return;
        }

        var daftar = hasil.Data ?? new List<KunjunganDto>();
        var tercatat = daftar
            .Where(k => !string.Equals(k.StatusVerifikasi, "Invalid", StringComparison.OrdinalIgnoreCase))
            .ToList();

        KunjunganTercatat = tercatat.Count;

        var pertama = tercatat
            .OrderBy(k => ApiMapper.Waktu(k.WaktuCheckin))
            .FirstOrDefault();

        StatusAbsensi = pertama is null
            ? "Belum absen"
            : $"Tercatat - pukul {ApiMapper.Jam(pertama.WaktuCheckin)}";
    }

    [RelayCommand]
    private async Task CatatAbsensiAsync()
    {
        var lokasi = await _gps.GetLokasiAsync();
        if (lokasi is null)
        {
            await _notif.ShowErrorAsync(
                "Lokasi perangkat tidak tersedia. Aktifkan GPS dan izin lokasi terlebih dahulu.");
            return;
        }

        GpsSiap = true;
        await PerbaruiLokasiAsync(lokasi);

        _flow.NomorAbsensi = NomorAbsensi;
        _flow.Tanggal = _session.TanggalHariIni;
        _flow.Latitude = lokasi.Latitude;
        _flow.Longitude = lokasi.Longitude;

        await Shell.Current.GoToAsync("pengisian-toko");
    }
}
