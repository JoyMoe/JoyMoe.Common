namespace JoyMoe.Common.Oss.S3
{
    public class S3StorageOptions
    {
        public string AccessKey { get; set; }

        public string SecretKey { get; set; }

        public string Region { get; set; }

        public string BucketName { get; set; }

        public bool UseCname { get; set; }

        public bool UseHttps { get; set; }
    }
}
