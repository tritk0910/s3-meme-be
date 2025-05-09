namespace s3;

public class CompleteMultipartUpload
{
    public string UploadId { get; set; } = string.Empty;
    public List<PartETagInfo> Parts { get; set; } = [];
}
