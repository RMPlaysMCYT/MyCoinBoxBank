using CommunityToolkit.Mvvm.ComponentModel;
using MyCoinBank.ViewModels;
using MyCoinBoxBank.Services;

namespace MyCoinBoxBank.ViewModels;

public partial class NavigationBarViewModel : ObservableObject
{
    public HomeViewModel HomeVMDL {get;}
    public InsertCoinViewModel InCoinVM {get;}
    public NavigationBarViewModel(IDatabaseService db, IESP32Service esp32Service){
        HomeVMDL = new HomeViewModel(db);
    }
}