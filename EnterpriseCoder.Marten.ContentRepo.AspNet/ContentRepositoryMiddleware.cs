using EnterpriseCoder.Marten.ContentRepo.Di;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.StaticFiles;

namespace EnterpriseCoder.Marten.ContentRepo.AspNet;

public class ContentRepositoryMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ContentRepositoryPathMap _pathMap;
    
    private readonly FileExtensionContentTypeProvider _fileExtensionContentTypeProvider = new();

    public ContentRepositoryMiddleware(RequestDelegate next, ContentRepositoryPathMap pathMap)
    {
        _next = next;
        _pathMap = pathMap;
    }

    public async Task InvokeAsync(HttpContext context, IContentRepositoryScoped contentRepositoryScoped)
    {
        foreach (var nextPathMap in _pathMap)
        {
            // Check if the request path starts with our prefix path
            if (context.Request.Path.StartsWithSegments(nextPathMap.UriPathPrefix,
                    StringComparison.OrdinalIgnoreCase))
            {
                // Extract the relative path
                string relativePath = context.Request.Path.Value.Substring(nextPathMap.UriPathPrefix.Length);

                // Add in the path prefix from the mapping
                string fullPath = nextPathMap.ContentPathPrefix + relativePath;
                
                // Fetch the content from the repository
                ContentRepositoryResourceInfo? resourceInfo =
                    await contentRepositoryScoped.GetResourceInfoAsync(nextPathMap.BucketName, fullPath);
                
                if (resourceInfo == null)
                {
                    // If the resource was not found, return 404 - Not Found
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }

                // Check for If-None-Match header.
                string resourceETag = Convert.ToHexString(resourceInfo.Sha256);
                var ifNoneMatch = context.Request.Headers["If-None-Match"];
                if (ifNoneMatch == resourceETag)
                {
                    context.Response.StatusCode = StatusCodes.Status304NotModified;
                    context.Response.ContentLength = 0;
                    return;
                }
                
                // Get the extension for the resource
                string extension = resourceInfo.ResourcePath.FileExtension;

                // Get the content type for the given extension and set the content type header.
                if (_fileExtensionContentTypeProvider.TryGetContentType(extension, out string? contentType))
                {
                    context.Response.ContentType = contentType;
                }
                else
                {
                    context.Response.ContentType = "application/octet-stream";
                }

                // Get a stream for the resource.
                using var resourceStream =
                    await contentRepositoryScoped.DownloadStreamAsync(nextPathMap.BucketName, fullPath);
                if (resourceStream == null)
                {
                    // If the stream is null, return 404 - Not Found
                    context.Response.StatusCode = StatusCodes.Status404NotFound;
                    return;
                }
                
                // Set the etag for the returned resource.
                context.Response.Headers["ETag"] = resourceETag;
                
                // Copy the stream to the response.
                await resourceStream.CopyToAsync(context.Response.Body);

                return;
            }
        }

        // Path didn't match.  Allow the next middleware to run.
        await _next(context);
    }
}