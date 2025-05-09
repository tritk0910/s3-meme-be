namespace s3;

public class PartETagInfo
{
  public int PartNumber { get; set; }
  public string ETag { get; set; } = string.Empty;
}