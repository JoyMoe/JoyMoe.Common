using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dapper;

namespace JoyMoe.Common.Data.Dapper
{
    public static class DbConnectionExtensions
    {
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> Keys = new();
        private static readonly ConcurrentDictionary<RuntimeTypeHandle, IEnumerable<PropertyInfo>> Properties = new();

        public static async Task<IEnumerable<TEntity>> ListAsync<TEntity>(this DbConnection connection, string? predicate, params object[] values) where TEntity : class
        {
            return await connection.QueryAsync<TEntity>(BuildCommand(BuildSelectSql<TEntity>(predicate), values)).ConfigureAwait(false);
        }

        public static async Task<TEntity?> FirstOrDefaultAsync<TEntity>(this DbConnection connection, string? predicate, params object[] values) where TEntity : class
        {
            return await connection.QueryFirstOrDefaultAsync<TEntity>(BuildCommand(BuildSelectSql<TEntity>(predicate), values)).ConfigureAwait(false);
        }

        public static async Task<TEntity?> SingleOrDefaultAsync<TEntity>(this DbConnection connection, string? predicate, params object[] values) where TEntity : class
        {
            return await connection.QuerySingleOrDefaultAsync<TEntity>(BuildCommand(BuildSelectSql<TEntity>(predicate), values)).ConfigureAwait(false);
        }

        public static async Task<long> CountAsync<TEntity>(this DbConnection connection, string? predicate, params object[] values) where TEntity : class
        {
            return await connection.ExecuteScalarAsync<long>(BuildCommand(BuildSelectSql<TEntity>(predicate), values)).ConfigureAwait(false);
        }

        public static async Task<int> InsertAsync<TEntity>(this DbConnection connection, TEntity entity, IDbTransaction? transaction) where TEntity : class
        {
            var type = typeof(TEntity);

            var table = type.Name.Pluralize();

            var i = 0;

            var columnsBuilder = new StringBuilder();
            var fieldsBuilder = new StringBuilder();
            var values = new List<object?>();

            foreach (var property in GetProperties(type))
            {
                columnsBuilder.Append($"@{property.Name}");
                columnsBuilder.Append(" , ");

                fieldsBuilder.Append($"{{{i}}}");
                fieldsBuilder.Append(" , ");

                values.Add(property.GetValue(entity));

                i++;
            }

            columnsBuilder.Remove(columnsBuilder.Length - 3, 3);
            fieldsBuilder.Remove(fieldsBuilder.Length - 3, 3);

            var columns = columnsBuilder.ToString();
            var fields = fieldsBuilder.ToString();

            var sql = $"INSERT INTO {table.Escape()} ( {columns} ) VALUES ( {fields} )";

            return await connection.ExecuteAsync(BuildCommand(sql, values, transaction)).ConfigureAwait(false);
        }

        public static async Task<int> BulkInsertAsync<TEntity>(this DbConnection connection, IEnumerable<TEntity> entities, IDbTransaction? transaction) where TEntity : class
        {
            if (entities == null)
            {
                throw new ArgumentNullException(nameof(entities));
            }

            var type = typeof(TEntity);

            var table = type.Name.Pluralize();

            var columnsBuilder = new StringBuilder();
            foreach (var property in GetProperties(type))
            {
                columnsBuilder.Append($"@{property.Name}");
                columnsBuilder.Append(" , ");
            }

            columnsBuilder.Remove(columnsBuilder.Length - 3, 3);

            var columns = columnsBuilder.ToString();

            var i = 0;
            var values = new List<object?>();
            var fieldsBuilder = new StringBuilder();
            foreach (var entity in entities)
            {
                fieldsBuilder.Append(" ( ");

                foreach (var property in GetProperties(type))
                {
                    fieldsBuilder.Append($"{{{i}}}");
                    fieldsBuilder.Append(" , ");

                    values.Add(property.GetValue(entity));

                    i++;
                }

                fieldsBuilder.Remove(fieldsBuilder.Length - 3, 3);

                fieldsBuilder.Append(" ) ");
                fieldsBuilder.Append(" , ");
            }

            fieldsBuilder.Remove(fieldsBuilder.Length - 3, 3);

            var fields = fieldsBuilder.ToString();

            var sql = $"INSERT INTO {table.Escape()} ( {columns} ) VALUES {fields}";

            return await connection.ExecuteAsync(BuildCommand(sql, values, transaction)).ConfigureAwait(false);
        }

