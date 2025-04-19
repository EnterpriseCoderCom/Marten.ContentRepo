
# EnterpriseCoder.Marten.ContentRepo

## Overview

This library can be used to create a content repository within a 
PostgreSQL database.  This library uses MartenDb and requires a
Marten **IDocumentSession** instance to do it's work against the
PostgreSQL database.  All methods related to the core ContentRepository
class are implemented using Async/Await.

Content resources are mapped using addressing similar to an Amazon S3
bucket.  Each resource is stored and retrieved using...

* A bucket name (e.g. "default" or "userImageResources")
* A virtual path (e.g. /images/userImage.png)

"ContentRepo" will, using Marten, automatically create the tables that
it needs in order to support storing files into the database.  

The "ContentRepo" project creates 2 libraries:
1. EnterpriseCoder.Marten.ContentRepo - The core ContentRepository class and supporting code.
2. EnterpriseCoder.Marten.ContentRepo.Di - Support for Microsoft Dependency Injection.

## Getting Started

ContentRepository is the core class and can be instantiated directly:

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
## Functionality

The core interface (**IContentRepository**) has the following methods that can be 
used for manipulating content.

### Buckets
* CreateBucketAsync - Create a new bucket.
* DeleteBucketAsync - Destroy a bucket, and optionally, it's contents.

### Content
* UploadStreamAsync - Upload the contents of a C# Stream into the repository.
* DownloadStreamAsync - Obtain a C# Stream in order to read content from the repository.





While "ContentRepo" requires that content paths be in the form of a
path, the path is only a key to look up the resource.


Resource listings can be obtained by using a "path prefix".  For example
to obtain all resources that start with "/images":

```csharp
IDocumentSession documentSession;

IContentRespoitory repo = new ContentRepository();
var pagedFileListing = repo.GetFileListingAsync(
    documentSession,
    "myBucket",
    "/images",
    1, 50, // page 1, 50 items per page.
    true); // recursive
```


# TODO

* [ ] Write Summary documentation
* [ ] Need a method to lookup by user data long.
* [ ] BucketExistsAsync - this removes the need to commit prematurely
* [ ] ListBucketsAsync
* [X] Paged results need a better return type that gives page information.
* [X] More unit tests around paging.
* [X] Review exceptions that are thrown for correctness.
* [X] Command line tool for dumping contents - no doing this, requires database access
* [X] Ability to select by user guid
* [X] Make sure Copy and Move preserve user data
* [X] Pre-compiled queries
