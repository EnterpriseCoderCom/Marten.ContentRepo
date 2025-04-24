using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.Marten.ContentRepo.AspNet;

public static class ContentRepositoryExtensionMethods
{
    public static IServiceCollection MapContentRepository(this IServiceCollection services,
        Action<ContentRepositoryPathMapBuilder> configureMapping)

    {
        // Call the configureMapping delegate to build mappings of Uri's to Content Repo bucket/path pairs.
        var contentMapBuilder = new ContentRepositoryPathMapBuilder();
        configureMapping(contentMapBuilder);
        
        // Add the mapping configuration as a DI singleton.
        services.AddSingleton(contentMapBuilder.Build());

        return services;
    }


    public static IApplicationBuilder UseContentRepository(this IApplicationBuilder app)
    {
        // Get the ContentRepositoryPathMap from DI.
        var pathMap = app.ApplicationServices.GetService<ContentRepositoryPathMap>();
        if (pathMap == null)
        {
            throw new InvalidOperationException(
                "Content Repository mapping not found.  Use 'MapContentRepository' to configure routing of URLs to Content.");
        }
        app.UseMiddleware<ContentRepositoryMiddleware>();

        return app;
    }
}