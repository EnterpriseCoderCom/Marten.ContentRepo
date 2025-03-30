using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class GridFileSystemTests : IClassFixture<DatabaseTestFixture>
{
    private const string TestFilename = "angrybird.png";
    private const string TestResourcePath = "/resources/angrybird.png";
    private const int TestBlockCount = 8;

    private readonly IGridFileSystem _gridFileSystem;
    private readonly DatabaseHelper _databaseHelper;

    public GridFileSystemTests(DatabaseTestFixture databaseFixture)
    {
        _gridFileSystem = databaseFixture.ServiceProvider.GetRequiredService<IGridFileSystem>();
        _databaseHelper = databaseFixture.ServiceProvider.GetRequiredService<DatabaseHelper>();
    }

    [Fact]
    public async Task StoreFileAndRetrieve()
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
            await _gridFileSystem.UploadStreamAsync(TestResourcePath, fileStream);
            await _gridFileSystem.DocumentSession.SaveChangesAsync();
        }
        Assert.True(await _gridFileSystem.FileExistsAsync(TestResourcePath));
        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());
        
        // ===================================================================================
        // Try to overwrite the file by saving again...should throw since we are going 
        // to leave the "overwriteExisting" argument to default to false.
        // ===================================================================================
        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await Assert.ThrowsAsync<IOException>(async () =>
                await _gridFileSystem.UploadStreamAsync(TestResourcePath, fileStream));
            _gridFileSystem.DocumentSession.EjectAllPendingChanges();
        }

        // ===================================================================================
        // Read the saved data back out of the grid file system and ensure that the SHA256's
        // match to ensure data integrity
        // ===================================================================================
        await using Stream? readStream = await _gridFileSystem.DownLoadStreamAsync(TestResourcePath);
        Assert.NotNull(readStream);

        // Generate a sha256 for the stream that comes out of the database.
        byte[] rereadSha256;
        using (SHA256 sha256Hash = SHA256.Create())
        {
            rereadSha256 = await sha256Hash.ComputeHashAsync(readStream!);
        }

        // Get the file information from the database and ensure that the stored 
        // SHA256 matches.
        GridFsFileInfo? fileInfo = await _gridFileSystem.GetFileInfoAsync(TestResourcePath);
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
    public async Task StoreAndDeleteFile()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _StoreTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        await _gridFileSystem.DeleteFileAsync(TestResourcePath);
        await _gridFileSystem.DocumentSession.SaveChangesAsync();
        
        Assert.Equal(0, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(0, await _databaseHelper.CountBlocksAsync());

        Assert.False(await _gridFileSystem.FileExistsAsync(TestResourcePath));
    }

    [Fact]
    public async Task RenameFileTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _StoreTestResourceAsync();

        await _gridFileSystem.RenameFileAsync(TestResourcePath, "/aNewFilename/InANewPlate.png");
        await _gridFileSystem.DocumentSession.SaveChangesAsync();

        Assert.False(await _gridFileSystem.FileExistsAsync(TestResourcePath));
        Assert.True(await _gridFileSystem.FileExistsAsync("/aNewFilename/InANewPlate.png"));
    }

    [Fact]
    public async Task OverwriteTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _StoreTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        var resourceFilePath = _GetTestResourceFilePath(TestFilename);
        using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _gridFileSystem.UploadStreamAsync(TestResourcePath, fileStream, true);
            await _gridFileSystem.DocumentSession.SaveChangesAsync();
        }
        
        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());
    }

    [Fact]
    public async Task CopyTest()
    {
        await _databaseHelper.ClearDatabaseAsync();

        await _StoreTestResourceAsync();

        Assert.Equal(1, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount, await _databaseHelper.CountBlocksAsync());

        await _gridFileSystem.CopyFileAsync(TestResourcePath, "/newPath/anotherBird.png", true);
        await _gridFileSystem.DocumentSession.SaveChangesAsync();
        
        Assert.Equal(2, await _databaseHelper.CountHeadersAsync());
        Assert.Equal(TestBlockCount * 2, await _databaseHelper.CountBlocksAsync());

        await Assert.ThrowsAsync<IOException>(async () =>
            await _gridFileSystem.CopyFileAsync(TestResourcePath, TestResourcePath));
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

    private async Task _StoreTestResourceAsync()
    {
        var resourceFilePath = _GetTestResourceFilePath(TestFilename);

        await using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _gridFileSystem.UploadStreamAsync(TestResourcePath, fileStream);
            await _gridFileSystem.DocumentSession.SaveChangesAsync();
        }

        Assert.True(await _gridFileSystem.FileExistsAsync(TestResourcePath));
    }
}