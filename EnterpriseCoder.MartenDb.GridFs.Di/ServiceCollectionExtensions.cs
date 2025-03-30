using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.MartenDb.GridFs.Di;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMartenDbGridFs(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddScoped<IGridFileSystem, GridFileSystem>();
        
        return serviceCollection;
    }
}