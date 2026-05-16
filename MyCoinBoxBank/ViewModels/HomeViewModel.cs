using System;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCoinBoxBank.Services;

namespace MyCoinBoxBank.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    private readonly IDatabaseService _db;
    private readonly IESP32Service _esp32;

    // Navigation callbacks — set by NavigationBarViewModel
    public Action? OnInsertCoinRequested { get; set; }
    public Action? OnWithdrawRequested   { get; set; }

    [ObservableProperty] private string _balanceDisplay = "Loading...";
    [ObservableProperty] private string _deviceStatus   = "Checking device...";
    [ObservableProperty] private bool   _isDeviceOnline = false;

    public HomeViewModel(IDatabaseService db, IESP32Service esp32)
    {
        _db    = db;
        _esp32 = esp32;
        _ = LoadBalanceAsync();
        _ = CheckDeviceAsync();
    }

    public async Task LoadBalanceAsync()
    {
        try
        {
            var balance    = await _db.GetTotalBalanceAsync();
            BalanceDisplay = $"₱{balance:0.00}";
        }
        catch { BalanceDisplay = "No data yet"; }
    }

    private async Task CheckDeviceAsync()
    {
        var online     = await _esp32.PingAsync();
        IsDeviceOnline = online;
        DeviceStatus   = online ? "✅ Device Connected" : "❌ Device Not Connected";
    }

    [RelayCommand]
    public void OpenInsertCoin() => OnInsertCoinRequested?.Invoke();

    [RelayCommand]
    public void OpenWithdraw() => OnWithdrawRequested?.Invoke();

    [RelayCommand]
    public async Task RefreshBalanceAsync()
    {
        await LoadBalanceAsync();
        await CheckDeviceAsync();
    }
}