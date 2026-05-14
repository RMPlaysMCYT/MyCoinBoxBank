using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using MyCoinBoxBank.Models;
using MyCoinBoxBank.Services;
using MyCoinBoxBank.ViewModels;

namespace MyCoinBank.ViewModels;

public partial class InsertCoinViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly Esp32Service _esp32Service;

    [ObservableProperty] private string _statusMessage = "Raedy";
    [ObservableProperty] private bool _isBusy = "False";
    [ObservableProperty] private decimal _selectedAmount = 1.00m;

    public InsertCoinViewModel(IDatabaseService db, Esp32Service esp32Service)
    {
        _db = db;
        _esp32Service = esp32Service;
    }

    public async Task InsertCoins()
    {
        IsBusy = true;
        StatusMessage = "Connecting to the server";

        var triggered = await esp32Service.TriggerInsertAsync(); 
        if (triggered){
            await _db.AddTransactionAsync(new Transaction
            {
                Type = TransactionType.Insert,
                Amount = SelectedAmount,
                Notes = "Inserted via App"

            });
            StatusMessage = $"Inserted ${SelectedAmount:0.00} successfully";
        } else {
            await _db.AddTransactionAsync(new Transaction
            {
                Type = TransactionType.Insert,
                Amount = SelectedAmount,
                Notes = "Offline Insert - ESP32 not Reached"

            });
            StatusMessage = "ESP32 offline, Logged Locally";
        }
        IsBusy = false;
    }
}

