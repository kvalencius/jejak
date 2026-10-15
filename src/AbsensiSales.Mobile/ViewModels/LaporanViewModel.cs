using System.Collections.ObjectModel;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using AbsensiSales.Core.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.Mobile.ViewModels;

public partial class LaporanViewModel : ObservableObject
{
    private const int TargetKunjunganHarian = 8;

    private readonly SessionService _session;
    private readonly KunjunganRepository _kunjunganRepo;
    private readonly TokoRepository _tokoRepo;

    public LaporanViewModel(
        SessionService session,
        KunjunganRepository kunjunganRepo,
        TokoRepository tokoRepo)
    {
        _session = session;
        _kunjunganRepo = kunjunganRepo;
        _tokoRepo = tokoRepo;
    }

    public ObservableCollection<LaporanItemViewModel> DaftarKunjungan { get; } = new();

    [ObservableProperty]
    private string _judulTanggal = string.Empty;

    [ObservableProperty]
    private string _infoSales = string.Empty;

    [ObservableProperty]
    private int _jumlahKunjungan;

    [ObservableProperty]
    private string _absensiPertama = "-";

    [ObservableProperty]
    private string _statusAbsensiPertama = "Belum tercatat";

    [ObservableProperty]
    private string _statusTargetHarian = string.Empty;

    public async void OnAppearing()
    {
        var sales = _session.SalesAktif;
        if (sales is null)
        {
            await Shell.Current.GoToAsync("//login");
            return;
        }

        JudulTanggal = DateTime.Now.ToString("dd MMMM yyyy");
        InfoSales = $"{sales.Nama} - {sales.Nip} / {sales.Wilayah}";

        var kunjungan = await _kunjunganRepo.GetBySalesAsync(sales.Id, _session.TanggalHariIni);

        JumlahKunjungan = kunjungan.Count(k => k.StatusEnum == StatusKunjungan.Tersimpan);

        var pertama = kunjungan
            .Where(k => k.Status == nameof(StatusKunjungan.Tersimpan))
            .OrderBy(k => k.WaktuCatat)
            .FirstOrDefault();
        AbsensiPertama = pertama?.WaktuAbsensiAwal ?? "-";
        StatusAbsensiPertama = pertama is null ? "Belum tercatat" : "Tercatat - WIB";

        StatusTargetHarian = JumlahKunjungan >= TargetKunjunganHarian
            ? $"Target harian tercapai ({JumlahKunjungan}/{TargetKunjunganHarian})"
            : $"{JumlahKunjungan} toko tercatat";

        DaftarKunjungan.Clear();
        foreach (var k in kunjungan)
        {
            var toko = await _tokoRepo.GetByIdAsync(k.TokoId);
            DaftarKunjungan.Add(new LaporanItemViewModel
            {
                NamaToko = toko?.NamaToko ?? "Toko",
                KodeToko = toko?.KodeToko ?? "-",
                Waktu = k.WaktuCatat,
                Alamat = toko?.Alamat ?? string.Empty,
                Status = k.StatusLabel,
                CheckGps = k.StatusEnum == StatusKunjungan.Draf ? "- GPS belum ada" : "+ GPS sesuai",
                CheckWajah = k.SkorWajah > 0 ? "+ Wajah cocok" : "- Wajah belum ada",
                CheckFoto = k.StatusEnum == StatusKunjungan.Draf ? "- Foto belum ada" : "+ Foto lengkap"
            });
        }
    }

    [RelayCommand]
    private async Task AjukanIzinAsync() => await Shell.Current.GoToAsync("pengajuan-izin");

    [RelayCommand]
    private async Task LihatStatusPermohonanAsync() =>
        await Shell.Current.GoToAsync("status-permohonan");
}
