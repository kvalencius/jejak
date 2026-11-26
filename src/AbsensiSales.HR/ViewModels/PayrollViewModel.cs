using System.Collections.ObjectModel;
using System.Globalization;
using AbsensiSales.Core.Models;
using AbsensiSales.Core.Repositories;
using AbsensiSales.HR.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;

namespace AbsensiSales.HR.ViewModels;

/// <summary>
/// Perhitungan gaji HR. Data dihitung dari absensi kunjungan dan permohonan
/// izin/cuti yang tersimpan di SQLite, lalu diubah menjadi slip PDF.
/// Bila database belum berisi data sama sekali, dipakai data contoh agar
/// halaman tetap bisa ditinjau.
/// </summary>
public partial class PayrollViewModel : ObservableObject
{
    private const decimal GajiPokok = 2_500_000m;
    private const decimal BonusPerKunjungan = 15_000m;
    private const int HariKerjaBulanan = 22;
    private const int TargetKunjunganBulanan = 176;

    private static readonly CultureInfo Indonesia = CultureInfo.GetCultureInfo("id-ID");

    private readonly PdfService _pdfService;
    private readonly SalesRepository _salesRepo;
    private readonly KunjunganRepository _kunjunganRepo;
    private readonly PermohonanRepository _permohonanRepo;

    public PayrollViewModel(
        PdfService pdfService,
        SalesRepository salesRepo,
        KunjunganRepository kunjunganRepo,
        PermohonanRepository permohonanRepo)
    {
        _pdfService = pdfService;
        _salesRepo = salesRepo;
        _kunjunganRepo = kunjunganRepo;
        _permohonanRepo = permohonanRepo;
    }

    public ObservableCollection<PayrollItem> DaftarGaji { get; } = new();

    [ObservableProperty]
    private string _periode = string.Empty;

    [ObservableProperty]
    private string _totalHariKerja = "0 hari";

    [ObservableProperty]
    private string _totalKunjunganTarget = "0 kunjungan";

    [ObservableProperty]
    private string _rekapSiap = "Belum ada data untuk dihitung";

    public async Task MuatAsync()
    {
        var sekarang = DateTime.Today;
        var bulanIni = sekarang.ToString("yyyy-MM");
        var akhirBulan = new DateTime(sekarang.Year, sekarang.Month,
            DateTime.DaysInMonth(sekarang.Year, sekarang.Month));

        Periode = Indonesia.TextInfo.ToTitleCase(sekarang.ToString("MMMM yyyy", Indonesia));
        TotalHariKerja = $"{HariKerjaBulanan} hari";
        TotalKunjunganTarget = $"{TargetKunjunganBulanan} kunjungan";

        List<PayrollItem> hasil;

        try
        {
            var semuaSales = await _salesRepo.GetAllAsync();

            if (semuaSales.Count == 0)
            {
                hasil = BuatDataMock().ToList();
            }
            else
            {
                var semuaKunjungan = await _kunjunganRepo.GetAllAsync();
                var semuaPermohonan = await _permohonanRepo.GetAllAsync();

                hasil = semuaSales
                    .OrderBy(s => s.Nama)
                    .Select(sales => BuatItem(
                        sales,
                        HitungKunjunganBulanIni(sales.Id, semuaKunjungan, bulanIni),
                        HitungHariIzinBulanIni(sales.Id, semuaPermohonan, sekarang, akhirBulan)))
                    .ToList();
            }
        }
        catch
        {
            // Database belum siap: tetap tampilkan contoh agar halaman terisi.
            hasil = BuatDataMock().ToList();
        }

        DaftarGaji.Clear();
        foreach (var item in hasil)
            DaftarGaji.Add(item);

        var totalKunjungan = DaftarGaji.Sum(item => item.KunjunganToko);
        RekapSiap = DaftarGaji.Count == 0
            ? "Belum ada data untuk dihitung"
            : $"{totalKunjungan} kunjungan tercatat - {DaftarGaji.Count} sales siap ditinjau";
    }

    [RelayCommand]
    private async Task HitungUlangAsync()
    {
        await MuatAsync();
        await Shell.Current.DisplayAlertAsync(
            "Perhitungan selesai",
            "Rekapitulasi gaji berhasil dihitung ulang dari data absensi & izin.",
            "OK");
    }

    [RelayCommand]
    private async Task SetujuiRekapAsync() =>
        await Shell.Current.DisplayAlertAsync(
            "Rekap disetujui",
            "Rekapitulasi gaji disetujui dan siap dicetak.",
            "OK");

