using SQLite;

namespace AbsensiSales.Core.Models;

public enum StatusPermohonan
{
    Pending,
    Disetujui,
    Ditolak
}

public class Permohonan
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    public int SalesId { get; set; }
    public string Jenis { get; set; } = "Cuti";
    public string TanggalMulai { get; set; } = string.Empty;
    public string TanggalSelesai { get; set; } = string.Empty;
    public string Alasan { get; set; } = string.Empty;
    public string Lampiran { get; set; } = string.Empty;
    public string DiajukanPada { get; set; } = string.Empty;
    public string Status { get; set; } = nameof(StatusPermohonan.Pending);
    public string CatatanHr { get; set; } = string.Empty;

    [Ignore]
    public StatusPermohonan StatusEnum =>
        Enum.TryParse<StatusPermohonan>(Status, out var value) ? value : StatusPermohonan.Pending;
}
