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
        _dbPath = Path.Combine( Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "MyCoinBoxBank.db"); 
    }

    public async Task InitializeAsync()
    {
        if (_db is not null) return;
        _db = new SQLiteAsyncConnection(_dbPath);
        await _db.CreateTableAsync<Transaction>();
        await _db.CreateTableAsync<Coin>();
        
        var coins = await _db.Table<Coin>().ToListAsync();
        if (coins.Count == 0)
        {
            await _db.InsertAllAsync(new List<Coin>
            {
                new Coin { Denomination = "1.00", Value = 1.00m, Count = 0 },
                new Coin { Denomination = "0.25", Value = 0.25m, Count = 0 },
                new Coin { Denomination = "0.10", Value = 0.10m, Count = 0 },
                new Coin { Denomination = "0.05", Value = 0.05m, Count = 0 },
            });
        }
    }

    public Task<List<Coin>> GetCoinsAsync()
    {
        await EnsureInit();
        return await _db.Table<Coin>().ToListAsync();
    }
    
    public async Task<List<decimal>> GetTotalBalanceAsync()
    {
        var coins = await GetCoinsAsync();
        return coins.Sum(c => c.Value * c.Count);
    }

    public Task<List<Transaction>> GetTransactionsAsync()
    {
        throw new NotImplementedException();
    }

    public Task AddTransactionAsync(Transaction transaction)
    {
        throw new NotImplementedException();
    }

    public Task UpdateCoinAsync(Coin coin)
    {
        throw new NotImplementedException();
    }

    private async Task EnsureInit()
    {
        if (_db is null) await InitializeAsync();
    }
}

