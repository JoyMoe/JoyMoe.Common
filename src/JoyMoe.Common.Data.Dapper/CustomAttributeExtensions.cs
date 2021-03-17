// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    internal static class CustomAttributeExtensions
    {
        public static bool HasCustomAttribute<T>(this MemberInfo element, bool inherit) where T : Attribute
        {
            var attributes = Attribute.GetCustomAttributes(element, typeof(T), inherit);

            return attributes.Length > 0;
        }
    }
}
