﻿
# EnterpriseCoder.Marten.ContentRepo

## Overview

**EnterpriseCoder.Marten.ContentRepo** is a C#, .NET 6+ library that can be used to 
create a content repository within a PostgreSQL database.  
This library uses MartenDb and requires a Marten **IDocumentSession** instance to do 
its work against the database.  

### EnterpriseCoder.Marten.ContentRepo
* Bucket + ResourcePath/Prefix system similar to Amazon S3
* Requires PostgreSQL + MartenDb
* Implemented as three tables
  * ContentBucket - mt_doc_contentbucket
  * ContentResourceHeader - mt_doc_contentresourceheader
  * ContentResourceBlock - mt_doc_contentresourceblock
* MartenDb will automatically create the required schema if the AutoCreateSchemaObjects options are set to at least "AutoCreate.CreateOnly".
* All ContentRepo code uses async/await.
* Transaction control is left to the caller of this library whenever possible, making operations against the content repo reside in the same transactions as your application database changes.
* The IContentRepository/IContentRepositoryScope interfaces are designed for paging of information to prevent over-allocation of memory.
* **EnterpriseCoder.Marten.ContentRepo.Di** library adds a "scoped" ContentRepositoryScoped class that works with dependency injection.
* Where possible, compiled queries have been used to improve performance.

### EnterpriseCoder.Marten.ContentRepo.Di
* Provides a ContentRepositoryScoped class that allows for "scoped" Marten IDocumentSessions using Microsoft Dependency Injection.

### EnterpriseCoder.Marten.ContentRepo.AspNet
* Map content repository locations to server URLs in an ASP.NET server.

## Getting Started

ContentRepository is the core class and can be instantiated directly as shown in the example below.  Note that this
class contains no internal state information, so creating more than one copy is not an issue. 

### EnterpriseCoder.Marten.ContentRepo
```csharp
using EnterpriseCoder.Marten.ContentRepo;

IContentRepository repo = new ContentRepository();

// The core ContentRepository class requires that you provide an 
// IDocumentSession from Marten.
IDocumentSession documentSession;
await repo.CreateBucketAsync(documentSession, "myBucket");
```
### EnterpriseCoder.Marten.ContentRepo.Di
If you prefer to use dependency injection, use the ContentRepositoryScoped class found in the
EnterpriseCoder.Marten.ContentRepo.Di library.

```csharp
// Configure ContentRepo for use with Dependency Injection.  This registers a singleton
// IContentRepository and scoped instances for IContentRepositoryScoped.
services.AddMartenContentRepo();

// IContentRepositoryScoped is the interface that can now be injected.
public class MyService : IMyService
{
    private readonly IContentRepositoryScoped _repo;
    
    // Using constructor injection or method injection for ASP.NET minimal APIs
    public MyService( IContentRepositoryScoped repo ) 
    {
        _repo = repo;
    }
}
```
### EnterpriseCoder.Marten.ContentRepo.AspNet
```csharp
// Add the content repository to DI
builder.Services.AddMartenContentRepo();

// Set up URL to ContentRepo path mappings.
builder.Services.MapContentRepository(config =>
{
    config
        .AddMapping(
            uriPathPrefix: "/images", 
            bucketName: "images", contentPathPrefix: "/userImages")
        .AddMapping(
            uriPathPrefix: "/documents", 
            bucketName: "documents", contentPathPrefix: "/userDocuments");
});

// After your application is built...
// Insert the middleware to handle content repository requests
app.UseContentRepository();

```

## Paths
Content resources are mapped using addressing similar to an Amazon S3
bucket.  Each resource is stored and retrieved using...

* A bucket name (e.g. "default" or "userImageResources")
* A virtual path (e.g. /images/userImage.png)

"ContentRepo" will, using Marten, automatically create the tables that
it needs to support storing resources into the database.  

The "ContentRepo" project creates 2 libraries:
1. EnterpriseCoder.Marten.ContentRepo - The core ContentRepository class and supporting code.
2. EnterpriseCoder.Marten.ContentRepo.Di - Support for Microsoft Dependency Injection.

## Functionality

The core interface (**IContentRepository**) has the following methods that can be 
used for manipulating content.

