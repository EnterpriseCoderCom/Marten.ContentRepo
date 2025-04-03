using System.Reflection;
using System.Security.Cryptography;
using EnterpriseCoder.MartenDb.GridFs.Di;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;

namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class GridFileSystemScopedTests : IClassFixture<DatabaseTestFixture>
{
    private readonly ITestOutputHelper _output;
    private const string TestFilename = "angrybird.png";
    private const string TestResourcePath = "/resources/angrybird.png";
    private const int TestBlockCount = 8;

    private readonly IGridFileSystemScoped _gridFileSystemScoped;
    private readonly DatabaseHelper _databaseHelper;

    public GridFileSystemScopedTests(DatabaseTestFixture databaseFixture, ITestOutputHelper output)
    {
        _output = output;
        _gridFileSystemScoped = databaseFixture.ServiceProvider.GetRequiredService<IGridFileSystemScoped>();
        _databaseHelper = databaseFixture.ServiceProvider.GetRequiredService<DatabaseHelper>();
    }

    [Fact]
    public async Task UploadAndRetrieve()
    {
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
            await _gridFileSystemScoped.UploadStreamAsync(TestResourcePath, fileStream);
            await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();
        }

        Assert.True(await _gridFileSystemScoped.FileExistsAsync(TestResourcePath));
        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        // ===================================================================================
        // Try to overwrite the file by saving again...should throw since we are going 
        // to leave the "overwriteExisting" argument to default to false.
        // ===================================================================================
        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await Assert.ThrowsAsync<IOException>(async () =>
                await _gridFileSystemScoped.UploadStreamAsync(TestResourcePath, fileStream));
            _gridFileSystemScoped.DocumentSession.EjectAllPendingChanges();
        }

        // ===================================================================================
        // Read the saved data back out of the grid file system and ensure that the SHA256's
        // match to ensure data integrity
        // ===================================================================================
        await using Stream? readStream = await _gridFileSystemScoped.DownloadStreamAsync(TestResourcePath);
        Assert.NotNull(readStream);

        // Generate a sha256 for the stream that comes out of the database.
        byte[] rereadSha256;
        using (SHA256 sha256Hash = SHA256.Create())
        {
            rereadSha256 = await sha256Hash.ComputeHashAsync(readStream!);
        }

        // Get the file information from the database and ensure that the stored 
        // SHA256 matches.
        GridFsFileInfo? fileInfo = await _gridFileSystemScoped.GetFileInfoAsync(TestResourcePath);
        Assert.NotNull(fileInfo);

        // Verify that the SHA256's are the same between stored vs reloaded.
        Assert.Equal(rereadSha256, fileInfo!.Sha256);

        // Now load the original resource and get it's SHa256.
        await using (FileStream resourceStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            byte[] originalSha;
            using (SHA256 sha256Hash = SHA256.Create())
            {
                originalSha = await sha256Hash.ComputeHashAsync(resourceStream);
            }

            Assert.Equal(rereadSha256, originalSha);
        }
    }

    [Fact]
    public async Task UploadAndDeleteFile()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        await _gridFileSystemScoped.DeleteFileAsync(TestResourcePath);
        await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(0, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(0, await _databaseHelper.CountBlocksAsync());

        Assert.False(await _gridFileSystemScoped.FileExistsAsync(TestResourcePath));
    }

    [Fact]
    public async Task RenameFileTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        await _gridFileSystemScoped.RenameFileAsync(TestResourcePath, "/aNewFilename/InANewPlate.png");
        await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();

        Assert.False(await _gridFileSystemScoped.FileExistsAsync(TestResourcePath));
        Assert.True(await _gridFileSystemScoped.FileExistsAsync("/aNewFilename/InANewPlate.png"));
    }

    [Fact]
    public async Task OverwriteTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        var resourceFilePath = _GetTestResourceFilePath(TestFilename);
        using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _gridFileSystemScoped.UploadStreamAsync(TestResourcePath, fileStream, true);
            await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();
        }

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());
    }

    [Fact]
    public async Task CopyTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _UploadTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        await _gridFileSystemScoped.CopyFileAsync(TestResourcePath, "/newPath/anotherBird.png", true);
        await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(2, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * 2, await _databaseHelper.CountBlocksAsync());

        await Assert.ThrowsAsync<IOException>(async () =>
            await _gridFileSystemScoped.CopyFileAsync(TestResourcePath, TestResourcePath));
    }

    [Fact]
    public async Task FileListingRecursiveTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        const int testArticleCount = 20;
        const int subItemCount = 5;

        HashSet<string> masterTrackingSet = new HashSet<string>(testArticleCount);

        // Upload the test article 100 (20 * 5) times.
        foreach (var i in Enumerable.Range(1, testArticleCount))
        {
            foreach (var j in Enumerable.Range(1, subItemCount))
            {
                string path = $"/directory/subdirectory{i}/item{j}.png";
                
                masterTrackingSet.Add(path);
                await _UploadTestResourceAsync(path, false);
            }
        }

        await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();
        
        Assert.Equal(testArticleCount * subItemCount, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * testArticleCount * subItemCount, await _databaseHelper.CountBlocksAsync());

        // ===================================================================================
        // Try to get a listing with no recursion...should be zero.
        // ===================================================================================
        var fileListing = await _gridFileSystemScoped.GetFileListingAsync(
            "/directory", 1, 200);
        Assert.Equal(0, fileListing.Count);
        
        // ===================================================================================
        // Get a listing for the files that we uploaded with recursive listing
        // ===================================================================================
        HashSet<string> compareSet = new HashSet<string>(masterTrackingSet);
        fileListing = await _gridFileSystemScoped.GetFileListingAsync("/directory",
            1, 200, true);
        
        Assert.Equal(testArticleCount * subItemCount, fileListing.Count);
        foreach (var nextItem in fileListing)
        {
            compareSet.Remove(nextItem.FilePath);
        }

        Assert.True(compareSet.Count == 0);
    }
    
    [Fact]
    public async Task FileListingTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        const int testArticleCount = 100;

        HashSet<string> masterTrackingSet = new HashSet<string>(testArticleCount);

        // Upload the test article 100 times.
        foreach (var i in Enumerable.Range(1, testArticleCount))
        {
            masterTrackingSet.Add($"/directory/item{i}.png");
            await _UploadTestResourceAsync($"/directory/item{i}.png", false);
        }

        await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();

        Assert.Equal(testArticleCount, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * testArticleCount, await _databaseHelper.CountBlocksAsync());

        // ===================================================================================
        // Get a listing for the files that we uploaded.  Get them all at once.
        // ===================================================================================
        HashSet<string> compareSet = new HashSet<string>(masterTrackingSet);
        var fileListing = await _gridFileSystemScoped.GetFileListingAsync("/directory", 1, 200);
        Assert.Equal(testArticleCount, fileListing.Count);
        foreach (var nextItem in fileListing)
        {
            compareSet.Remove(nextItem.FilePath);
        }

        Assert.True(compareSet.Count == 0);

        // ===================================================================================
        // Do the same thing again, but get them in sets of 25
        // ===================================================================================
        compareSet = new HashSet<string>(masterTrackingSet);
        int pageCount = testArticleCount / 25;
        for (int nextPage = 1; nextPage <= pageCount; nextPage++)
        {
            fileListing = await _gridFileSystemScoped.GetFileListingAsync("/directory", nextPage, 25);
            foreach (var nextItem in fileListing)
            {
                compareSet.Remove(nextItem.FilePath);
            }
        }

        Assert.True(compareSet.Count == 0);
    }
    
    private static string _GetTestResourceFilePath(string filename)
    {
        var assembly = Assembly.GetExecutingAssembly();
        string basePath = Path.GetDirectoryName(assembly.Location)!;
        Assert.NotNull(basePath);

        string resourceFilePath = Path.Combine(basePath, "TestFiles", filename);
        Assert.True(File.Exists(resourceFilePath));
        return resourceFilePath;
    }

    private async Task _UploadTestResourceAsync()
    {
        var resourceFilePath = _GetTestResourceFilePath(TestFilename);

        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _gridFileSystemScoped.UploadStreamAsync(TestResourcePath, fileStream);
            await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();
        }

        Assert.True(await _gridFileSystemScoped.FileExistsAsync(TestResourcePath));
    }

    private async Task _UploadTestResourceAsync(string resourceFilename, bool saveAfterUpdate = true)
    {
        var resourceFilePath = _GetTestResourceFilePath(TestFilename);

        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _gridFileSystemScoped.UploadStreamAsync(resourceFilename, fileStream);
            if (saveAfterUpdate)
            {
                await _gridFileSystemScoped.DocumentSession.SaveChangesAsync();
            }
        }

        if (saveAfterUpdate)
        {
            Assert.True(await _gridFileSystemScoped.FileExistsAsync(resourceFilename));
        }
    }
}