        public static async Task<int> UpdateAsync<TEntity>(this DbConnection connection, TEntity entity, IDbTransaction? transaction) where TEntity : class
        {
            var type = typeof(TEntity);

            var table = type.Name.Pluralize();

            var i = 0;

            var values = new List<object?>();
            var keys = GetKeys(type);

            var fieldsBuilder = new StringBuilder();
            foreach (var property in GetProperties(type).Where(property => keys.All(k => k.Name != property.Name)))
            {
                fieldsBuilder.Append($"@{property.Name} = {{{i}}}");
                fieldsBuilder.Append(" , ");

                values.Add(property.GetValue(entity));

                i++;
            }

            fieldsBuilder.Remove(fieldsBuilder.Length - 3, 3);

            var fields = fieldsBuilder.ToString();

            var predicateBuilder = new StringBuilder();
            foreach (var key in keys)
            {
                predicateBuilder.Append($"@{key.Name} = {{{i}}}");
                predicateBuilder.Append(" AND ");

                values.Add(key.GetValue(entity));

                i++;
            }

            predicateBuilder.Remove(predicateBuilder.Length - 5, 5);

            var predicate = predicateBuilder.ToString();

            if (string.IsNullOrWhiteSpace(predicate))
            {
                return 0;
            }

            var sql = $"UPDATE {table.Escape()} SET {fields} WHERE {predicate}";

            return await connection.ExecuteAsync(BuildCommand(sql, values, transaction)).ConfigureAwait(false);
        }

        public static async Task<int> DeleteAsync<TEntity>(this DbConnection connection, TEntity entity, IDbTransaction? transaction) where TEntity : class
        {
            var type = typeof(TEntity);

            var table = type.Name.Pluralize();

            var i = 0;

            var values = new List<object?>();
            var keys = GetKeys(type);

            var predicateBuilder = new StringBuilder();
            foreach (var key in keys)
            {
                predicateBuilder.Append($"@{key.Name} = {{{i}}}");
                predicateBuilder.Append(" AND ");

                values.Add(key.GetValue(entity));

                i++;
            }

            predicateBuilder.Remove(predicateBuilder.Length - 5, 5);

            var predicate = predicateBuilder.ToString();

            if (string.IsNullOrWhiteSpace(predicate))
            {
                return 0;
            }

            var sql = $"DELETE FROM {table.Escape()} WHERE {predicate}";

            return await connection.ExecuteAsync(BuildCommand(sql, values, transaction)).ConfigureAwait(false);
        }

        private static List<PropertyInfo> GetKeys(Type type)
        {
            if (Keys.TryGetValue(type.TypeHandle, out var cache))
            {
                return cache.ToList();
            }

            var properties = GetProperties(type);
            var keys = properties.Where(p => p.GetCustomAttributes(true).Any(a => a is KeyAttribute)).ToList();

            if (keys.Count == 0)
            {
                var id = properties.Find(p => string.Equals(p.Name, "id", StringComparison.CurrentCultureIgnoreCase));
                if (id != null)
                {
                    keys.Add(id);
                }
            }

            Keys[type.TypeHandle] = keys;

            return keys;
        }

        private static List<PropertyInfo> GetProperties(Type type)
        {
            if (Properties.TryGetValue(type.TypeHandle, out var cache))
            {
                return cache.ToList();
            }

            var properties = type.GetProperties().ToList();
            Properties[type.TypeHandle] = properties;

            return properties;
        }

        private static string BuildSelectSql<TEntity>(string? predicate) where TEntity : class
        {
            var type = typeof(TEntity);

            var table = type.Name.Pluralize();

            return string.IsNullOrWhiteSpace(predicate)
                ? $"SELECT * FROM {table.Escape()}"
                : $"SELECT * FROM {table.Escape()} WHERE {predicate}";
        }

        private static CommandDefinition BuildCommand(string sql, IReadOnlyList<object?> values, IDbTransaction? transaction = null)
        {
            var tokens = new List<string>();
            var parameters = new DynamicParameters();
            foreach (var token in sql.Split(' '))
            {
                if (string.IsNullOrWhiteSpace(token)) continue;

                if (token[0] == '{' && char.IsDigit(token, 1) && token[^1] == '}')
                {
                    var id = int.Parse(token[1..^1], CultureInfo.InvariantCulture);

                    if (id >= values.Count)
                    {
                        throw new ArgumentOutOfRangeException(nameof(sql));
                    }

                    var holder = $"@__p{id}";
                    tokens.Add(holder);
                    parameters.Add(holder, values[id]);
                    continue;
                }

                tokens.Add(token);
            }

            sql = string.Join(' ', tokens).PrepareSql();

            return new CommandDefinition(sql, parameters, transaction, flags: CommandFlags.None);
        }
    }
}
