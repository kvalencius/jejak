using AbsensiSales.Core.Data;
using AbsensiSales.Core.Models;
using SQLite;

namespace AbsensiSales.Core.Repositories;

public class KunjunganRepository
{
    private readonly AppDatabase _db;

    public KunjunganRepository(AppDatabase db) => _db = db;

    public async Task<int> SaveAsync(AbsensiKunjungan kunjungan)
    {
        var conn = await _db.GetConnectionAsync();
        if (kunjungan.Id == 0)
            await conn.InsertAsync(kunjungan);
        else
            await conn.UpdateAsync(kunjungan);
        return kunjungan.Id;
    }

    public async Task<List<AbsensiKunjungan>> GetBySalesAsync(int salesId, string? tanggal = null)
    {
        var conn = await _db.GetConnectionAsync();
        var query = conn.Table<AbsensiKunjungan>().Where(k => k.SalesId == salesId);
        if (!string.IsNullOrEmpty(tanggal))
            query = query.Where(k => k.Tanggal == tanggal);
        return await query.OrderByDescending(k => k.WaktuCatat).ToListAsync();
    }

    public async Task<List<AbsensiKunjungan>> GetAllAsync(string? tanggal = null)
    {
        var conn = await _db.GetConnectionAsync();
        var query = conn.Table<AbsensiKunjungan>();
        if (!string.IsNullOrEmpty(tanggal))
            query = query.Where(k => k.Tanggal == tanggal);
        return await query.ToListAsync();
    }

    public async Task<AbsensiKunjungan?> GetByIdAsync(int id)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<AbsensiKunjungan>().Where(k => k.Id == id).FirstOrDefaultAsync();
    }

    public async Task<int> HitungKunjunganHariIniAsync(int salesId, string tanggal)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<AbsensiKunjungan>()
            .CountAsync(k => k.SalesId == salesId
                          && k.Tanggal == tanggal
                          && k.Status == nameof(StatusKunjungan.Tersimpan));
    }
}