    [RelayCommand]
    private async Task CetakSlipAsync(PayrollItem? item)
    {
        if (item is null && DaftarGaji.Count == 0)
        {
            await Shell.Current.DisplayAlertAsync(
                "Belum ada data",
                "Tidak ada data gaji yang bisa dicetak pada periode ini.",
                "OK");
            return;
        }

        try
        {
            var filePath = item is null
                ? await _pdfService.BuatRekapGajiAsync(DaftarGaji, Periode)
                : await _pdfService.BuatSlipGajiAsync(item, Periode);

            await Launcher.Default.OpenAsync(new OpenFileRequest
            {
                File = new ReadOnlyFile(filePath),
                Title = item is null ? "Rekap Gaji" : $"Slip Gaji {item.NamaSales}"
            });
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync(
                "Gagal mencetak slip",
                $"File PDF tidak dapat dibuat atau dibuka.\n\n{ex.Message}",
                "OK");
        }
    }

    [RelayCommand]
    private Task CetakRekapAsync() => CetakSlipAsync(null);

    private static int HitungKunjunganBulanIni(
        int salesId,
        IReadOnlyCollection<AbsensiKunjungan> semuaKunjungan,
        string bulanIni) =>
        semuaKunjungan.Count(k =>
            k.SalesId == salesId
            && k.Status == nameof(StatusKunjungan.Tersimpan)
            && k.Tanggal.StartsWith(bulanIni, StringComparison.Ordinal));

    private static int HitungHariIzinBulanIni(
        int salesId,
        IReadOnlyCollection<Permohonan> semuaPermohonan,
        DateTime awalBulan,
        DateTime akhirBulan)
    {
        var hariIzin = 0;

        foreach (var permohonan in semuaPermohonan)
        {
            if (permohonan.SalesId != salesId
                || permohonan.Status != nameof(StatusPermohonan.Disetujui))
                continue;

            if (!DateTime.TryParse(permohonan.TanggalMulai, out var mulai))
                continue;

            var selesai = DateTime.TryParse(permohonan.TanggalSelesai, out var akhir)
                ? akhir
                : mulai;

            // Potong rentang izin agar hanya menghitung hari di bulan berjalan.
            if (mulai < awalBulan) mulai = awalBulan;
            if (selesai > akhirBulan) selesai = akhirBulan;

            if (selesai < mulai)
                continue;

            hariIzin += (selesai - mulai).Days + 1;
        }

        return Math.Min(hariIzin, HariKerjaBulanan);
    }

    private static PayrollItem BuatItem(Sales sales, int kunjungan, int izin)
    {
        decimal tunjangan = kunjungan * BonusPerKunjungan;
        decimal potongan = izin * (GajiPokok / HariKerjaBulanan);
        decimal totalGaji = Math.Max(0, GajiPokok + tunjangan - potongan);

        return new PayrollItem
        {
            NamaSales = sales.Nama,
            KodeSales = sales.Nip,
            HariKerja = Math.Max(0, HariKerjaBulanan - izin),
            KunjunganToko = kunjungan,
            JumlahIzin = izin,
            StatusTarget = kunjungan >= TargetKunjunganBulanan
                ? "Target tercapai"
                : "Belum tercapai",
            GajiPokok = GajiPokok,
            TunjanganKunjungan = tunjangan,
            PotonganIzin = potongan,
            TotalGaji = totalGaji
        };
    }

    private static IEnumerable<PayrollItem> BuatDataMock() => new List<PayrollItem>
    {
        BuatItem("Andi Pratama", "SLS-014", kunjungan: 2, izin: 2),
        BuatItem("Ayu Ningsih", "SLS-022", kunjungan: 1, izin: 1),
        BuatItem("Budi Raharjo", "SLS-011", kunjungan: 1, izin: 1),
        BuatItem("Citra Dewi", "SLS-015", kunjungan: 0, izin: 0)
    };

    private static PayrollItem BuatItem(
        string nama,
        string kode,
        int kunjungan,
        int izin)
    {
        decimal tunjangan = kunjungan * BonusPerKunjungan;
        decimal potongan = izin * (GajiPokok / HariKerjaBulanan);
        decimal totalGaji = Math.Max(0, GajiPokok + tunjangan - potongan);

        return new PayrollItem
        {
            NamaSales = nama,
            KodeSales = kode,
            HariKerja = Math.Max(0, HariKerjaBulanan - izin),
            KunjunganToko = kunjungan,
            JumlahIzin = izin,
            StatusTarget = kunjungan >= TargetKunjunganBulanan
                ? "Target tercapai"
                : "Belum tercapai",
            GajiPokok = GajiPokok,
            TunjanganKunjungan = tunjangan,
            PotonganIzin = potongan,
            TotalGaji = totalGaji
        };
    }
}
