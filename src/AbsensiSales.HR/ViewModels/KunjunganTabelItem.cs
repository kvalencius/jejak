namespace AbsensiSales.HR.ViewModels;

public partial class KunjunganTabelItem
{
    public required int Id { get; init; }
    public required string NamaSales { get; init; }
    public required string Nip { get; init; }
    public required string NamaToko { get; init; }
    public required string KodeToko { get; init; }
    public required string WaktuAwal { get; init; }
    public required string WaktuCatat { get; init; }
    public required string Lokasi { get; init; }
    public required string Status { get; init; }
    public required bool AdaWajah { get; init; }
    public required bool AdaFoto { get; init; }
}
