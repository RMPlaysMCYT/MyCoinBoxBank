using CommunityToolkit.Mvvm.ComponentModel;

namespace MyCoinBoxBank.ViewModels;

public partial class NavigationBarViewModel : ObservableObject
{
    public HomeViewModel HomeVMDL {get;}
    public InsertCoinViewModel InCoinVM {get;}
    public NavigationBarViewModel(IDatabaseService db, IESP32Service Esp32Service){
        HomeVMDL = new HomeViewModel(db);
        InCoinVM = new InsertCoinViewModel(db, Esp32Service)
    }
}