// ReSharper disable once CheckNamespace
namespace System.Reflection
{
    internal static class PropertyInfoExtensions
    {
        public static bool IsVirtual(this PropertyInfo method)
        {
            if (!method.CanRead) return false;

            var getter = method.GetGetMethod();
            if (getter == null) return false;

            return getter.IsVirtual && !getter.IsFinal;
        }
    }
}
