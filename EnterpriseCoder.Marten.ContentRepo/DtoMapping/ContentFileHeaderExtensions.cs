using EnterpriseCoder.Marten.ContentRepo.Entities;

namespace EnterpriseCoder.Marten.ContentRepo.DtoMapping;

public static class ContentFileHeaderExtensions
{
    public static ContentRepositoryFileInfo ToContentFileInfoDto(this ContentFileHeader inHeader)
    {
        return new ContentRepositoryFileInfo
        {
            ResourcePath = inHeader.FilePath,
            StoredLength = inHeader.StoredLength,
            UserDataLong = inHeader.UserDataLong,
            UserDataGuid = inHeader.UserDataGuid,
            Sha256 = inHeader.Sha256,
            OriginalLength = inHeader.OriginalLength,
            UpdateDateTime = inHeader.UpdatedDateTime
        };
    }
}