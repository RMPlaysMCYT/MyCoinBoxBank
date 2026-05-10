using System;
using System.IO;
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
    }
}

