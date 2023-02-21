using System.Reflection;
using System.Text.RegularExpressions;
using JoyMoe.Common.Abstractions;

namespace JoyMoe.Common.Data;

public static class RepositoryTraits
{
    public static class ConcurrencyTrait
    {
        public static Task OnBeforeUpdateAsync<TEntity>(TEntity entity, CancellationToken ct = default) {
            if (entity is not IConcurrency concurrency) return Task.CompletedTask;

            concurrency.Timestamp = Guid.NewGuid();

            return Task.CompletedTask;
        }
    }

    public static class CanonicalNameTrait
    {
        private static readonly Regex ResourceNameRegex = new(@"\{(?<name>\w+)\}");

        private static readonly Dictionary<Type, Dictionary<string, PropertyInfo>> PropertiesCache = new();

        public static Task OnBeforeAddAsync<TEntity>(TEntity entity, CancellationToken ct = default) {
            if (entity is not ICanonicalName named) return Task.CompletedTask;

            var type = entity.GetType();

            var attribute = type.GetCustomAttribute<ResourceNameAttribute>(false);
            if (attribute == null) throw new InvalidOperationException();

            if (!PropertiesCache.TryGetValue(type, out var properties)) {
                properties = type.GetProperties(BindingFlags.GetProperty |
                                                BindingFlags.IgnoreCase |
                                                BindingFlags.Public |
                                                BindingFlags.Instance).ToDictionary(p => p.Name, p => p);

                PropertiesCache.Add(type, properties);
            }

            var name = ResourceNameRegex.Replace(attribute.ResourceName, m => {
                var matched = m.Groups["name"].Value;
                matched = $"{char.ToUpper(matched[0])}{matched[1..]}";
                if (string.Equals(type.Name, matched)) {
                    matched = string.Empty;
                }

                if (!properties.TryGetValue($"{matched}Name", out var property)) {
                    throw new MissingFieldException(type.Name, $"{matched}Name");
                }

                return property.GetValue(entity)?.ToString() ?? string.Empty;
            });

            named.CanonicalName = name;

            return Task.CompletedTask;
        }
    }

    public static class TimestampTrait
    {
        public static Task OnBeforeAddAsync<TEntity>(TEntity entity, CancellationToken ct = default) {
            if (entity is not ITimestamp stamp) return Task.CompletedTask;

            stamp.CreationDate     = DateTime.UtcNow;
            stamp.ModificationDate = DateTime.UtcNow;

            return Task.CompletedTask;
        }

        public static Task OnBeforeUpdateAsync<TEntity>(TEntity entity, CancellationToken ct = default) {
            if (entity is not ITimestamp stamp) return Task.CompletedTask;

            stamp.ModificationDate = DateTime.UtcNow;

            return Task.CompletedTask;
        }
    }
}
