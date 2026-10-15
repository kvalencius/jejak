namespace AbsensiSales.HR.ViewModels;

public partial class PermohonanItem
{
    public required int Id { get; init; }
    public required string NamaSales { get; init; }
    public required string Nip { get; init; }
    public required string Jenis { get; init; }
    public required string Periode { get; init; }
    public required string Alasan { get; init; }
    public required string Lampiran { get; init; }
    public required string Diajukan { get; init; }
    public required string Status { get; init; }
    public required string CatatanHr { get; init; }

    public bool Pending => Status == "Pending";
}
