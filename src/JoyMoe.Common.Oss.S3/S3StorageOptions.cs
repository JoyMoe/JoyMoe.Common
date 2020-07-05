namespace JoyMoe.Common.Oss.S3
{
    public class S3StorageOptions
    {
        public string AccessKey { get; set; } = null!;

        public string SecretKey { get; set; } = null!;

        public string Region { get; set; } = null!;

        public string BucketName { get; set; } = null!;

        public bool UseCname { get; set; } = false;

        public bool UseHttps { get; set; } = false;
    }
}
