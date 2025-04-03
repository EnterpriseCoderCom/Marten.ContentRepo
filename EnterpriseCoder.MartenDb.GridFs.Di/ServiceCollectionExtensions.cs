using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.MartenDb.GridFs.Di;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMartenDbGridFs(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IGridFileSystem, GridFileSystem>();
        serviceCollection.AddScoped<IGridFileSystemScoped, GridFileSystemScoped>();
        
        return serviceCollection;
    }
}