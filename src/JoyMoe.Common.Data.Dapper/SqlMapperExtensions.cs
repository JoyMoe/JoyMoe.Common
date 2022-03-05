using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using JoyMoe.Common.Data.Dapper;

namespace Dapper.Contrib
{
    /// <summary>
    /// The Dapper.Contrib extensions for Dapper
    ///
    /// codes from https://github.com/StackExchange/Dapper/tree/main/Dapper.Contrib
    /// the Apache 2.0 License
    /// </summary>
    public static class SqlMapperExtensions
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IList<PropertyInfo>> KeyProperties   = new();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IList<PropertyInfo>> TypeProperties  = new();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, PropertyInfo?>       VersionProperty = new();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, string>              TypeTableName   = new();

        private static readonly ISqlAdapter DefaultAdapter = new SqlServerAdapter();

        private static readonly Dictionary<string, ISqlAdapter> AdapterDictionary = new(6)
        {
            ["sqlconnection"]    = new SqlServerAdapter(),
            ["sqlceconnection"]  = new SqlCeServerAdapter(),
            ["npgsqlconnection"] = new PostgresAdapter(),
            ["sqliteconnection"] = new SQLiteAdapter(),
            ["mysqlconnection"]  = new MySqlAdapter(),
            ["fbconnection"]     = new FbAdapter()
        };

        public static Task<IEnumerable<T>> QueryAsync<T>(
            this IDbConnection           connection,
            Expression<Func<T, bool>>?   predicate,
            Dictionary<string, string?>? orderings   = null,
            int?                         size        = null,
            IDbTransaction?              transaction = null,
            int?                         timeout     = null,
            ISqlAdapter?                 adapter     = null) where T : class
        {
            adapter ??= GetFormatter(connection);

            var (sb, parameters) = BuildQuery(predicate, adapter);

            if (orderings != null)
            {
                sb.Append(" ORDER BY ");
                foreach (var (column, modifier) in orderings)
                {
                    adapter.AppendColumnName(sb, column);
                    if (!string.IsNullOrWhiteSpace(modifier))
                    {
                        sb.AppendFormat(" {0}", modifier);
                    }
                }
            }

            if (size.HasValue)
            {
                sb.AppendFormat(" LIMIT {0}", size);
            }

            return connection.QueryAsync<T>(sb.ToString(), parameters, transaction, timeout);
        }

        public static Task<T> QueryFirstOrDefaultAsync<T>(
            this IDbConnection         connection,
            Expression<Func<T, bool>>? predicate,
            IDbTransaction?            transaction = null,
            int?                       timeout     = null, ISqlAdapter? adapter = null)
            where T : class
        {
            adapter ??= GetFormatter(connection);

            var (sb, parameters) = BuildQuery(predicate, adapter);

            return connection.QueryFirstOrDefaultAsync<T>(sb.ToString(), parameters, transaction, timeout);
        }

        public static Task<T> QuerySingleOrDefaultAsync<T>(
            this IDbConnection         connection,
            Expression<Func<T, bool>>? predicate,
            IDbTransaction?            transaction = null,
            int?                       timeout     = null, ISqlAdapter? adapter = null)
            where T : class
        {
            adapter ??= GetFormatter(connection);

            var (sb, parameters) = BuildQuery(predicate, adapter);

            return connection.QuerySingleOrDefaultAsync<T>(sb.ToString(), parameters, transaction, timeout);
        }

        public static Task<TResult> CountAsync<T, TResult>(
            this IDbConnection connection, Expression<Func<T, bool>>? predicate,
            IDbTransaction?    transaction = null,
            int?               timeout     = null, ISqlAdapter? adapter = null) where T : class
        {
            adapter ??= GetFormatter(connection);

            var (sb, parameters) = BuildQuery(predicate, adapter);

            sb.Remove(0, 8);

            return connection.QueryFirstOrDefaultAsync<TResult>(
                $"SELECT COUNT(*) {sb}",
                parameters,
                transaction,
                timeout
            );
        }

        private static (StringBuilder, DynamicParameters?) BuildQuery<T>(
            Expression<Func<T, bool>>? predicate, ISqlAdapter adapter)
        {
            var type = typeof(T);

            var translator = new ExpressionTranslator(adapter);

            var name = GetTableName(type!);

            var (clause, parameters) = translator.Translate(predicate);

            var sb = new StringBuilder("SELECT * FROM ");

            adapter.AppendColumnName(sb, name);

            if (!string.IsNullOrWhiteSpace(clause))
            {
                sb.AppendFormat(" WHERE {0}", clause);
            }

            return (sb, parameters);
        }

