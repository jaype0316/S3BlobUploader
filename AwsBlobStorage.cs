using Amazon.S3.Model;
using Amazon.S3.Util;
using Amazon.S3;

public class AwsBlobStorage 
{
    readonly IAmazonS3 _client;
    readonly string _bucket;
    readonly bool _isPublicReadAccess;

    public AwsBlobStorage(IAmazonS3 client, string bucket, bool publicReadAccess =false)
    {
        _client = client;
        _bucket = bucket;
        _isPublicReadAccess = publicReadAccess;
    }

    public async Task<bool> Add(string key, Stream stream)
    {
        await AssertBucketExists();
        var putObjectRequest = new PutObjectRequest()
        {
            BucketName = _bucket,
            Key = key,
            InputStream = stream
        };
        var response = await _client.PutObjectAsync(putObjectRequest);

        return response.HttpStatusCode == System.Net.HttpStatusCode.OK;
    }

    private async Task AssertBucketExists()
    {
        var bucketExists = await AmazonS3Util.DoesS3BucketExistV2Async(_client, _bucket);
        if (!bucketExists)
        {
            var putBucketRequest = new PutBucketRequest()
            {
                BucketName = _bucket,
                UseClientRegion = true
            };
            await _client.PutBucketAsync(putBucketRequest);

            if(_isPublicReadAccess)
                await SetPublicReadBucketPolicyAsync();
        }
    }

    private async Task SetPublicReadBucketPolicyAsync()
    {
        var bucketPolicy = new
        {
            Version = "2012-10-17",
            Statement = new[]
            {
                new
                {
                    Effect = "Allow",
                    Principal = "*",
                    Action = "s3:GetObject",
                    Resource = $"arn:aws:s3:::{_bucket}/*"
                }
            }
        };

        var putBucketPolicyRequest = new PutBucketPolicyRequest
        {
            BucketName = _bucket,
            Policy = Newtonsoft.Json.JsonConvert.SerializeObject(bucketPolicy)
        };

        await _client.PutBucketPolicyAsync(putBucketPolicyRequest);
    }
}