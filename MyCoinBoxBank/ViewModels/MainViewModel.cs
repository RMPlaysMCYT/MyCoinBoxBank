using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Threading;

namespace MyCoinBoxBank.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private object _currentView;

    private readonly IDatabaseService _db;
    private readonly IESP32Service _esp32Service;

    public MainViewModel()
    {
        _db = db;
        _esp32Service = Esp32Service;
        CurrentView = new SplashViewModel();
        InitializeAppSync();
    }

    private async Task InitializeAppSync()
    {
        await _db.InitializeAsync();
        await Task.Delay(3000);
        Dispatcher.UIThread.Post(() =>
            {
                CurrentView = new NavigationBarViewModel();
            }
        );
    }
}