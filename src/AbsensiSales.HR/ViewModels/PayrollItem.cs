namespace AbsensiSales.HR.ViewModels;

public partial class PayrollItem
{
    public required string Nama { get; init; }
    public required string Nip { get; init; }
    public required int HariKerja { get; init; }
    public required int KunjunganTercatat { get; init; }
    public required int KunjunganTarget { get; init; }
    public required int IzinCuti { get; init; }
    public required decimal GajiAkhir { get; init; }

    public string StatusTarget => KunjunganTercatat >= KunjunganTarget ? "Target tercapai" : "Belum tercapai";
}
