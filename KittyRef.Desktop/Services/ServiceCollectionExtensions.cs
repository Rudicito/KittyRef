using KittyRef.Desktop.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KittyRef.Desktop.Services;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
    }
}
