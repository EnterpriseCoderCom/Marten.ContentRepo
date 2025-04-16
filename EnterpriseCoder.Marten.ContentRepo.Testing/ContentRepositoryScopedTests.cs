using System.Reflection;
using System.Security.Cryptography;
using EnterpriseCoder.Marten.ContentRepo.Di;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class ContentRepositoryScopedTests : IClassFixture<DatabaseTestFixture>
{
    private const string TestFilename = "angrybird.png";
    private const string TestResourcePath = "/resources/angrybird.png";
    private const int TestBlockCount = 8;

    private readonly IContentRepositoryScoped _contentRepositoryScoped;
    private readonly DatabaseHelper _databaseHelper;
    private readonly ITestOutputHelper _output;

    public ContentRepositoryScopedTests(DatabaseTestFixture databaseFixture, ITestOutputHelper output)
    {
        _output = output;
        _contentRepositoryScoped = databaseFixture.ServiceProvider.GetRequiredService<IContentRepositoryScoped>();
        _databaseHelper = databaseFixture.ServiceProvider.GetRequiredService<DatabaseHelper>();
    }

    [Fact]
    public async Task UploadAndRetrieve()
    {
        var bucketName = "default";

        // ===================================================================================
        // Clear the database tables.
        // ===================================================================================
        await _databaseHelper.ClearDatabaseAsync();

        // ===================================================================================
        // Load the test article into the database.
        // ===================================================================================
        var resourceFilePath = _GetTestResourceFilePath(TestFilename);

        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _contentRepositoryScoped.UploadStreamAsync(bucketName, TestResourcePath, fileStream);
            await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();
        }

        Assert.True(await _contentRepositoryScoped.ResourceExistsAsync(bucketName, TestResourcePath));
        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        // ===================================================================================
        // Try to overwrite the file by saving again...should throw since we are going 
        // to leave the "overwriteExisting" argument to default to false.
        // ===================================================================================
        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await Assert.ThrowsAsync<OverwriteNotPermittedException>(async () =>
                await _contentRepositoryScoped.UploadStreamAsync(bucketName, TestResourcePath, fileStream));
            _contentRepositoryScoped.DocumentSession.EjectAllPendingChanges();
        }

        // ===================================================================================
        // Read the saved data back out of the grid file system and ensure that the SHA256's
        // match to ensure data integrity
        // ===================================================================================
        await using var readStream =
            await _contentRepositoryScoped.DownloadStreamAsync(bucketName, TestResourcePath);
        Assert.NotNull(readStream);

        // Generate a sha256 for the stream that comes out of the database.
        byte[] rereadSha256;
        using (var sha256Hash = SHA256.Create())
        {
            rereadSha256 = await sha256Hash.ComputeHashAsync(readStream!);
        }

        // Get the file information from the database and ensure that the stored 
        // SHA256 matches.
        var fileInfo =
            await _contentRepositoryScoped.GetResourceInfoAsync(bucketName, TestResourcePath);
        Assert.NotNull(fileInfo);

        // Verify that the SHA256's are the same between stored vs reloaded.
        Assert.Equal(rereadSha256, fileInfo!.Sha256);

        // Now load the original resource and get it's SHa256.
        await using (var resourceStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            byte[] originalSha;
            using (var sha256Hash = SHA256.Create())
            {
                originalSha = await sha256Hash.ComputeHashAsync(resourceStream);
            }

            Assert.Equal(rereadSha256, originalSha);
        }
    }

    [Fact]
    public async Task UploadAndDeleteFile()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        await _contentRepositoryScoped.DeleteResourceAsync(bucketName, TestResourcePath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(0, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(0, await _databaseHelper.CountBlocksAsync());

        Assert.False(await _contentRepositoryScoped.ResourceExistsAsync(bucketName, TestResourcePath));
    }

    [Fact]
    public async Task RenameFileTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        await _contentRepositoryScoped.RenameResourceAsync(bucketName, TestResourcePath, bucketName,
            "/aNewFilename/InANewPlate.png");
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.False(await _contentRepositoryScoped.ResourceExistsAsync(bucketName, TestResourcePath));
        Assert.True(await _contentRepositoryScoped.ResourceExistsAsync(bucketName, "/aNewFilename/InANewPlate.png"));
    }

    [Fact]
    public async Task OverwriteTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        var resourceFilePath = _GetTestResourceFilePath(TestFilename);
        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _contentRepositoryScoped.UploadStreamAsync(bucketName, TestResourcePath, fileStream, true, true);
            await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();
        }

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());
    }

    [Fact]
    public async Task CopyTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        await _contentRepositoryScoped.CopyResourceAsync(bucketName, TestResourcePath, bucketName,
            "/newPath/anotherBird.png");

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(2, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * 2, await _databaseHelper.CountBlocksAsync());

        await Assert.ThrowsAsync<OverwriteNotPermittedException>(async () =>
            await _contentRepositoryScoped.CopyResourceAsync(bucketName, TestResourcePath, bucketName, TestResourcePath));
    }

    [Fact]
    public async Task DeleteBucketTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        const int testArticleCount = 20;
        const int subItemCount = 5;

        // Upload the test article 100 (20 * 5) times.
        foreach (var i in Enumerable.Range(1, testArticleCount))
        {
            foreach (var j in Enumerable.Range(1, subItemCount))
            {
                var path = $"/directory/subdirectory{i}/item{j}.png";
                await _UploadTestResourceAsync(bucketName, path, false);
            }
        }

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Delete the whole bucket!
        await _contentRepositoryScoped.DeleteBucketAsync(bucketName, true);

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(0, await _databaseHelper.CountBucketsAsync());
        Assert.Equal(0, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(0, await _databaseHelper.CountBlocksAsync());
    }

    [Fact]
    public async Task FileListingRecursiveTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        const int testArticleCount = 20;
        const int subItemCount = 5;

        HashSet<string> masterTrackingSet = new HashSet<string>(testArticleCount);

        // Upload the test article 100 (20 * 5) times.
        foreach (var i in Enumerable.Range(1, testArticleCount))
        {
            foreach (var j in Enumerable.Range(1, subItemCount))
            {
                var path = $"/directory/subdirectory{i}/item{j}.png";

                masterTrackingSet.Add(path);
                await _UploadTestResourceAsync(bucketName, path, false);
            }
        }

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(testArticleCount * subItemCount, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * testArticleCount * subItemCount, await _databaseHelper.CountBlocksAsync());

        // ===================================================================================
        // Try to get a listing with no recursion...should be zero.
        // ===================================================================================
        var fileListing = await _contentRepositoryScoped.GetResourceListingAsync(bucketName,
            "/directory", 1, 200);
        Assert.Equal(0, fileListing.Count);

        // ===================================================================================
        // Get a listing for the files that we uploaded with recursive listing
        // ===================================================================================
        HashSet<string> compareSet = new HashSet<string>(masterTrackingSet);
        fileListing = await _contentRepositoryScoped.GetResourceListingAsync(bucketName, "/directory",
            1, 200, true);
        Assert.Equal(1, fileListing.PageNumber);
        Assert.Equal(100, fileListing.TotalItemCount);
        Assert.True(fileListing.IsLastPage);
        Assert.True(fileListing.IsFirstPage);
        Assert.False(fileListing.HasNextPage);
        Assert.False(fileListing.HasPreviousPage);
        Assert.Equal(1, fileListing.PageCount);

        Assert.Equal(testArticleCount * subItemCount, fileListing.Count);
        foreach (var nextItem in fileListing)
        {
            compareSet.Remove(nextItem.ResourcePath);
        }

        Assert.True(compareSet.Count == 0);
    }

    [Fact]
    public async Task FileListingTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        const int testArticleCount = 100;

        HashSet<string> masterTrackingSet = new HashSet<string>(testArticleCount);

        // Upload the test article 100 times.
        foreach (var i in Enumerable.Range(1, testArticleCount))
        {
            masterTrackingSet.Add($"/directory/item{i}.png");
            await _UploadTestResourceAsync(bucketName, $"/directory/item{i}.png", false);
        }

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(testArticleCount, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * testArticleCount, await _databaseHelper.CountBlocksAsync());

        // ===================================================================================
        // Get a listing for the files that we uploaded.  Get them all at once.
        // ===================================================================================
        var compareSet = new HashSet<string>(masterTrackingSet);
        var fileListing = await _contentRepositoryScoped.GetResourceListingAsync(
            bucketName, "/directory", 1, 200);

        Assert.Equal(testArticleCount, fileListing.Count);
        foreach (var nextItem in fileListing)
        {
            compareSet.Remove(nextItem.ResourcePath);
        }

        Assert.True(compareSet.Count == 0);

        // ===================================================================================
        // Do the same thing again, but get them in sets of 25
        // ===================================================================================
        compareSet = new HashSet<string>(masterTrackingSet);
        var pageCount = testArticleCount / 25;
        for (var nextPage = 1; nextPage <= pageCount; nextPage++)
        {
            fileListing = await _contentRepositoryScoped.GetResourceListingAsync(bucketName, "/directory", nextPage, 25);
            foreach (var nextItem in fileListing)
            {
                compareSet.Remove(nextItem.ResourcePath);
            }
        }

        Assert.True(compareSet.Count == 0);
    }

    [Fact]
    public async Task GetByUserDataGuidTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        const int testArticleCount = 100;

        var testUserGuid1 = Guid.NewGuid();
        var testUserGuid2 = Guid.NewGuid();

        // Upload the test article 100 times.
        foreach (var i in Enumerable.Range(1, testArticleCount))
        {
            var guidToUse = i % 2 == 0 ? testUserGuid1 : testUserGuid2;
            await _UploadTestResourceAsync(bucketName, $"/directory/item{i}.png", false, guidToUse);
        }

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(testArticleCount, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * testArticleCount, await _databaseHelper.CountBlocksAsync());

        // ===================================================================================
        // Use the first user guid and make sure we can select 1/2 of the data.
        // ===================================================================================
        var returnList =
            await _contentRepositoryScoped.GetResourceListingByUserGuidAsync(bucketName, testUserGuid1, 1, 200);
        Assert.Equal(testArticleCount / 2, returnList.Count);
        Assert.Equal(1, returnList.PageCount);
        Assert.Equal(50, returnList.TotalItemCount);

        // ===================================================================================
        // Use the second user guid and make sure we can select 1/2 of the data.
        // ===================================================================================
        returnList =
            await _contentRepositoryScoped.GetResourceListingByUserGuidAsync(bucketName, testUserGuid2, 1, 200);
        Assert.Equal(testArticleCount / 2, returnList.Count);
        Assert.Equal(1, returnList.PageCount);
        Assert.Equal(50, returnList.TotalItemCount);
    }

    [Fact]
    public async Task CopyPreservesUserDataTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        var guidToUse = Guid.NewGuid();
        var testValue = Random.Shared.NextInt64();

        await _UploadTestResourceAsync(bucketName, "/testItem.png", false, guidToUse, testValue);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        var fileInfo = await _contentRepositoryScoped.GetResourceInfoAsync(bucketName, "/testItem.png");
        Assert.NotNull(fileInfo);
        Assert.Equal(testValue, fileInfo!.UserDataLong);
        Assert.Equal(guidToUse, fileInfo.UserDataGuid);

        // Copy the file to another location.
        await _contentRepositoryScoped.CopyResourceAsync(bucketName, "/testItem.png", bucketName, "/testItem2.png");
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        fileInfo = await _contentRepositoryScoped.GetResourceInfoAsync(bucketName, "/testItem2.png");
        Assert.NotNull(fileInfo);
        Assert.Equal(testValue, fileInfo!.UserDataLong);
        Assert.Equal(guidToUse, fileInfo.UserDataGuid);
    }

    [Fact]
    public async Task RenamePreservesUserDataTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        var guidToUse = Guid.NewGuid();
        var testValue = Random.Shared.NextInt64();

        await _UploadTestResourceAsync(bucketName, "/testItem.png", false, guidToUse, testValue);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        var fileInfo = await _contentRepositoryScoped.GetResourceInfoAsync(bucketName, "/testItem.png");
        Assert.NotNull(fileInfo);
        Assert.Equal(testValue, fileInfo!.UserDataLong);
        Assert.Equal(guidToUse, fileInfo.UserDataGuid);

        // Copy the file to another location.
        await _contentRepositoryScoped.RenameResourceAsync(bucketName, "/testItem.png", bucketName, "/testItem2.png");
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        fileInfo = await _contentRepositoryScoped.GetResourceInfoAsync(bucketName, "/testItem2.png");
        Assert.NotNull(fileInfo);
        Assert.Equal(testValue, fileInfo!.UserDataLong);
        Assert.Equal(guidToUse, fileInfo.UserDataGuid);
    }

    [Fact]
    public async Task FileListingPagingTest()
    {
        var bucketName = "default";

        await _databaseHelper.ClearDatabaseAsync();

        const int testArticleCount = 100;
        const int pageSize = 20;

        var testUserGuid1 = Guid.NewGuid();
        var testUserGuid2 = Guid.NewGuid();

        // Upload the test article 100 times.
        foreach (var currentTestArticleOffset in Enumerable.Range(1, testArticleCount))
        {
            var guidToUse = currentTestArticleOffset % 2 == 0 ? testUserGuid1 : testUserGuid2;
            await _UploadTestResourceAsync(bucketName, $"/directory/item{currentTestArticleOffset}.png", false, guidToUse);
        }

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(testArticleCount, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * testArticleCount, await _databaseHelper.CountBlocksAsync());
        
        // Pull in 5 pages at 20 items per page.
        foreach (var currentPageOffset in Enumerable.Range(1, 5))
        {
            var fileListing = await _contentRepositoryScoped.GetResourceListingAsync(bucketName, $"/directory", currentPageOffset, 20);
            Assert.Equal(testArticleCount, fileListing.TotalItemCount);
            Assert.Equal(testArticleCount/pageSize, fileListing.PageCount);
            Assert.Equal(pageSize, fileListing.Count);
            Assert.Equal(currentPageOffset, fileListing.PageNumber);
            Assert.Equal( (currentPageOffset-1) * pageSize + 1, fileListing.FirstItemOnPage);
            Assert.Equal( (currentPageOffset-1) * pageSize + pageSize, fileListing.LastItemOnPage);
                
            if (currentPageOffset == 1)
            {
                Assert.True(fileListing.IsFirstPage);
                Assert.False(fileListing.IsLastPage);
                Assert.False(fileListing.HasPreviousPage);
                Assert.True(fileListing.HasNextPage);
            }
            else if (currentPageOffset == 5)
            {
                Assert.False(fileListing.IsFirstPage);
                Assert.True(fileListing.IsLastPage);
                Assert.True(fileListing.HasPreviousPage);
                Assert.False(fileListing.HasNextPage);
            }
            else
            {
                Assert.False(fileListing.IsFirstPage);
                Assert.False(fileListing.IsLastPage);
                Assert.True(fileListing.HasPreviousPage);
                Assert.True(fileListing.HasNextPage);
            }
        }
    }

    private static string _GetTestResourceFilePath(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        var basePath = Path.GetDirectoryName(assembly.Location)!;
        Assert.NotNull(basePath);

        var resourceFilePath = Path.Combine(basePath, "TestFiles", filename);
        Assert.True(File.Exists(resourceFilePath));
        return resourceFilePath;
    }

    private async Task _UploadTestResourceAsync()
    {
        var bucketName = "default";

        var resourceFilePath = _GetTestResourceFilePath(TestFilename);

        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _contentRepositoryScoped.UploadStreamAsync(bucketName, TestResourcePath, fileStream);
            await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();
        }

        Assert.True(await _contentRepositoryScoped.ResourceExistsAsync(bucketName, TestResourcePath));
    }

    private async Task _UploadTestResourceAsync(string bucketName, string resourceFilename, bool saveAfterUpdate = true,
        Guid? userGuid = null, long userLong = 0L)
    {
        var resourceFilePath = _GetTestResourceFilePath(TestFilename);

        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _contentRepositoryScoped.UploadStreamAsync(bucketName, resourceFilename, fileStream,
                userGuid: userGuid, userValue: userLong);

            if (saveAfterUpdate)
            {
                await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();
            }
        }

        if (saveAfterUpdate)
        {
            Assert.True(await _contentRepositoryScoped.ResourceExistsAsync(bucketName, resourceFilename));
        }
    }
}