### Buckets
* CreateBucketAsync - Create a new bucket.
* DeleteBucketAsync - Destroy a bucket, and optionally, its contents.
* BucketExistsAsync - Check to see if a bucket with a given name already exists.
* ListBucketsAsync - Obtain a listing of all available buckets (paged)

```csharp
IDocumentSession documentSession;
IContentRespoitoryScoped repo;

// We could just create the bucket.  CreateBucketAsync completes without error
// if the bucket already exists.
if( await repo.BucketExistsAsych("myBucket") is false ) 
{
    await repo.CreateBucketAsych("myBucket");
    
    // Don't forget to save your changes!  Either...
    await documentSession.SaveChangesAsync();
    
    // Or like this...
    await repo.DocumentSession.SaveChangesAsync();
}

// List the names of all buckets (paged)
int itemsPerPage = 10;
PagedBucketNameListing pageListing = repo.ListBucketsAsync(1, itemsPerPage);

for( int pageNumber = 1 ; pageNumber <= pageListing.PageCount ; pageNumber++ ) 
{
    if( pageNumber > 1 ) 
    {
        pageListing = repo.ListBucketsAsync(pageNumber, itemsPerPage);
    }
    
    foreach( string nextBucketName in pageListing ) 
    {
        Console.WriteLine($"Bucket: {nextBucketName}");
    }
}

// Destroy a bucket, along with all of it's contents.
await repo.DeleteBucketAsync("myBucket", force: true);
await documentSession.SaveChangesAsync();
```

### Content
* UploadStreamAsync - Upload the contents of a C# System.Io.Stream into the repository.
* DownloadStreamAsync - Read a previously uploaded resource from the database as a System.Io.Stream.
* ResourceExistsAsync - Determine if a resource exists in the database.
* DeleteResourceAsync - Delete a resource from the database.
* GetResourceInfoAsync - Get information about an uploaded resource (resource size, creation time, etc)
* RenameResourceAsync - Rename/Move a resource, even between buckets.
* CopyResourceAsync - Make a copy of a resource.

```csharp
IDocumentSession documentSession;
IContentRespoitoryScoped repo;

using FileStream myFileString = File.OpenRead("MyImage.png");

// Upload a file into the repository...will fail if the resource already exists, 
// or there's no such bucket.
await repo.UploadStreamAsync("myBucket", "/images/MyImage.png", myFileStream);

// Upload a file into the repository...overwrite existing and auto-create the bucket.
await repo.UploadStreamAsync("myBucket", "/images/MyImage.png", myFileStream, 
    autoCreateBucket: true, overwriteExisting: true);

// Save your changes through the documentSession, or you can get the 
// session through the DocumentSession property of the repo.
await repo.DocumentSession.SaveChangesAsync();
// or...
await documentSession.SaveChangesAsync();

// Download a document from the repository
using readStream = await repo.DownloadStreamAsync("myBucket", "/images/MyImage.png");
httpResponse.ContentType = "image/png";
readStream.CopyTo(httpResponse.OutputStream);
```

## Content Listing
* GetResourceListingAsync - Get a paged listing of resources based on a bucket and prefix mask.
* GetResourceListingByUserDataGuidAsync - Get a paged listing of resources based on a user-defined guid.
* GetResourceListingByUserDataLongAsync - Get a paged listing of resources based on a user-defined long value.

While "ContentRepo" requires that content paths be in the form of a
path, the path is only a key to look up the resource.

Resource listings can be obtained by using a "path prefix."  For example, 
to get all resources that start with "/images":

```csharp
IDocumentSession documentSession;
IContentRespoitory repo = new ContentRepository();

int itemsPerPage = 50;

// Get page 1's information.  This also gives us the page range information.
var pagedResourceListing = repo.GetResourceListingAsync(
    documentSession,
    "myBucket",
    "/images",
    1, itemsPerPage, 
    true); // recursive

// Loop through the pages
for( var nextPageNumber = 1 ; nextPageNumber <= pagedResourceListing.PageCount ; i++ )
{
    // Since we just got page 1, we don't fetch it again...
    if( nextPageNumber > 1 )
    {
        // Move to the next page of results.
        pagedResourceListing = repo.GetResourceListingAsync(
            documentSession,
            "myBucket",
            "/images",
            nextPageNumber, itemsPerPage,
            true); // recursive        
    }
    
    foreach( var nextItem in pagedResourceListing )
    {
        // Process the items in this page of results
    }
}

```

