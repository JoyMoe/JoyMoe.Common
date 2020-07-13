namespace JoyMoe.Common.Storage.QCloud
{
    public class QCloudStorageOptions
    {
        public string SecretId { get; set; } = null!;

        public string SecretKey { get; set; } = null!;

        public string Region { get; set; } = null!;

        public string BucketName { get; set; } = null!;

        public string? CanonicalName { get; set; }

        public bool UseHttps { get; set; } = true;

        public virtual string Endpoint => $"{BucketName}.cos.{Region}.myqcloud.com";
    }
}
