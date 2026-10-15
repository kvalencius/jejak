using AbsensiSales.Core.Data;
using AbsensiSales.Core.Models;
using SQLite;

namespace AbsensiSales.Core.Repositories;

public class SalesRepository
{
    private readonly AppDatabase _db;

    public SalesRepository(AppDatabase db) => _db = db;

    public async Task<Sales?> LoginAsync(string username, string password)
    {
        var conn = await _db.GetConnectionAsync();
        var user = await conn.Table<Sales>()
            .Where(s => s.Username == username && s.Password == password)
            .FirstOrDefaultAsync();
        return user;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<Sales>().CountAsync(s => s.Username == username) > 0;
    }

    public async Task RegisterAsync(Sales sales)
    {
        var conn = await _db.GetConnectionAsync();
        await conn.InsertAsync(sales);
    }

    public async Task<List<Sales>> GetAllAsync()
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<Sales>().OrderBy(s => s.Nama).ToListAsync();
    }

    public async Task<Sales?> GetByIdAsync(int id)
    {
        var conn = await _db.GetConnectionAsync();
        return await conn.Table<Sales>().Where(s => s.Id == id).FirstOrDefaultAsync();
    }
}
