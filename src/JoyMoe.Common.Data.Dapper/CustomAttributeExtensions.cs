// ReSharper disable once CheckNamespace

namespace System.Reflection;

internal static class CustomAttributeExtensions
{
    public static bool HasCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute {
        return Attribute.IsDefined(element, typeof(T), inherit);
    }
}
