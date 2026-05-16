using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using SQLite;
using MyCoinBoxBank.Models;

namespace MyCoinBoxBank.Services;
public class DatabaseService : IDatabaseService
{
    private SQLiteAsyncConnection? _db;
    private readonly string _dbPath;

    public DatabaseService()
    {
        _dbPath = Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData), 
            "MyCoinBoxBank.db"); 
    }

    public async Task InitializeAsync()
    {
        if (_db is not null) return;
        _db = new SQLiteAsyncConnection(_dbPath);
        await _db.CreateTableAsync<Coin>();
        await _db.CreateTableAsync<Transaction>();
        
        var coins = await _db.Table<Coin>().ToListAsync();
        if (coins.Count == 0)
        {
            await _db.InsertAllAsync(new List<Coin>
            {
                new Coin { Denomination = "1.00", Value = 1.00m, Count = 0 },
                new Coin { Denomination = "5.00", Value = 5.00m, Count = 0 },
                new Coin { Denomination = "10.00", Value = 10.00m, Count = 0 },
                new Coin { Denomination = "20.00", Value = 20.00m, Count = 0 },
            });
        }
    }

    public async Task<List<Coin>> GetCoinsAsync()
    {
        await EnsureInit();
        return await _db!.Table<Coin>().ToListAsync();
    }
    
    public async Task<decimal> GetTotalBalanceAsync()
    {
        var coins = await GetCoinsAsync();
        return coins.Sum(c => c.Value * c.Count);
    }

    public async Task<List<Transaction>> GetTransactionsAsync()
    {
        await EnsureInit();
        return await _db!.Table<Transaction>()
            .OrderByDescending(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task AddTransactionAsync(Transaction transaction)
    {
        await EnsureInit();
        await _db!.InsertAsync(transaction);
    }

    public async Task UpdateCoinAsync(Coin coin)
    {
        await EnsureInit();
        coin.LastUpdated = DateTime.Now;
        await _db!.UpdateAsync(coin);
    }

    private async Task EnsureInit()
    {
        if (_db is null) await InitializeAsync();
    }
}

