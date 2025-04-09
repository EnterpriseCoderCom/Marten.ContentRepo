using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.Marten.ContentRepo.Di;

public static class ContentRepositoryServiceCollectionExtensions
{
    public static IServiceCollection AddMartenContentRepo(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IContentRepository, ContentRepository>();
        serviceCollection.AddScoped<IContentRepositoryScoped, ContentRepositoryScoped>();

        return serviceCollection;
    }
}