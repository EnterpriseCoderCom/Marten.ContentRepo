using System.Reflection;
using System.Security.Cryptography;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.MartenDb.GridFs.Testing;

public class StorageTests : IClassFixture<DatabaseTestFixture>
{
    private const string TestFilename = "angrybird.png";
    private const string TestResourcePath = "/resources/angrybird.png";
    
    private readonly IGridFileSystem _gridFileSystem;
    private readonly DatabaseHelper _databaseHelper;
    
    public StorageTests(DatabaseTestFixture databaseFixture)
    {
        _gridFileSystem = databaseFixture.ServiceProvider.GetRequiredService<IGridFileSystem>();
        _databaseHelper = databaseFixture.ServiceProvider.GetRequiredService<DatabaseHelper>();
    }

    [Fact]
    public async Task StoreFileAndRetrieve()
    {
        await _databaseHelper.ClearDatabaseAsync();
        
        var resourceFilePath = _GetTestResourceFilePath(TestFilename);

        using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _gridFileSystem.SaveStreamAsync(TestResourcePath, fileStream);
            await _gridFileSystem.DocumentSession.SaveChangesAsync();
        }
        
        Assert.True(await _gridFileSystem.FileExistsAsync(TestResourcePath));

        await using Stream? readStream = await _gridFileSystem.LoadStreamAsync(TestResourcePath);
        Assert.NotNull(readStream);
        
        // Generate a sha256
        byte[] rereadSha256;
        using (SHA256 sha256Hash = SHA256.Create())
        {
            rereadSha256 = await sha256Hash.ComputeHashAsync(readStream!);
        }

        // Load and compare the sha256
        GridFsFileInfo? fileInfo = await _gridFileSystem.GetFileInfoAsync(TestResourcePath);
        Assert.NotNull(fileInfo);
        
        // Verify that the SHA256's are the same.
        Assert.Equal(rereadSha256, fileInfo!.Sha256);
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

    [Fact]
    public async Task StoreAndDeleteFile()
    {
        await _databaseHelper.ClearDatabaseAsync();
        
        var resourceFilePath = _GetTestResourceFilePath(TestFilename);
        
        using (var fileStream = new FileStream(resourceFilePath, FileMode.Open))
        {
            await _gridFileSystem.SaveStreamAsync(TestResourcePath, fileStream);
            await _gridFileSystem.DocumentSession.SaveChangesAsync();
        }

        Assert.True(await _gridFileSystem.FileExistsAsync(TestResourcePath));

        await _gridFileSystem.DeleteFileAsync(TestResourcePath);
        await _gridFileSystem.DocumentSession.SaveChangesAsync();
        
        Assert.False(await _gridFileSystem.FileExistsAsync(TestResourcePath));
    }
}