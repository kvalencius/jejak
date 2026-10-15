namespace AbsensiSales.Mobile.ViewModels;

public partial class LaporanItemViewModel
{
    public required string NamaToko { get; init; }
    public required string KodeToko { get; init; }
    public required string Waktu { get; init; }
    public required string Alamat { get; init; }
    public required string Status { get; init; }
    public required string CheckGps { get; init; }
    public required string CheckWajah { get; init; }
    public required string CheckFoto { get; init; }

    public bool Tersimpan => Status == "Tersimpan";
}
