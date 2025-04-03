using Marten;

namespace EnterpriseCoder.MartenDb.GridFs;

public interface IGridFileSystem
{
    Task UploadStreamAsync(IDocumentSession documentSession, GridFsFilePath filePath, Stream inStream,
        bool overwriteExisting = false,
        Guid? userGuid = null, long userValue = 0L);
    Task<Stream?> DownloadStreamAsync(IDocumentSession documentSession, GridFsFilePath filePath);
    Task<bool> FileExistsAsync(IDocumentSession documentSession, GridFsFilePath filePath);
    Task DeleteFileAsync(IDocumentSession documentSession, GridFsFilePath filePath);
    Task<GridFsFileInfo?> GetFileInfoAsync(IDocumentSession documentSession, GridFsFilePath filePath);
    Task RenameFileAsync(IDocumentSession documentSession, GridFsFilePath oldFilePath, GridFsFilePath newFilePath,
        bool overwriteDestination = false);
    Task CopyFileAsync(IDocumentSession documentSession, GridFsFilePath oldFilePath, GridFsFilePath newFilePath,
        bool overwriteDestination = false);
    Task<IList<GridFsFileInfo>> GetFileListingAsync(IDocumentSession documentSession, GridFsDirectory directory,
        int oneBasedPage, int pageSize,
        bool recursive = false);
}