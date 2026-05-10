using System.Collections.Generic;
using System.Threading.Tasks;
using MyCoinBoxBank.Models;

namespace MyCoinBoxBank.Services;
public interface IDatabaseService
{
    Task InitializeAsync();
    Task<List<Coin>> GetCoinsAsync();
    Task <decimal> GetTotalBalanceAsync();
    Task<List<Transaction>> GetTransactionsAsync();
    Task AddTransactionAsync(Transaction transaction);
    Task UpdateCoinAsync(Coin coin);
}

