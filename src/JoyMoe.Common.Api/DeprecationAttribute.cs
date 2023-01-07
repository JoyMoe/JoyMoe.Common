namespace JoyMoe.Common.Api;

[AttributeUsage(AttributeTargets.Method)]
public class DeprecationAttribute : Attribute
{
    public DateTimeOffset? DeprecationDate { get; set; }

    public string? Documentation { get; set; }

    public string[]? SuccessorVersions { get; set; }

    public string? LatestVersion { get; set; }

    public string[]? Alternates { get; set; }

    public DeprecationAttribute() { }

    public DeprecationAttribute(DateTimeOffset deprecationDate) {
        DeprecationDate = deprecationDate;
    }

    public bool IsDeprecated => !DeprecationDate.HasValue || DeprecationDate.Value < DateTimeOffset.Now;
}
