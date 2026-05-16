using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCoinBoxBank.Services;

namespace MyCoinBoxBank.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IDatabaseService _db;

    [ObservableProperty] private string _balanceDisplay = "Loading..";
    [ObservableProperty] private string _deviceStatus = "Not Connected.";
    [ObservableProperty] private bool _isDeviceOnline = false;

    public HomeViewModel(IDatabaseService db)
    {
        _db = db;
        _ = LoadBalanceAsyncc();
    }

    private async Task LoadBalanceAsyncc()
    {
        try
        {
            var balance = await _db.GetTotalBalanceAsync();
            BalanceDisplay = $"${balance:0.00}";
        }
        catch
        {
            BalanceDisplay = "No Data Found";
        }
    }

    [RelayCommand]
    public async Task RefreshBalanceAsync()
    {
        await LoadBalanceAsyncc();
    }
}

