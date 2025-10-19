using System.Text;
using EnterpriseCoder.Marten.ContentRepo.Di;
using EnterpriseCoder.Marten.ContentRepo.Entities;
using EnterpriseCoder.Marten.ContentRepo.Exceptions;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseCoder.Marten.ContentRepo.Testing;

public class DirectoryOperationsTests : IClassFixture<DatabaseTestFixture>, IDisposable
{
    private readonly IContentRepositoryScoped _contentRepositoryScoped;
    private readonly DatabaseHelper _databaseHelper;
    private readonly string _testBucket = "test-bucket";

    public DirectoryOperationsTests(DatabaseTestFixture databaseFixture)
    {
        _contentRepositoryScoped = databaseFixture.ServiceProvider.GetRequiredService<IContentRepositoryScoped>();
        _databaseHelper = databaseFixture.ServiceProvider.GetRequiredService<DatabaseHelper>();

        _databaseHelper.ClearDatabaseAsync().Wait();
    }

    public void Dispose()
    {
        _databaseHelper.ClearDatabaseAsync().Wait();
    }

    private async Task CreateBucketAsync(string bucketName)
    {
        await _contentRepositoryScoped.CreateBucketAsync(bucketName);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();
    }

    #region CreateDirectoryAsync Tests

    [Fact]
    public async Task CreateDirectoryAsync_WithValidPath_CreatesDirectory()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");

