namespace JoyMoe.Common.Storage.S3
{
    public class S3StorageOptions
    {
        public string AccessKey { get; set; } = null!;

        public string SecretKey { get; set; } = null!;

        public string Region { get; set; } = null!;

        public string BucketName { get; set; } = null!;

        public string Endpoint { get; set; } = "s3.amazonaws.com";

        public bool UseCName { get; set; } = false;

        public bool UseHttps { get; set; } = true;
    }
}
