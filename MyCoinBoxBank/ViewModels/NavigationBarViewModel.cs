using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MyCoinBoxBank.Services;
using Avalonia.Threading;

namespace MyCoinBoxBank.ViewModels;

public partial class NavigationBarViewModel : ObservableObject
{
    private readonly IESP32Service _esp32;

    public HomeViewModel HomeVMDL { get; }
    public InsertCoinViewModel InsertCoinVM { get; }

    [ObservableProperty] private int _selectedTabIndex = 0;
    [ObservableProperty] private bool _showOfflineBanner = false;
    [ObservableProperty] private string _offlineMessage = "⚠ The Machine is Offline";

    public NavigationBarViewModel(IDatabaseService db, IESP32Service esp32)
    {
        _esp32 = esp32;

        HomeVMDL = new HomeViewModel(db, esp32);
        InsertCoinVM = new InsertCoinViewModel(db, esp32);

        HomeVMDL.OnInsertCoinRequested = NavigateToInsertCoin;
        HomeVMDL.OnWithdrawRequested = NavigateToWithdraw;
    }

    public async Task NavigateToInsertCoin()
    {
        var online = await _esp32.PingAsync();
        if (!online)
        {
            await FlashOfflineBanner("⚠ The Machine is Offline");
            return;
        }

        SelectedTabIndex = 4;
    }

    public async Task NavigateToWithdraw()
    {
        var online = await _esp32.PingAsync();
        if (!online)
        {
            await FlashOfflineBanner("⚠ The Machine is Offline");
            return;
        }

        SelectedTabIndex = 5;
    }

    [RelayCommand]
    public void NavigateToHome()
    {
        SelectedTabIndex = 0;
        _ = HomeVMDL.RefreshBalanceAsync();
    }

    private async Task FlashOfflineBanner(string message)
    {
        OfflineMessage = message;
        ShowOfflineBanner = true;

        await Task.Delay(3000);

        Dispatcher.UIThread.Post(() =>
        {
            ShowOfflineBanner = false;
        });
    }
}