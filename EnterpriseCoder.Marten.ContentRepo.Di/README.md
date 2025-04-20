
# EnterpriseCoder.Marten.ContentRepo

## Overview

**EnterpriseCoder.Marten.ContentRepo** can be used to create a content repository within
a PostgreSQL database.  This library uses MartenDb and requires a
Marten **IDocumentSession** instance to do its work against the
database.

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

## Getting Started

ContentRepository is the core class and can be instantiated directly as shown in the example below.  Note that this
class contains no internal state information, so creating more than one copy is not an issue.

```csharp
using EnterpriseCoder.Marten.ContentRepo;

IContentRepository repo = new ContentRepository();

// The core ContentRepository class requires that you provide an 
// IDocumentSession from Marten.
IDocumentSession documentSession;
await repo.CreateBucketAsync(documentSession, "myBucket");

```
If you prefer to use dependency injection, use the ContentRepositoryScoped class found in the
EnterpriseCoder.Marten.ContentRepo.Di library.

```csharp
// Configure ContentRepo for use with Dependency Injection
services.AddMartenContentRepo();

// IContentRepositoryScoped is the interface that can now be injected.
public async Task MyMethod( IContentRepositoryScoped repo ) 
{
    // Note that using DI, the IDocumentSession will automatically be
    // injected into the ContentRepositoryScoped instance.
    await repo.CreateBucketAsync("myBucket");
}        
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

### Content
* UploadStreamAsync - Upload the contents of a C# System.Io.Stream into the repository.
* DownloadStreamAsync - Read a previously uploaded resource from the database as a System.Io.Stream.
* ResourceExistsAsync - Determine if a resource exists in the database.
* DeleteResourceAsync - Delete a resource from the database.
* GetResourceInfoAsync - Get information about an uploaded resource (resource size, creation time, etc)
* RenameResourceAsync - Rename/Move a resource, even between buckets.
* CopyResourceAsync - Make a copy of a resource.

## Content Listing
* GetResourceListingAsync - Get a paged listing of resources based on a bucket and prefix mask.
* GetResourceListingByUserDataGuidAsync - Get a paged listing of resources based on a user-defined guid.
* GetResourceListingByUserDataLongAsync - Get a paged listing of resources based on a user-defined long value.

While "ContentRepo" requires that content paths be in the form of a
path, the path is only a key to look up the resource.

Resource listings can be obtained by using a "path prefix".  For example,
to get all resources that start with "/images":

```csharp
IDocumentSession documentSession;

IContentRespoitory repo = new ContentRepository();
var pagedResourceListing = repo.GetResourceListingAsync(
    documentSession,
    "myBucket",
    "/images",
    1, 50, // page 1, 50 items per page.
    true); // recursive
```