        public static Task<int> InsertAsync<T>(
            this IDbConnection connection, T entityToInsert,
            IDbTransaction?    transaction = null,
            int?               timeout     = null, ISqlAdapter? adapter = null) where T : class
        {
            var type = typeof(T);
            adapter ??= GetFormatter(connection);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                var implementsGenericIEnumerableOrIsGenericIEnumerable =
                    typeInfo.ImplementedInterfaces.Any(
                        ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsGenericIEnumerableOrIsGenericIEnumerable)
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            var name = GetTableName(type!);

            var allProperties = TypePropertiesCache(type!);

            //insert list of entities
            var sb = new StringBuilder("INSERT INTO ");

            adapter.AppendColumnName(sb, name);

            sb.Append(" (");
            for (var i = 0; i < allProperties.Count; i++)
            {
                var property = allProperties[i];
                adapter.AppendColumnName(sb, property.Name);
                if (i < allProperties.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(") VALUES (");
            for (var i = 0; i < allProperties.Count; i++)
            {
                var property = allProperties[i];
                sb.AppendFormat("@{0}", property.Name);
                if (i < allProperties.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(')');

            return connection.ExecuteAsync(sb.ToString(), entityToInsert, transaction, timeout);
        }

        public static async Task<int> UpdateAsync<T>(
            this IDbConnection connection,         T    entityToUpdate,
            IDbTransaction?    transaction = null, int? timeout = null,
            ISqlAdapter?       adapter     = null) where T : class
        {
            var type = typeof(T);
            adapter ??= GetFormatter(connection);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                var implementsGenericIEnumerableOrIsGenericIEnumerable =
                    typeInfo.ImplementedInterfaces.Any(
                        ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsGenericIEnumerableOrIsGenericIEnumerable)
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            var keyProperties = KeyPropertiesCache(type!);
            if (keyProperties.Count == 0)
            {
                throw new ArgumentException("Entity must have at least one [Key]");
            }

            var name = GetTableName(type!);

            var sb = new StringBuilder("UPDATE ");
            adapter.AppendColumnName(sb, name);
            sb.Append(" SET ");

            var versionProperty = VersionPropertyCache(type!);
            if (versionProperty != null)
            {
                adapter.AppendColumnName(sb, versionProperty.Name);
                sb.AppendFormat(" = \'{0}\', ", Guid.NewGuid());

                keyProperties.Add(versionProperty);
            }

            var allProperties   = TypePropertiesCache(type!);
            var nonIdProperties = allProperties.Except(keyProperties).ToList();

            for (var i = 0; i < nonIdProperties.Count; i++)
            {
                var property = nonIdProperties[i];
                adapter.AppendColumnNameEqualsValue(sb, property.Name);
                if (i < nonIdProperties.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(" WHERE ");

            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties[i];
                adapter.AppendColumnNameEqualsValue(sb, property.Name);
                if (i < keyProperties.Count - 1)
                {
                    sb.Append(" AND ");
                }
            }

            return await connection
                        .ExecuteAsync(sb.ToString(), entityToUpdate, commandTimeout: timeout, transaction: transaction)
                        .ConfigureAwait(false);
        }

        public static async Task<int> DeleteAsync<T>(
            this IDbConnection connection,         T    entityToDelete,
            IDbTransaction?    transaction = null, int? timeout = null,
            ISqlAdapter?       adapter     = null) where T : class
        {
            if (entityToDelete == null)
            {
                throw new ArgumentException("Cannot Delete null Object", nameof(entityToDelete));
            }

            var type = typeof(T);
            adapter ??= GetFormatter(connection);

            if (type.IsArray)
            {
                type = type.GetElementType();
            }
            else if (type.IsGenericType)
            {
                var typeInfo = type.GetTypeInfo();
                var implementsGenericIEnumerableOrIsGenericIEnumerable =
                    typeInfo.ImplementedInterfaces.Any(
                        ti => ti.IsGenericType && ti.GetGenericTypeDefinition() == typeof(IEnumerable<>)) ||
                    typeInfo.GetGenericTypeDefinition() == typeof(IEnumerable<>);

                if (implementsGenericIEnumerableOrIsGenericIEnumerable)
                {
                    type = type.GetGenericArguments()[0];
                }
            }

            var keyProperties = KeyPropertiesCache(type!);
            if (keyProperties.Count == 0)
            {
                throw new ArgumentException("Entity must have at least one [Key]");
            }

            var name = GetTableName(type!);

            var sb = new StringBuilder("DELETE FROM ");
            adapter.AppendColumnName(sb, name);
            sb.Append(" WHERE ");

            for (var i = 0; i < keyProperties.Count; i++)
            {
                var property = keyProperties[i];
                adapter.AppendColumnNameEqualsValue(sb, property.Name);
                if (i < keyProperties.Count - 1)
                {
                    sb.Append(" AND ");
                }
            }

            return await connection.ExecuteAsync(sb.ToString(), entityToDelete, transaction, timeout)
                                   .ConfigureAwait(false);
        }

        private static IList<PropertyInfo> KeyPropertiesCache(Type type)
        {
            if (KeyProperties.TryGetValue(type.TypeHandle, out var pi))
            {
                return pi;
            }

            var allProperties = TypePropertiesCache(type);
            var keyProperties = allProperties.Where(p => p.HasCustomAttribute<KeyAttribute>(true)).ToList();

            if (keyProperties.Count == 0)
            {
                var idProperty =
                    allProperties.FirstOrDefault(
                        p => string.Equals(p.Name, "id", StringComparison.InvariantCultureIgnoreCase));
                if (idProperty != null)
                {
                    keyProperties.Add(idProperty);
                }
            }

            KeyProperties[type.TypeHandle] = keyProperties;
            return keyProperties;
        }

        private static IList<PropertyInfo> TypePropertiesCache(Type type)
        {
            if (TypeProperties.TryGetValue(type.TypeHandle, out var pis))
            {
                return pis;
            }

            var properties = type.GetProperties().Where(IsNotVirtual).ToList();
            TypeProperties[type.TypeHandle] = properties;
            return properties;
        }

        private static PropertyInfo? VersionPropertyCache(Type type)
        {
            if (VersionProperty.TryGetValue(type.TypeHandle, out var pi))
            {
                return pi;
            }

            var allProperties   = TypePropertiesCache(type);
            var versionProperty = allProperties.FirstOrDefault(p => p.HasCustomAttribute<TimestampAttribute>(true));

            VersionProperty[type.TypeHandle] = versionProperty;
            return versionProperty;
        }

        private static bool IsNotVirtual(PropertyInfo property)
        {
            if (!property.CanRead) return false;

            var getter = property.GetGetMethod();
            if (getter == null) return false;

            return !property.HasCustomAttribute<NotMappedAttribute>(false);
        }

        private static string GetTableName(Type type)
        {
            if (TypeTableName.TryGetValue(type.TypeHandle, out var name)) return name;

            var tableAttrName = type.GetCustomAttribute<TableAttribute>(false)?.Name;

            if (tableAttrName != null)
            {
                name = tableAttrName;
            }
            else
            {
                name = KeyPropertiesCache(type).Count == 1
                    ? type.Name.Pluralize()
                    : type.Name;
                if (type.IsInterface && name.StartsWith("I"))
                {
                    name = name.Substring(1);
                }
            }

            TypeTableName[type.TypeHandle] = name;
            return name;
        }

        private static ISqlAdapter GetFormatter(IDbConnection connection)
        {
            var name = connection.GetType().Name.ToLower();

            return AdapterDictionary.TryGetValue(name, out var adapter)
                ? adapter
                : DefaultAdapter;
        }
    }
}

public interface ISqlAdapter
{
    void AppendColumnName(StringBuilder sb, string columnName);

    void AppendColumnNameEqualsValue(StringBuilder sb, string columnName);
}

public class SqlServerAdapter : ISqlAdapter
{
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}]", columnName);
    }

    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}] = @{1}", columnName, columnName);
    }
}

public class SqlCeServerAdapter : ISqlAdapter
{
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}]", columnName);
    }

    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("[{0}] = @{1}", columnName, columnName);
    }
}

public class MySqlAdapter : ISqlAdapter
{
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("`{0}`", columnName);
    }

    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("`{0}` = @{1}", columnName, columnName);
    }
}

public class PostgresAdapter : ISqlAdapter
{
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\"", columnName);
    }

    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\" = @{1}", columnName, columnName);
    }
}

public class SQLiteAdapter : ISqlAdapter
{
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\"", columnName);
    }

    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("\"{0}\" = @{1}", columnName, columnName);
    }
}

public class FbAdapter : ISqlAdapter
{
    public void AppendColumnName(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("{0}", columnName);
    }

    public void AppendColumnNameEqualsValue(StringBuilder sb, string columnName)
    {
        sb.AppendFormat("{0} = @{1}", columnName, columnName);
    }
}
