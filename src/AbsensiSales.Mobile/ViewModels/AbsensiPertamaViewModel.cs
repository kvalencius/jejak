using System.Globalization;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using AbsensiSales.Core.Services;
using AbsensiSales.Mobile.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
// Tambahkan ini jika belum ada (biasanya sudah ada bawaan MAUI)
using Microsoft.Maui.Dispatching;

namespace AbsensiSales.Mobile.ViewModels;

public partial class AbsensiPertamaViewModel : ObservableObject
{
    private const int TargetKunjunganHarian = 8;

    private readonly SessionService _session;
    private readonly KunjunganRepository _kunjunganRepo;
    private readonly GpsService _gps;
    private readonly AbsensiFlowState _flow;

    // 1. Tambahkan variabel untuk Timer
    private IDispatcherTimer _clockTimer;

    public AbsensiPertamaViewModel(
        SessionService session,
        KunjunganRepository kunjunganRepo,
        GpsService gps,
        AbsensiFlowState flowState)
    {
        _session = session;
        _kunjunganRepo = kunjunganRepo;
        _gps = gps;
        _flow = flowState;
    }

    [ObservableProperty]
    private string _namaSales = string.Empty;

    [ObservableProperty]
    private string _infoSales = string.Empty;

    [ObservableProperty]
    private string _waktuSekarang = string.Empty;

    [ObservableProperty]
    private string _tanggalManusia = string.Empty;

    [ObservableProperty]
    private string _statusAbsensi = "Belum absen";

    [ObservableProperty]
    private int _kunjunganTercatat;

    [ObservableProperty]
    private string _alamatTokoContoh = "Jl. Raya Darmo No. 24, Surabaya";

    [ObservableProperty]
    private string _koordinatContoh = "-7.2854, 112.7398 - Akurasi +/-8 meter";

    [ObservableProperty]
    private string _labelTokoContoh = "Toko Sumber Jaya - GPS +/-8 m";

    [ObservableProperty]
    private bool _gpsSiap = true;

    [ObservableProperty]
    private bool _kameraSiap = true;

    public double Progress => (double)KunjunganTercatat / TargetKunjunganHarian;

    public string LabelProgress => $"{KunjunganTercatat} dari {TargetKunjunganHarian} toko";

    partial void OnKunjunganTercatatChanged(int value)
    {
        OnPropertyChanged(nameof(Progress));
        OnPropertyChanged(nameof(LabelProgress));
    }

    public async void OnAppearing()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        NamaSales = sales.Nama;
        InfoSales = $"{sales.Nip} - {sales.Wilayah}";
        TanggalManusia = DateTime.Now.ToString("dddd, dd MMMM yyyy", new CultureInfo("id-ID"));

        // Update waktu pertama kali buka
        WaktuSekarang = DateTime.Now.ToString("HH:mm:ss");

        // 2. Inisialisasi dan jalankan Timer
        MulaiJamDigital();

        KunjunganTercatat = await _kunjunganRepo.HitungKunjunganHariIniAsync(
            sales.Id, _session.TanggalHariIni);

        StatusAbsensi = KunjunganTercatat > 0
            ? $"Tercatat - pukul {WaktuSekarang}"
            : "Belum absen";

        if (_flow.KunjunganId == 0 && _flow.TokoTerpilih is null)
        {
            _flow.WaktuAbsensiAwal = WaktuSekarang;
            _flow.Tanggal = _session.TanggalHariIni;
        }
    }

    // 3. Method untuk menghentikan timer (Panggil ini di halaman .xaml.cs saat OnDisappearing)
    public void OnDisappearing()
    {
        if (_clockTimer is not null && _clockTimer.IsRunning)
        {
            _clockTimer.Stop();
        }
    }

    private void MulaiJamDigital()
    {
        // Hindari membuat timer ganda jika OnAppearing dipanggil berkali-kali
        if (_clockTimer is null)
        {
            // Buat timer yang berjalan di UI Thread bawaan MAUI
            _clockTimer = Application.Current.Dispatcher.CreateTimer();
            _clockTimer.Interval = TimeSpan.FromSeconds(1); // Update setiap 1 detik
            _clockTimer.Tick += (s, e) =>
            {
                // Nilai ini akan otomatis meng-update UI karena memakai [ObservableProperty]
                WaktuSekarang = DateTime.Now.ToString("HH:mm:ss");
            };
        }

        if (!_clockTimer.IsRunning)
        {
            _clockTimer.Start();
        }
    }

    [RelayCommand]
    private async Task KeluarAsync()
    {
        var yakin = await Shell.Current.DisplayAlertAsync(
            "Keluar dari akun?",
            "Anda akan kembali ke halaman login.",
            "Keluar",
            "Batal");

        if (!yakin)
            return;

        OnDisappearing();
        _session.Logout();
        _flow.Reset();

        await Shell.Current.GoToAsync("//login");
    }

    [RelayCommand]
    private async Task CatatAbsensiAsync()
    {
        var lokasi = await _gps.GetLokasiAsync();
        if (lokasi is not null)
        {
            KoordinatContoh = $"{lokasi.Latitude:F4}, {lokasi.Longitude:F4} - Akurasi +/-{lokasi.AkurasiMeter:F0} meter";
        }

        _flow.WaktuAbsensiAwal = DateTime.Now.ToString("HH:mm:ss");
        _flow.Tanggal = _session.TanggalHariIni;

        await Shell.Current.GoToAsync("pengisian-toko");
    }
}