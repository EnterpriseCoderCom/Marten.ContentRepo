using System.Runtime.Serialization;

namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class BucketNotFoundException : Exception
{
    protected BucketNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public BucketNotFoundException(string bucketName) : base(bucketName)
    {
    }
}