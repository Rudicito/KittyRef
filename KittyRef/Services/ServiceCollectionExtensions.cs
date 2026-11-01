using KittyRef.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace KittyRef.Services;

public static class ServiceCollectionExtensions
{
    public static void AddCommonServices(this IServiceCollection collection)
    {
        collection.AddTransient<MainWindowViewModel>();
    }
}