        // Act
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, directoryPath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        var exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, directoryPath);
        Assert.True(exists);
    }

    [Fact]
    public async Task CreateDirectoryAsync_WithNestedPath_CreatesNestedDirectory()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir1 = new ContentRepositoryDirectory("/projects");
        var dir2 = new ContentRepositoryDirectory("/projects/2024");

        // Act
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir1);
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir2);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir1));
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir2));
    }

    [Fact]
    public async Task CreateDirectoryAsync_WithDuplicatePath_ThrowsException()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, directoryPath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryAlreadyExistsException>(
            () => _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, directoryPath)
        );
    }

    [Fact]
    public async Task CreateDirectoryAsync_WithoutBucket_ThrowsException()
    {
        // Arrange
        var directoryPath = new ContentRepositoryDirectory("/projects");

        // Act & Assert
        await Assert.ThrowsAsync<BucketNotFoundException>(
            () => _contentRepositoryScoped.CreateDirectoryAsync(
                "non-existent-bucket", directoryPath)
        );
    }

    [Fact]
    public async Task CreateDirectoryAsync_WithAutoCreateBucket_CreatesBucket()
    {
        // Arrange
        var bucketName = "new-bucket";
        var directoryPath = new ContentRepositoryDirectory("/projects");

        // Act
        await _contentRepositoryScoped.CreateDirectoryAsync(bucketName, directoryPath, autoCreateBucket: true);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        bool exists = await _contentRepositoryScoped.DirectoryExistsAsync(bucketName, directoryPath);
        Assert.True(exists);
    }

    [Fact]
    public async Task CreateDirectoryAsync_WithMultiLevelNesting_CreatesAll()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var paths = new[]
        {
            new ContentRepositoryDirectory("/assets"),
            new ContentRepositoryDirectory("/assets/images"),
            new ContentRepositoryDirectory("/assets/images/backgrounds"),
            new ContentRepositoryDirectory("/assets/images/icons")
        };

        // Act
        foreach (var path in paths)
        {
            await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, path);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        foreach (var path in paths)
        {
            Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, path));
        }
    }

    [Fact]
    public async Task CreateDirectoryAsync_WithRootDirectory_CreatesRoot()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var rootPath = new ContentRepositoryDirectory("/");

        // Act
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, rootPath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        var exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, rootPath);
        Assert.True(exists);
    }

    #endregion

    #region DeleteDirectoryAsync Tests

    [Fact]
    public async Task DeleteDirectoryAsync_WithEmptyDirectory_Succeeds()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, directoryPath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        await _contentRepositoryScoped.DeleteDirectoryAsync(_testBucket, directoryPath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        bool exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, directoryPath);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteDirectoryAsync_WithFilesNoForce_ThrowsException()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");
        var filePath = new ContentRepositoryResourcePath("/projects/file.txt");

        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, directoryPath);
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("content")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotEmptyException>(
            () => _contentRepositoryScoped.DeleteDirectoryAsync(_testBucket, directoryPath, force: false)
        );
    }

    [Fact]
    public async Task DeleteDirectoryAsync_WithFilesForce_DeletesAll()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");
        var filePath = new ContentRepositoryResourcePath("/projects/file.txt");

        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, directoryPath);
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("content")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        await _contentRepositoryScoped.DeleteDirectoryAsync(_testBucket, directoryPath, force: true);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        bool exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, directoryPath);
        Assert.False(exists);
    }

    [Fact]
    public async Task DeleteDirectoryAsync_WithNonExistentDirectory_ThrowsException()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");

        // Act & Assert
        await Assert.ThrowsAsync<EnterpriseCoder.Marten.ContentRepo.Exceptions.DirectoryNotFoundException>(
            () => _contentRepositoryScoped.DeleteDirectoryAsync(_testBucket, directoryPath)
        );
    }

    [Fact]
    public async Task DeleteDirectoryAsync_WithNestedFiles_ForceDeletesAll()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir1 = new ContentRepositoryDirectory("/data");
        var dir2 = new ContentRepositoryDirectory("/data/backups");
        var filePath1 = new ContentRepositoryResourcePath("/data/config.txt");
        var filePath2 = new ContentRepositoryResourcePath("/data/backups/backup.zip");

        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir1);
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir2);

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("config")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath1, stream);
        }

        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("backup")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath2, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        await _contentRepositoryScoped.DeleteDirectoryAsync(_testBucket, dir1, force: true);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        Assert.False(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir1));
        Assert.False(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir2));
    }

    [Fact]
    public async Task DeleteDirectoryAsync_WithMultipleFiles_NoForceLeavesOtherDirectories()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir1 = new ContentRepositoryDirectory("/photos");
        var dir2 = new ContentRepositoryDirectory("/videos");

        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir1);
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir2);

        var filePath = new ContentRepositoryResourcePath("/photos/pic.jpg");
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("photo")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<DirectoryNotEmptyException>(
            () => _contentRepositoryScoped.DeleteDirectoryAsync(_testBucket, dir1, force: false)
        );

        // Verify other directory still exists
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir2));
    }

    #endregion

    #region DirectoryExistsAsync Tests

    [Fact]
    public async Task DirectoryExistsAsync_WithExistingExplicitDirectory_ReturnsTrue()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, directoryPath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        bool exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, directoryPath);

        // Assert
        Assert.True(exists);
    }

    [Fact]
    public async Task DirectoryExistsAsync_WithNonExistentDirectory_ReturnsFalse()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var directoryPath = new ContentRepositoryDirectory("/projects");

        // Act
        bool exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, directoryPath);

        // Assert
        Assert.False(exists);
    }

    [Fact]
    public async Task DirectoryExistsAsync_WithImplicitDirectory_ReturnsFalse()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var filePath = new ContentRepositoryResourcePath("/projects/file.txt");
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("content")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        var implicitDir = new ContentRepositoryDirectory("/projects");

        // Act
        bool exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, implicitDir);

        // Assert
        Assert.False(exists);  // Implicit doesn't count as explicit directory
    }

    [Fact]
    public async Task DirectoryExistsAsync_WithMultipleLevels_IdentifiesEachLevel()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var root = new ContentRepositoryDirectory("/");
        var level1 = new ContentRepositoryDirectory("/data");
        var level2 = new ContentRepositoryDirectory("/data/backup");

        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, level1);
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, level2);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act & Assert
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, level1));
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, level2));
    }

    #endregion

    #region GetDirectoryListingAsync Tests

    [Fact]
    public async Task GetDirectoryListingAsync_WithExplicitDirectories_ReturnsAll()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/photos"));
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/videos"));
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/"));

        // Assert
        Assert.Equal(2, listing.Count);
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "photos")));
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "videos")));
    }

    [Fact]
    public async Task GetDirectoryListingAsync_WithMixedContent_ReturnsAllUnique()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        // Create explicit directory
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/photos"));

        // Create file implying directory
        var filePath = new ContentRepositoryResourcePath("/videos/video1.mp4");
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("content")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/"));

        // Assert
        Assert.Equal(2, listing.Count);
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "photos")));
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "videos")));
    }

    [Fact]
    public async Task GetDirectoryListingAsync_WithEmptyDirectory_ReturnsEmpty()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        // Act
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/"));

        // Assert
        Assert.Empty(listing);
    }

    [Fact]
    public async Task GetDirectoryListingAsync_WithNestedDirectories_ReturnsOnlyDirectChildren()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/root"));
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/root/child1"));
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/root/child2"));
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/root"));

        // Assert
        Assert.Equal(2, listing.Count);
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "child1")));
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "child2")));
    }

    [Fact]
    public async Task GetDirectoryListingAsync_WithMixedLevels_FiltersByParent()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/root"));
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/root/child"));
        await _contentRepositoryScoped.CreateDirectoryAsync(
            _testBucket, new ContentRepositoryDirectory("/other"));
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/root"));

        // Assert
        Assert.Single(listing);
        Assert.Equal("child", listing.First().ChildDirectoryName);
    }

    [Fact]
    public async Task GetDirectoryListingAsync_WithDuplicateImplicitExplicit_ReturnsOnceEach()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir = new ContentRepositoryDirectory("/media");
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir);

        // Upload file that would create implicit directory at same level
        var filePath = new ContentRepositoryResourcePath("/files/document.pdf");
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("pdf")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/"));

        // Assert
        Assert.Equal(2, listing.Count);  // Should have both, not duplicates
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "media")));
        Assert.NotNull(Assert.Single(listing.Where(x => x.ChildDirectoryName == "files")));
    }

    #endregion

    #region Integration Tests

    [Fact]
    public async Task CompleteWorkflow_CreateStructureUploadDelete_Works()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir1 = new ContentRepositoryDirectory("/2024");
        var dir2 = new ContentRepositoryDirectory("/2024/january");
        var filePath = new ContentRepositoryResourcePath("/2024/january/photo.jpg");

        // Act - Create directories
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir1);
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir2);

        // Upload file
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("image")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Verify structure
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/"));
        Assert.Single(listing);
        Assert.Equal("2024", listing.First().ChildDirectoryName);

        // Delete file
        await _contentRepositoryScoped.DeleteResourceAsync(_testBucket, filePath);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Verify directories still exist (cleanup is separate)
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir1));
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir2));
    }

    [Fact]
    public async Task CompleteWorkflow_MultipleDirectoriesWithFiles_Works()
    {
        // Arrange & Act
        await CreateBucketAsync(_testBucket);
        var dirs = new[]
        {
            new ContentRepositoryDirectory("/2024"),
            new ContentRepositoryDirectory("/2024/january"),
            new ContentRepositoryDirectory("/2024/february"),
            new ContentRepositoryDirectory("/2025")
        };

        foreach (var dir in dirs)
        {
            await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir);
        }

        // Upload files to each year
        for (int month = 1; month <= 2; month++)
        {
            var filePath = new ContentRepositoryResourcePath($"/2024/{(month == 1 ? "january" : "february")}/file{month}.txt");
            using var stream = new MemoryStream(Encoding.UTF8.GetBytes($"file{month}"));
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }

        var file2025 = new ContentRepositoryResourcePath("/2025/file.txt");
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("2025file")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, file2025, stream);
        }

        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Act - Get root listing
        var rootListing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/"));

        // Assert
        Assert.Equal(2, rootListing.Count);
        Assert.Contains(rootListing, x => x.ChildDirectoryName == "2024");
        Assert.Contains(rootListing, x => x.ChildDirectoryName == "2025");

        // Verify 2024 subdirectories
        var listing2024 = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/2024"));
        Assert.Equal(2, listing2024.Count);
    }

    [Fact]
    public async Task CompleteWorkflow_CreateDeleteRecreate_Works()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir = new ContentRepositoryDirectory("/temp");

        // Act - Create
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir));

        // Delete
        await _contentRepositoryScoped.DeleteDirectoryAsync(_testBucket, dir);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();
        Assert.False(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir));

        // Recreate
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir));
    }

    [Fact]
    public async Task CompleteWorkflow_DeleteBucketWithDirectories_RemovesAll()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir1 = new ContentRepositoryDirectory("/data");
        var dir2 = new ContentRepositoryDirectory("/data/archive");

        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir1);
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir2);

        var filePath = new ContentRepositoryResourcePath("/data/archive/file.zip");
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes("archive")))
        {
            await _contentRepositoryScoped.UploadStreamAsync(_testBucket, filePath, stream);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Verify setup
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir1));
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir2));

        // Act - Delete bucket
        await _contentRepositoryScoped.DeleteBucketAsync(_testBucket, force: true);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert - Directories should be gone
        Assert.False(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir1));
        Assert.False(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir2));
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public async Task DirectoryOperations_WithSpecialCharactersInPath_Works()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir = new ContentRepositoryDirectory("/project-2024_final");

        // Act
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        var exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir);
        Assert.True(exists);
    }

    [Fact]
    public async Task DirectoryOperations_WithDeeplyNestedPath_Works()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var paths = new[]
        {
            new ContentRepositoryDirectory("/a"),
            new ContentRepositoryDirectory("/a/b"),
            new ContentRepositoryDirectory("/a/b/c"),
            new ContentRepositoryDirectory("/a/b/c/d"),
            new ContentRepositoryDirectory("/a/b/c/d/e")
        };

        // Act
        foreach (var path in paths)
        {
            await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, path);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        foreach (var path in paths)
        {
            Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, path));
        }
    }

    [Fact]
    public async Task DirectoryOperations_WithLongDirectoryName_Works()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var longName = new string('x', 200);
        var dir = new ContentRepositoryDirectory($"/{longName}");

        // Act
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        var exists = await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir);
        Assert.True(exists);
    }

    [Fact]
    public async Task DirectoryOperations_CaseSensitivity_TreatsPathsAsNormalized()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dir1 = new ContentRepositoryDirectory("/Projects");
        var dir2 = new ContentRepositoryDirectory("/projects");

        // Act
        await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir1);
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert - Both should refer to same directory (normalized to lowercase)
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir1));
        Assert.True(await _contentRepositoryScoped.DirectoryExistsAsync(_testBucket, dir2));
    }

    [Fact]
    public async Task DirectoryOperations_ManyDirectoriesInBucket_Works()
    {
        // Arrange
        await CreateBucketAsync(_testBucket);
        var dirCount = 50;
        var directories = Enumerable.Range(1, dirCount)
            .Select(i => new ContentRepositoryDirectory($"/dir{i:D3}"))
            .ToList();

        // Act
        foreach (var dir in directories)
        {
            await _contentRepositoryScoped.CreateDirectoryAsync(_testBucket, dir);
        }
        await _contentRepositoryScoped.DocumentSession.SaveChangesAsync();

        // Assert
        var listing = await _contentRepositoryScoped.GetDirectoryListingAsync(
            _testBucket, new ContentRepositoryDirectory("/"));
        Assert.Equal(dirCount, listing.Count);
    }

    #endregion
}
