using Marten;

namespace EnterpriseCoder.MartenDb.GridFs.Di;

public class GridFileSystemScoped : IGridFileSystemScoped
{
    private readonly IDocumentSession _documentSession;
    private readonly IGridFileSystem _gridFileSystem;

    public GridFileSystemScoped(IDocumentSession documentSession, IGridFileSystem gridFileSystem)
    {
        _documentSession = documentSession;
        _gridFileSystem = gridFileSystem;
    }

    public async Task UploadStreamAsync(GridFsFilePath filePath, Stream inStream, bool overwriteExisting = false,
        Guid? userGuid = null,
        long userValue = 0L)
    {
        await _gridFileSystem.UploadStreamAsync(_documentSession, filePath, inStream, overwriteExisting, userGuid,
            userValue);
    }

    public async Task<Stream?> DownloadStreamAsync(GridFsFilePath filePath)
    {
        return await _gridFileSystem.DownloadStreamAsync(_documentSession, filePath);
    }

    public async Task<bool> FileExistsAsync(GridFsFilePath filePath)
    {
        return await _gridFileSystem.FileExistsAsync(_documentSession, filePath);
    }

    public async Task DeleteFileAsync(GridFsFilePath filePath)
    {
        await _gridFileSystem.DeleteFileAsync(_documentSession, filePath);
    }

    public async Task<GridFsFileInfo?> GetFileInfoAsync(GridFsFilePath filePath)
    {
        return await _gridFileSystem.GetFileInfoAsync(_documentSession, filePath);
    }

    public async Task RenameFileAsync(GridFsFilePath oldFilePath, GridFsFilePath newFilePath,
        bool overwriteDestination = false)
    {
        await _gridFileSystem.RenameFileAsync(_documentSession, oldFilePath, newFilePath, overwriteDestination);
    }

    public async Task CopyFileAsync(GridFsFilePath oldFilePath, GridFsFilePath newFilePath,
        bool overwriteDestination = false)
    {
        await _gridFileSystem.CopyFileAsync(_documentSession, oldFilePath, newFilePath);
    }

    public async Task<IList<GridFsFileInfo>> GetFileListingAsync(GridFsDirectory directory, int oneBasedPage,
        int pageSize, bool recursive = false)
    {
        return await _gridFileSystem.GetFileListingAsync(_documentSession, directory, oneBasedPage, pageSize,
            recursive);
    }

    public IDocumentSession DocumentSession => _documentSession;
}