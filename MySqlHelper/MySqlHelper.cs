using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Data.Common;

#if NET6_0_OR_GREATER
using Microsoft.EntityFrameworkCore;
#endif

namespace MySqlHelper
{
    public static class MySqlHelper
    {
        // ----------------- LOGGING / PROFILING -----------------
        public static Func<string, Task> QueryLogger { get; set; } = async sql => { /* do nothing */ };
        private static async Task LogQueryAsync(string sql)
        {
            if (QueryLogger != null) await QueryLogger(sql);
        }

        // ----------------- ADO.NET CRUD -----------------
        public static async Task InsertAsync<T>(DbConnection conn, string tableName, T item)
        {
            await InsertAsync(conn, tableName, new[] { item });
        }

        public static async Task InsertAsync<T>(DbConnection conn, string tableName, IEnumerable<T> items)
        {
            if (!items.Any()) return;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = string.Join(",", props.Select(p => $"`{p.Name}`"));

            var valuesList = items.Select(item =>
            {
                var vals = props.Select(p => FormatValue(p.GetValue(item))).ToArray();
                return "(" + string.Join(",", vals) + ")";
            });

            var sql = $"INSERT INTO `{tableName}` ({columns}) VALUES {string.Join(",", valuesList)};";
            await LogQueryAsync(sql);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task<List<T>> SelectAsync<T>(DbConnection conn, string tableName, string whereClause = null) where T : new()
        {
            var sql = $"SELECT * FROM `{tableName}`" + (string.IsNullOrWhiteSpace(whereClause) ? "" : $" WHERE {whereClause}");
            await LogQueryAsync(sql);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = await cmd.ExecuteReaderAsync();
            return reader.MapToList<T>();
        }

        public static async Task UpdateAsync<T>(DbConnection conn, string tableName, T item, string keyColumn)
        {
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                                 .Where(p => !string.Equals(p.Name, keyColumn, StringComparison.OrdinalIgnoreCase));

            var setClause = string.Join(",", props.Select(p => $"`{p.Name}`={FormatValue(p.GetValue(item))}"));
            var keyVal = typeof(T).GetProperty(keyColumn).GetValue(item);

            var sql = $"UPDATE `{tableName}` SET {setClause} WHERE `{keyColumn}`={FormatValue(keyVal)};";
            await LogQueryAsync(sql);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task DeleteAsync(DbConnection conn, string tableName, string keyColumn, object keyValue)
        {
            var sql = $"DELETE FROM `{tableName}` WHERE `{keyColumn}`={FormatValue(keyValue)};";
            await LogQueryAsync(sql);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }

        public static async Task UpsertAsync<T>(DbConnection conn, string tableName, IEnumerable<T> items, string keyColumn)
        {
            if (!items.Any()) return;

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var columns = string.Join(",", props.Select(p => $"`{p.Name}`"));

            var valuesList = items.Select(item =>
            {
                var vals = props.Select(p => FormatValue(p.GetValue(item))).ToArray();
                return "(" + string.Join(",", vals) + ")";
            });

            var updateList = string.Join(",", props
                .Where(p => !string.Equals(p.Name, keyColumn, StringComparison.OrdinalIgnoreCase))
                .Select(p => $"`{p.Name}`=VALUES(`{p.Name}`)"));

            var sql = $"INSERT INTO `{tableName}` ({columns}) VALUES {string.Join(",", valuesList)} ON DUPLICATE KEY UPDATE {updateList};";
            await LogQueryAsync(sql);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            await cmd.ExecuteNonQueryAsync();
        }

        // ----------------- TRANSACTION -----------------
        public static async Task RunTransactionAsync(DbConnection conn, Func<DbTransaction, Task> action)
        {
            using var tx = conn.BeginTransaction();
            try
            {
                await action(tx);
                tx.Commit();
            }
            catch
            {
                tx.Rollback();
                throw;
            }
        }

        // ----------------- PAGINATION -----------------
        public static async Task<List<T>> SelectPagedAsync<T>(DbConnection conn, string tableName, string whereClause = null, string orderBy = null, int page = 1, int pageSize = 50) where T : new()
        {
            int offset = (page - 1) * pageSize;
            var sql = $"SELECT * FROM `{tableName}`" +
                      (string.IsNullOrWhiteSpace(whereClause) ? "" : $" WHERE {whereClause}") +
                      (string.IsNullOrWhiteSpace(orderBy) ? "" : $" ORDER BY {orderBy}") +
                      $" LIMIT {pageSize} OFFSET {offset};";

            await LogQueryAsync(sql);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = await cmd.ExecuteReaderAsync();
            return reader.MapToList<T>();
        }

        // ----------------- ASYNC STREAMING -----------------
#if NET6_0_OR_GREATER
        public static async IAsyncEnumerable<T> StreamAsync<T>(DbConnection conn, string tableName, string whereClause = null) where T : new()
        {
            var sql = $"SELECT * FROM `{tableName}`" + (string.IsNullOrWhiteSpace(whereClause) ? "" : $" WHERE {whereClause}");
            await LogQueryAsync(sql);

            using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            using var reader = await cmd.ExecuteReaderAsync();

            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            while (await reader.ReadAsync())
            {
                var obj = new T();
                foreach (var prop in props)
                {
                    if (!reader.HasColumn(prop.Name) || reader[prop.Name] is DBNull) continue;
                    prop.SetValue(obj, reader[prop.Name]);
                }
                yield return obj;
            }
        }
#else
        public static async Task<List<T>> StreamAsync<T>(DbConnection conn, string tableName, string whereClause = null) where T : new()
        {
            return await SelectAsync<T>(conn, tableName, whereClause);
        }
#endif

        // ----------------- EF CORE INTEGRATION -----------------
#if NET6_0_OR_GREATER
        public static async Task InsertEFAsync<T>(DbContext context, T entity) where T : class
        {
            context.Set<T>().Add(entity);
            await context.SaveChangesAsync();
        }

        public static async Task InsertEFAsync<T>(DbContext context, IEnumerable<T> entities) where T : class
        {
            context.Set<T>().AddRange(entities);
            await context.SaveChangesAsync();
        }

        public static async Task UpdateEFAsync<T>(DbContext context, T entity) where T : class
        {
            context.Set<T>().Update(entity);
            await context.SaveChangesAsync();
        }

        public static async Task DeleteEFAsync<T>(DbContext context, T entity) where T : class
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }

        public static IQueryable<T> QueryEF<T>(DbContext context) where T : class
        {
            return context.Set<T>();
        }
#endif

        // ----------------- HELPERS -----------------
        private static string FormatValue(object value)
        {
            if (value == null) return "NULL";
            return value switch
            {
                string s => $"'{s.Replace("'", "''")}'",
                DateTime dt => $"'{dt:yyyy-MM-dd HH:mm:ss}'",
                bool b => b ? "1" : "0",
                _ => value.ToString()
            };
        }

        private static List<T> MapToList<T>(this DbDataReader reader) where T : new()
        {
            var list = new List<T>();
            var props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);
            while (reader.Read())
            {
                var obj = new T();
                foreach (var prop in props)
                {
                    if (!reader.HasColumn(prop.Name) || reader[prop.Name] is DBNull) continue;
                    prop.SetValue(obj, reader[prop.Name]);
                }
                list.Add(obj);
            }
            return list;
        }

        private static bool HasColumn(this DbDataReader reader, string columnName)
        {
            for (int i = 0; i < reader.FieldCount; i++)
                if (reader.GetName(i).Equals(columnName, StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }
    }
}
