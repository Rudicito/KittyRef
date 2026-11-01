using CommunityToolkit.Mvvm.ComponentModel;

namespace KittyRef.Desktop.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    public partial ObservableObject? CurrentPage { get; set; }

    public void ChangePage(ObservableObject newPage)
    {
        CurrentPage = newPage;
    }
}
