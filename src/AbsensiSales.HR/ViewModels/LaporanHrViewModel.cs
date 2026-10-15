using System.Collections.ObjectModel;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.HR.ViewModels;

public partial class LaporanHrViewModel : ObservableObject
{
    private readonly KunjunganRepository _kunjunganRepo;
    private readonly SalesRepository _salesRepo;
    private readonly TokoRepository _tokoRepo;
    private readonly Services.HrNavigationService _nav;

    public LaporanHrViewModel(
        KunjunganRepository kunjunganRepo,
        SalesRepository salesRepo,
        TokoRepository tokoRepo,
        Services.HrNavigationService nav)
    {
        _kunjunganRepo = kunjunganRepo;
        _salesRepo = salesRepo;
        _tokoRepo = tokoRepo;
        _nav = nav;
    }

    public ObservableCollection<KunjunganTabelItem> DaftarKunjungan { get; } = new();

    [ObservableProperty]
    private string _judulTanggal = string.Empty;

    [ObservableProperty]
    private string _salesHadir = "0 / 0";

    [ObservableProperty]
    private string _totalKunjungan = "0";

    [ObservableProperty]
    private string _berhasilTersimpan = "0";

    [ObservableProperty]
    private string _perluTindakLanjut = "0";

    [ObservableProperty]
    private string _jumlahHasil = string.Empty;

    [ObservableProperty]
    private string _filterStatus = "Semua status";

    public string[] PilihanStatus { get; } =
        { "Semua status", "Tersimpan", "Gagal validasi", "Draf" };

    [ObservableProperty]
    private KunjunganTabelItem? _kunjunganTerpilih;

    public async Task MuatAsync()
    {
        var tanggal = DateTime.Now.ToString("yyyy-MM-dd");
        JudulTanggal = DateTime.Now.ToString("dd MMMM yyyy");

        var semuaKunjungan = await _kunjunganRepo.GetAllAsync(tanggal);
        var semuaSales = await _salesRepo.GetAllAsync();

        int hadir = semuaSales
            .Count(s => semuaKunjungan.Any(k => k.SalesId == s.Id));
        SalesHadir = $"{hadir} / {semuaSales.Count}";
        TotalKunjungan = semuaKunjungan.Count.ToString();

        int tersimpan = semuaKunjungan.Count(k => k.Status == nameof(StatusKunjungan.Tersimpan));
        int bermasalah = semuaKunjungan.Count - tersimpan;
        BerhasilTersimpan = tersimpan.ToString();
        PerluTindakLanjut = bermasalah.ToString();

        await IsiTabelAsync(semuaKunjungan);
    }

    partial void OnFilterStatusChanged(string value) => _ = MuatAsync();

    private async Task IsiTabelAsync(List<AbsensiKunjungan> semua)
    {
        var hasil = FilterStatus switch
        {
            "Tersimpan" => semua.Where(k => k.Status == nameof(StatusKunjungan.Tersimpan)).ToList(),
            "Gagal validasi" => semua.Where(k => k.Status == nameof(StatusKunjungan.GagalValidasi)).ToList(),
            "Draf" => semua.Where(k => k.Status == nameof(StatusKunjungan.Draf)).ToList(),
            _ => semua
        };

        DaftarKunjungan.Clear();
        foreach (var k in hasil.OrderByDescending(k => k.WaktuCatat))
        {
            var sales = await _salesRepo.GetByIdAsync(k.SalesId);
            var toko = await _tokoRepo.GetByIdAsync(k.TokoId);

            DaftarKunjungan.Add(new KunjunganTabelItem
            {
                Id = k.Id,
                NamaSales = sales?.Nama ?? "-",
                Nip = sales?.Nip ?? "-",
                NamaToko = toko?.NamaToko ?? "-",
                KodeToko = toko?.KodeToko ?? "-",
                WaktuAwal = k.WaktuAbsensiAwal,
                WaktuCatat = k.WaktuCatat,
                Lokasi = k.Status == nameof(StatusKunjungan.Draf)
                    ? "Belum dicatat"
                    : k.Status == nameof(StatusKunjungan.GagalValidasi)
                        ? "Di luar radius"
                        : $"Sesuai - +/-{k.JarakMeter:F0} m",
                Status = k.StatusLabel,
                AdaWajah = k.SkorWajah > 0,
                AdaFoto = k.Status != nameof(StatusKunjungan.Draf)
            });
        }

        JumlahHasil = $"{DaftarKunjungan.Count} hasil - seluruh waktu dalam WIB";
    }

    [RelayCommand]
    private void BukaDetail(KunjunganTabelItem? item)
    {
        if (item is null)
            return;

        _nav.NavigateTo(typeof(Views.DetailKunjunganPage), item.Id);
    }

    [RelayCommand]
    private async Task EksporAsync() =>
        await Shell.Current.DisplayAlert("Ekspor laporan",
            "Laporan diekspor ke Excel/PDF (demo).", "OK");
}
