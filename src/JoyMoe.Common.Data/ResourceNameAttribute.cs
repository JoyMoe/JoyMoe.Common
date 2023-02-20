namespace JoyMoe.Common.Data;

[AttributeUsage(AttributeTargets.Class)]
public class ResourceNameAttribute : Attribute
{
    public string ResourceName { get; }

    public ResourceNameAttribute(string name) {
        ResourceName = name;
    }
}
