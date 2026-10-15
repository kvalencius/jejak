using AbsensiSales.Core.Data;
using AbsensiSales.Core.Models;
using SQLite;

namespace AbsensiSales.Core.Repositories;

public class PermohonanRepository
{
    private readonly AppDatabase _db;

    public PermohonanRepository(AppDatabase db) => _db = db;

    public async Task<int> SaveAsync(Permohonan permohonan)
    {
        var conn = await _db.GetConnectionAsync();
        if (permohonan.Id == 0)
            await conn.InsertAsync(permohonan);
        else
            await conn.UpdateAsync(permohonan);
        return permohonan.Id;
    }

    public async Task<List<Permohonan>> GetBySalesAsync(int salesId)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<Permohonan>()
            .Where(p => p.SalesId == salesId)
            .OrderByDescending(p => p.DiajukanPada)
            .ToListAsync();
    }

    public async Task<List<Permohonan>> GetAllAsync()
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<Permohonan>()
            .OrderByDescending(p => p.DiajukanPada)
            .ToListAsync();
    }

    public async Task<List<Permohonan>> GetPendingAsync()
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<Permohonan>()
            .Where(p => p.Status == nameof(StatusPermohonan.Pending))
            .OrderByDescending(p => p.DiajukanPada)
            .ToListAsync();
    }
}
