using System.Runtime.Serialization;

namespace EnterpriseCoder.Marten.ContentRepo.Exceptions;

public class ResourceNotFoundException : Exception
{
    protected ResourceNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
    {
    }

    public ResourceNotFoundException(string bucketName, string resourcePath) : base($"{bucketName}: {resourcePath} was not found")
    {
    }
}