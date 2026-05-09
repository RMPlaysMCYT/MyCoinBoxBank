using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Threading;

namespace MyCoinBoxBank.ViewModels;

public partial class MainViewModel : ObservableObject
{
    [ObservableProperty] private object _currentView;

    public MainViewModel()
    {
        CurrentView = new SplashViewModel();
        InitializeAppSync();
    }

    private async void InitializeAppSync()
    {
        await Task.Delay(3000);
        Dispatcher.UIThread.Post(() =>
            {
                CurrentView = new NavigationBarViewModel();
            }
        );
    }
}