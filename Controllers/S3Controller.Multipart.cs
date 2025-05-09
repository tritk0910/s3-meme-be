using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;

namespace s3.Controllers;

public partial class S3Controller
{
  [HttpPost("start-multipart")]
  public async Task<IResult> InitiateMultipartUpload([FromQuery] string fileName, [FromQuery] string contentType)
  {
    try
    {
      var key = Guid.NewGuid();
      var request = new InitiateMultipartUploadRequest
      {
        BucketName = s3Settings.Value.BucketName,
        Key = $"{key}",
        ContentType = contentType,
        Metadata =
            {
                ["file-name"] = fileName
            }
      };

      var response = await s3Client.InitiateMultipartUploadAsync(request);

      return Results.Ok(new { key, uploadId = response.UploadId });
    }
    catch (AmazonS3Exception ex)
    {
      return Results.BadRequest($"S3 error starting multipart upload: {ex.Message}");
    }
  }

  [HttpPost("multipart/{key}/presigned-part")]
  public IResult UploadPart(string key, string uploadId, int partNumber)
  {
    try
    {
      var request = new GetPreSignedUrlRequest
      {
        BucketName = s3Settings.Value.BucketName,
        Key = $"{key}",
        Verb = HttpVerb.PUT,
        Expires = DateTime.UtcNow.AddMinutes(15),
        UploadId = uploadId,
        PartNumber = partNumber
      };

      string preSignedUrl = s3Client.GetPreSignedURL(request);

      return Results.Ok(new { key, url = preSignedUrl });
    }
    catch (AmazonS3Exception ex)
    {
      return Results.BadRequest($"S3 error generating pre-signed URL for part: {ex.Message}");
    }
  }

  [HttpPost("multipart/{key}/complete")]
  public async Task<IResult> CompleteMultipartUpload(string key, CompleteMultipartUpload complete)
  {
    try
    {
      var request = new CompleteMultipartUploadRequest
      {
        BucketName = s3Settings.Value.BucketName,
        Key = $"{key}",
        UploadId = complete.UploadId,
        PartETags = complete.Parts.Select(p => new PartETag(p.PartNumber, p.ETag)).ToList()
      };

      var response = await s3Client.CompleteMultipartUploadAsync(request);

      return Results.Ok(new { key, location = response.Location });
    }
    catch (AmazonS3Exception ex)
    {
      return Results.BadRequest($"S3 error completing multipart upload: {ex.Message}");
    }
  }
}