using AbsensiSales.Core.Models;
using SQLite;

namespace AbsensiSales.Core.Data;

/// <summary>
/// Akses database SQLite lokal mengikuti pola repository dari dokumentasi
/// Microsoft Learn (.NET MAUI - "Local data" / "Databases").
/// File database disimpan di folder data aplikasi.
/// </summary>
public class AppDatabase
{
    private const string FileName = "absensi_sales.db3";
    private SQLiteAsyncConnection? _connection;
    private readonly string _folderPath;

    public AppDatabase(string? folderPath = null)
    {
        _folderPath = folderPath ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "AbsensiSales");
    }

    public string DatabasePath => Path.Combine(_folderPath, FileName);

    public async Task<SQLiteAsyncConnection> GetConnectionAsync()
    {
        if (_connection is not null)
            return _connection;

        Directory.CreateDirectory(_folderPath);
        _connection = new SQLiteAsyncConnection(DatabasePath,
            SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);

        await _connection.CreateTableAsync<Sales>();
        await _connection.CreateTableAsync<Toko>();
        await _connection.CreateTableAsync<AbsensiKunjungan>();
        await _connection.CreateTableAsync<Permohonan>();

        await SeedDataAsync(_connection);
        await SeedPermohonanSalesUtamaAsync(_connection);
        return _connection;
    }

    private static async Task SeedDataAsync(SQLiteAsyncConnection db)
    {
        if (await db.Table<Sales>().CountAsync() > 0)
            return;

        var sales = new List<Sales>
        {
            new() { Nip = "SLS-014", Nama = "Andi Pratama", Username = "andi.pratama", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "andi.jpg" },
            new() { Nip = "SLS-008", Nama = "Dewi Lestari", Username = "dewi.lestari", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "dewi.jpg" },
            new() { Nip = "SLS-019", Nama = "Rizky Putra", Username = "rizky.putra", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "rizky.jpg" },
            new() { Nip = "SLS-022", Nama = "Ayu Ningsih", Username = "ayu.ningsih", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "ayu.jpg" },
            new() { Nip = "SLS-011", Nama = "Budi Raharjo", Username = "budi.raharjo", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "budi.jpg" },
            new() { Nip = "SLS-015", Nama = "Citra Dewi", Username = "citra.dewi", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "citra.jpg" },
            new() { Nip = "SLS-017", Nama = "Doni Kurniawan", Username = "doni.kurniawan", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "doni.jpg" },
            new() { Nip = "SLS-021", Nama = "Eka Wijaya", Username = "eka.wijaya", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "eka.jpg" },
            new() { Nip = "SLS-024", Nama = "Fitri Handayani", Username = "fitri.handayani", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "fitri.jpg" },
            new() { Nip = "SLS-026", Nama = "Gunawan Saputra", Username = "gunawan.saputra", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "gunawan.jpg" },
            new() { Nip = "SLS-029", Nama = "Hana Puspita", Username = "hana.puspita", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "hana.jpg" },
            new() { Nip = "SLS-031", Nama = "Indra Maulana", Username = "indra.maulana", Password = "demo123", Wilayah = "Surabaya Pusat", FotoReferensi = "indra.jpg" },
        };
        await db.InsertAllAsync(sales);

        var toko = new List<Toko>
        {
            new() { KodeToko = "TK-021", NamaToko = "Toko Sumber Jaya", Alamat = "Jl. Raya Darmo No. 24, Surabaya", Pemilik = "Budi Santoso", Telepon = "0812 3456 7801", Latitude = -7.2854, Longitude = 112.7398, RadiusMeter = 100 },
            new() { KodeToko = "TK-034", NamaToko = "Toko Makmur", Alamat = "Jl. Diponegoro No. 18, Surabaya", Pemilik = "Siti Rahma", Telepon = "0813 5567 2210", Latitude = -7.2710, Longitude = 112.7490, RadiusMeter = 100 },
            new() { KodeToko = "TK-015", NamaToko = "Toko Berkah", Alamat = "Jl. Rungkut Asri Tengah No. 5, Surabaya", Pemilik = "Agus Salim", Telepon = "0857 8890 4432", Latitude = -7.3340, Longitude = 112.7900, RadiusMeter = 100 },
            new() { KodeToko = "TK-042", NamaToko = "Toko Sentosa", Alamat = "Jl. Kenanga No. 3, Surabaya", Pemilik = "Maya Lestari", Telepon = "0821 4455 6677", Latitude = -7.2580, Longitude = 112.7280, RadiusMeter = 100 },
            new() { KodeToko = "TK-063", NamaToko = "Toko Sejahtera", Alamat = "Jl. Margorejo No. 88, Surabaya", Pemilik = "Hendra Wijaya", Telepon = "0819 3322 1100", Latitude = -7.3210, Longitude = 112.7650, RadiusMeter = 100 },
            new() { KodeToko = "TK-056", NamaToko = "Toko Maju Bersama", Alamat = "Jl. Kartini No. 7, Surabaya", Pemilik = "Rina Astuti", Telepon = "0813 7788 9900", Latitude = -7.2490, Longitude = 112.7430, RadiusMeter = 100 },
            new() { KodeToko = "TK-071", NamaToko = "Toko Barokah", Alamat = "Jl. Dharmahusada No. 41, Surabaya", Pemilik = "Joko Prasetyo", Telepon = "0856 1122 3344", Latitude = -7.2950, Longitude = 112.7810, RadiusMeter = 100 },
            new() { KodeToko = "TK-078", NamaToko = "Toko Wahyu Cell", Alamat = "Jl. Petemon Pandean No. 12, Surabaya", Pemilik = "Wahyu Hidayat", Telepon = "0812 9900 5566", Latitude = -7.2400, Longitude = 112.7120, RadiusMeter = 100 },
        };
        await db.InsertAllAsync(toko);

        var andi = sales[0];
        var tanggalHariIni = DateTime.Now.ToString("yyyy-MM-dd");

        await db.InsertAsync(new AbsensiKunjungan
        {
            SalesId = andi.Id,
            TokoId = toko[0].Id,
            Tanggal = tanggalHariIni,
            WaktuAbsensiAwal = "08:00:12",
            WaktuCatat = "08:06:02",
            Latitude = -7.2854,
            Longitude = 112.7398,
            JarakMeter = 8,
            SkorWajah = 0.98,
            TujuanKunjungan = "Penawaran & pemesanan produk",
            Catatan = "Pengecekan stok dan penawaran paket Oktober.",
            Status = nameof(StatusKunjungan.Tersimpan)
        });

        await db.InsertAsync(new AbsensiKunjungan
        {
            SalesId = andi.Id,
            TokoId = toko[1].Id,
            Tanggal = tanggalHariIni,
            WaktuAbsensiAwal = "08:00:12",
            WaktuCatat = "10:18:30",
            Latitude = -7.2710,
            Longitude = 112.7490,
            JarakMeter = 6,
            SkorWajah = 0.95,
            TujuanKunjungan = "Kunjungan rutin",
            Catatan = "Restock display rak depan.",
            Status = nameof(StatusKunjungan.Tersimpan)
        });

        await db.InsertAsync(new AbsensiKunjungan
        {
            SalesId = sales[1].Id,
            TokoId = toko[2].Id,
            Tanggal = tanggalHariIni,
            WaktuAbsensiAwal = "07:55:08",
            WaktuCatat = "08:21:16",
            Latitude = -7.3340,
            Longitude = 112.7900,
            JarakMeter = 5,
            SkorWajah = 0.93,
            TujuanKunjungan = "Penawaran produk baru",
            Catatan = "Trial display 2 minggu.",
            Status = nameof(StatusKunjungan.Tersimpan)
        });

        await db.InsertAsync(new AbsensiKunjungan
        {
            SalesId = sales[1].Id,
            TokoId = toko[3].Id,
            Tanggal = tanggalHariIni,
            WaktuAbsensiAwal = "07:55:08",
            WaktuCatat = "11:10:24",
            Latitude = -7.2580,
            Longitude = 112.7280,
            JarakMeter = 7,
            SkorWajah = 0.91,
            TujuanKunjungan = "Kunjungan rutin",
            Catatan = "Cek harga kompetitor.",
            Status = nameof(StatusKunjungan.Tersimpan)
        });

        await db.InsertAsync(new AbsensiKunjungan
        {
            SalesId = sales[2].Id,
            TokoId = toko[4].Id,
            Tanggal = tanggalHariIni,
            WaktuAbsensiAwal = "08:02:45",
            WaktuCatat = "09:12:03",
            Latitude = -7.3300,
            Longitude = 112.8000,
            JarakMeter = 180,
            SkorWajah = 0.4,
            TujuanKunjungan = "Penawaran & pemesanan produk",
            Catatan = "",
            Status = nameof(StatusKunjungan.GagalValidasi)
        });

        await db.InsertAsync(new AbsensiKunjungan
        {
            SalesId = andi.Id,
            TokoId = toko[5].Id,
            Tanggal = tanggalHariIni,
            WaktuAbsensiAwal = "08:00:12",
            WaktuCatat = "14:25:00",
            JarakMeter = 0,
            SkorWajah = 0,
            TujuanKunjungan = "",
            Catatan = "",
            Status = nameof(StatusKunjungan.Draf)
        });

        await db.InsertAsync(new Permohonan
        {
            SalesId = sales[3].Id,
            Jenis = "Izin Sakit",
            TanggalMulai = tanggalHariIni,
            TanggalSelesai = tanggalHariIni,
            Alasan = "Demam, disertai surat dokter.",
            Lampiran = "surat_dokter.jpg",
            DiajukanPada = tanggalHariIni + " 07:30",
            Status = nameof(StatusPermohonan.Pending)
        });

        await db.InsertAsync(new Permohonan
        {
            SalesId = sales[4].Id,
            Jenis = "Cuti",
            TanggalMulai = tanggalHariIni,
            TanggalSelesai = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"),
            Alasan = "Acara keluarga di luar kota.",
            Lampiran = "",
            DiajukanPada = tanggalHariIni + " 08:15",
            Status = nameof(StatusPermohonan.Pending)
        });

        await db.InsertAsync(new Permohonan
        {
            SalesId = sales[5].Id,
            Jenis = "Cuti",
            TanggalMulai = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"),
            TanggalSelesai = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"),
            Alasan = "Keperluan administrasi.",
            Lampiran = "",
            DiajukanPada = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd") + " 09:00",
            Status = nameof(StatusPermohonan.Disetujui),
            CatatanHr = "Disetujui, pastikan target kunjungan tercapai sebelum cuti."
        });
    }

    /// <summary>
    /// Data permohonan contoh untuk akun demo andi.pratama agar halaman
    /// Status Permohonan tidak kosong. Berjalan idempoten, termasuk untuk
    /// file database yang dibuat sebelum seed ini ditambahkan.
    /// </summary>
    private static async Task SeedPermohonanSalesUtamaAsync(SQLiteAsyncConnection db)
    {
        var sales = await db.Table<Sales>()
            .Where(s => s.Username == "andi.pratama")
            .FirstOrDefaultAsync();
        sales ??= await db.Table<Sales>().FirstOrDefaultAsync();
        if (sales is null)
            return;

        if (await db.Table<Permohonan>().Where(p => p.SalesId == sales.Id).CountAsync() > 0)
            return;

        var hariIni = DateTime.Now.ToString("yyyy-MM-dd");

        await db.InsertAsync(new Permohonan
        {
            SalesId = sales.Id,
            Jenis = "Cuti",
            TanggalMulai = DateTime.Now.AddDays(5).ToString("yyyy-MM-dd"),
            TanggalSelesai = DateTime.Now.AddDays(6).ToString("yyyy-MM-dd"),
            Alasan = "Acara keluarga di luar kota.",
            Lampiran = "",
            DiajukanPada = hariIni + " 09:20",
            Status = nameof(StatusPermohonan.Pending)
        });

        await db.InsertAsync(new Permohonan
        {
            SalesId = sales.Id,
            Jenis = "Izin Sakit",
            TanggalMulai = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"),
            TanggalSelesai = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd"),
            Alasan = "Kontrol rutin ke dokter.",
            Lampiran = "surat_kontrol.jpg",
            DiajukanPada = DateTime.Now.AddDays(-4).ToString("yyyy-MM-dd") + " 07:45",
            Status = nameof(StatusPermohonan.Disetujui),
            CatatanHr = "Disetujui. Pastikan jadwal kunjungan minggu ini terpenuhi."
        });

        await db.InsertAsync(new Permohonan
        {
            SalesId = sales.Id,
            Jenis = "Cuti",
            TanggalMulai = DateTime.Now.AddDays(1).ToString("yyyy-MM-dd"),
            TanggalSelesai = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd"),
            Alasan = "Keperluan pribadi mendadak.",
            Lampiran = "",
            DiajukanPada = hariIni + " 16:05",
            Status = nameof(StatusPermohonan.Ditolak),
            CatatanHr = "Ditolak, jadwal kunjungan padat dan belum ada pengganti."
        });
    }
}
