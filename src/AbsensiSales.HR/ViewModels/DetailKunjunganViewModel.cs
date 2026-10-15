using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;

namespace AbsensiSales.HR.ViewModels;

[QueryProperty(nameof(KunjunganId), "id")]
public partial class DetailKunjunganViewModel : ObservableObject
{
    private readonly KunjunganRepository _kunjunganRepo;
    private readonly SalesRepository _salesRepo;
    private readonly TokoRepository _tokoRepo;

    public DetailKunjunganViewModel(
        KunjunganRepository kunjunganRepo,
        SalesRepository salesRepo,
        TokoRepository tokoRepo)
    {
        _kunjunganRepo = kunjunganRepo;
        _salesRepo = salesRepo;
        _tokoRepo = tokoRepo;
    }

    [ObservableProperty]
    private int _kunjunganId;

    [ObservableProperty]
    private string _judul = "Detail kunjungan";

    [ObservableProperty]
    private string _namaSales = string.Empty;

    [ObservableProperty]
    private string _infoSales = string.Empty;

    [ObservableProperty]
    private string _inisial = "?";

    [ObservableProperty]
    private string _namaToko = string.Empty;

    [ObservableProperty]
    private string _infoToko = string.Empty;

    [ObservableProperty]
    private string _status = string.Empty;

    [ObservableProperty]
    private string _waktuAwal = string.Empty;

    [ObservableProperty]
    private string _waktuCatat = string.Empty;

    [ObservableProperty]
    private string _lokasi = string.Empty;

    [ObservableProperty]
    private string _tujuan = string.Empty;

    [ObservableProperty]
    private string _catatan = string.Empty;

    public async Task MuatAsync()
    {
        var kunjungan = await _kunjunganRepo.GetByIdAsync(KunjunganId);
        if (kunjungan is null)
            return;

        var sales = await _salesRepo.GetByIdAsync(kunjungan.SalesId);
        var toko = await _tokoRepo.GetByIdAsync(kunjungan.TokoId);

        Judul = $"Detail kunjungan - {toko?.NamaToko ?? "Toko"}";
        NamaSales = sales?.Nama ?? "-";
        InfoSales = sales is null ? "-" : $"{sales.Nip} - {sales.Wilayah}";
        Inisial = sales?.Inisial ?? "?";
        NamaToko = toko?.NamaToko ?? "-";
        InfoToko = toko is null ? "-" : $"{toko.KodeToko} - {toko.Alamat}";
        Status = kunjungan.StatusLabel;
        WaktuAwal = kunjungan.WaktuAbsensiAwal;
        WaktuCatat = kunjungan.WaktuCatat;
        Lokasi = $"{kunjungan.Latitude:F4}, {kunjungan.Longitude:F4} - +/-{kunjungan.JarakMeter:F0} m";
        Tujuan = string.IsNullOrWhiteSpace(kunjungan.TujuanKunjungan)
            ? "-" : kunjungan.TujuanKunjungan;
        Catatan = string.IsNullOrWhiteSpace(kunjungan.Catatan)
            ? "Tidak ada catatan tambahan." : kunjungan.Catatan;
    }
}
