using System.Globalization;

namespace AbsensiSales.HR.ViewModels;

/// <summary>
/// Satu baris rekap gaji: komponen gaji disimpan agar slip PDF dan tabel
/// memakai angka yang sama (tanpa menghitung ulang dengan konstanta berbeda).
/// </summary>
public partial class PayrollItem
{
    public required string NamaSales { get; init; }
    public required string KodeSales { get; init; }
    public required int HariKerja { get; init; }
    public required int KunjunganToko { get; init; }
    public required int JumlahIzin { get; init; }
    public required string StatusTarget { get; init; }

    public required decimal GajiPokok { get; init; }
    public required decimal TunjanganKunjungan { get; init; }
    public required decimal PotonganIzin { get; init; }
    public required decimal TotalGaji { get; init; }

    public string TotalGajiText =>
        $"Rp {TotalGaji.ToString("N0", CultureInfo.InvariantCulture)}";

    public bool TargetTercapai => StatusTarget == "Target tercapai";
}
