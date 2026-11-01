using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using KittyRef.Desktop.Services;
using Microsoft.Extensions.DependencyInjection;
using MainWindow = KittyRef.Desktop.Views.MainWindow;
using MainWindowViewModel = KittyRef.Desktop.ViewModels.MainWindowViewModel;

namespace KittyRef.Desktop;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Register all the services needed for the application to run
        var collection = new ServiceCollection();
        collection.AddCommonServices();

        // Creates a ServiceProvider containing services from the provided IServiceCollection
        var services = collection.BuildServiceProvider();

        var vm = services.GetRequiredService<MainWindowViewModel>();
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = vm
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
