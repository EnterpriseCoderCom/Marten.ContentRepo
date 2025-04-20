using EnterpriseCoder.Marten.ContentRepo.Entities;

namespace EnterpriseCoder.Marten.ContentRepo.DtoMapping;

public static class ContentFileHeaderExtensions
{
    public static ContentRepositoryResourceInfo ToContentFileInfoDto(this ContentResourceHeader inHeader, string bucketName)
    {
        return new ContentRepositoryResourceInfo
        {
            BucketName = bucketName,
            ResourcePath = inHeader.ResourcePath,
            StoredLength = inHeader.StoredLength,
            UserDataLong = inHeader.UserDataLong,
            UserDataGuid = inHeader.UserDataGuid,
            Sha256 = inHeader.Sha256,
            OriginalLength = inHeader.OriginalLength,
            UpdateDateTime = inHeader.UpdatedDateTime
        };
    }
}