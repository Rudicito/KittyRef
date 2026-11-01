using Microsoft.Extensions.DependencyInjection;
using MainWindowViewModel = KittyRef.Desktop.ViewModels.MainWindowViewModel;

namespace KittyRef.Desktop.Services;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
    }
}
