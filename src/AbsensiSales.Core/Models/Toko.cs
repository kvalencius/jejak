using SQLite;

namespace AbsensiSales.Core.Models;

public class Toko
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public string KodeToko { get; set; } = string.Empty;
    public string NamaToko { get; set; } = string.Empty;
    public string Alamat { get; set; } = string.Empty;
    public string Pemilik { get; set; } = string.Empty;
    public string Telepon { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double RadiusMeter { get; set; } = 100;
}
