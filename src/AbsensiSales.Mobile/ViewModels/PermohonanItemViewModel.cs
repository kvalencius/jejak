namespace AbsensiSales.Mobile.ViewModels;

public partial class PermohonanItemViewModel
{
    public required string Jenis { get; init; }
    public required string Periode { get; init; }
    public required string Alasan { get; init; }
    public required string Status { get; init; }
    public required string Diajukan { get; init; }

    public bool Pending => Status == "Pending";
    public bool Disetujui => Status == "Disetujui";
    public bool Ditolak => Status == "Ditolak";
}
