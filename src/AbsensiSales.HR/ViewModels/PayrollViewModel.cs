using System.Collections.ObjectModel;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace AbsensiSales.HR.ViewModels;

public partial class PayrollViewModel : ObservableObject
{
    private const decimal GajiPokok = 2_500_000m;
    private const decimal BonusPerKunjungan = 15_000m;
    private const int TargetKunjunganBulanan = 8 * 22; // 8 toko/hari x 22 hari kerja

    private readonly SalesRepository _salesRepo;
    private readonly KunjunganRepository _kunjunganRepo;
    private readonly PermohonanRepository _permohonanRepo;

    public PayrollViewModel(
        SalesRepository salesRepo,
        KunjunganRepository kunjunganRepo,
        PermohonanRepository permohonanRepo)
    {
        _salesRepo = salesRepo;
        _kunjunganRepo = kunjunganRepo;
        _permohonanRepo = permohonanRepo;
    }

    public ObservableCollection<PayrollItem> DaftarGaji { get; } = new();

    [ObservableProperty]
    private string _periode = DateTime.Now.ToString("MMMM yyyy");

    [ObservableProperty]
    private string _totalHariKerja = "22 hari";

    [ObservableProperty]
    private string _totalKunjunganTarget = "176 kunjungan";

    [ObservableProperty]
    private string _jumlahSales = "0";

    [ObservableProperty]
    private string _rekapSiap = "Belum dihitung";

    public async Task MuatAsync()
    {
        var semuaSales = await _salesRepo.GetAllAsync();
        var semuaKunjungan = await _kunjunganRepo.GetAllAsync();
        var semuaPermohonan = await _permohonanRepo.GetAllAsync();

        JumlahSales = semuaSales.Count.ToString();

        DaftarGaji.Clear();
        int totalKunjungan = 0;

        foreach (var s in semuaSales)
        {
            int kunjungan = semuaKunjungan
                .Count(k => k.SalesId == s.Id && k.Status == nameof(StatusKunjungan.Tersimpan));
            int izin = semuaPermohonan
                .Count(p => p.SalesId == s.Id && p.Status != nameof(StatusPermohonan.Ditolak));

            decimal bonus = kunjungan * BonusPerKunjungan;
            decimal potonganIzin = izin * (GajiPokok / 22m); // 1 hari kerja per izin
            decimal gaji = GajiPokok + bonus - potonganIzin;

            totalKunjungan += kunjungan;

            DaftarGaji.Add(new PayrollItem
            {
                Nama = s.Nama,
                Nip = s.Nip,
                HariKerja = 22 - izin,
                KunjunganTercatat = kunjungan,
                KunjunganTarget = TargetKunjunganBulanan,
                IzinCuti = izin,
                GajiAkhir = Math.Max(0, gaji)
            });
        }

        RekapSiap = $"{totalKunjungan} kunjungan tercatat - siap ditinjau";
    }

    [RelayCommand]
    private async Task HitungUlangAsync()
    {
        await MuatAsync();
        await Shell.Current.DisplayAlert("Perhitungan selesai",
            "Rekapitulasi gaji berhasil dihitung ulang dari data absensi & izin.", "OK");
    }

    [RelayCommand]
    private async Task SetujuiRekapAsync() =>
        await Shell.Current.DisplayAlert("Rekap disetujui",
            "Rekapitulasi gaji disetujui dan siap dicetak.", "OK");

    [RelayCommand]
    private async Task CetakSlipAsync() =>
        await Shell.Current.DisplayAlert("Cetak slip gaji",
            "Slip gaji diekspor ke PDF (demo).", "OK");
}
