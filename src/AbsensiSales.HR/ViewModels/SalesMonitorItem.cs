namespace AbsensiSales.HR.ViewModels;

public partial class SalesMonitorItem
{
    public required string Inisial { get; init; }
    public required string Nama { get; init; }
    public required string Nip { get; init; }
    public required string Wilayah { get; init; }
    public required string Status { get; init; }
    public required string Waktu { get; init; }
    public required int SalesId { get; init; }

    public bool Hadir => Status == "Hadir";
}
