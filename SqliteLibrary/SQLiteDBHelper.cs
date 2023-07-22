using System.Data.SQLite;
using System.Reflection;

namespace SqliteLibrary
{
    public static class SQLiteDBHelper
    {
        public static void CreateSQLiteDatabase(string fileName, params Type[] tableTypes)
        {
            SQLiteConnection.CreateFile(fileName);

            using var connection = new SQLiteConnection($"Data Source={fileName};Version=3;");

            connection.Open();

            using (var transaction = connection.BeginTransaction())
            {
                foreach (Type type in tableTypes)
                {
                    string createTableSql = GetCreateTableSql(type);

                    using var command = new SQLiteCommand(createTableSql, connection);

                    command.ExecuteNonQuery();
                }

                transaction.Commit();
            }

            connection.Close();
        }

        public static string GetCreateTableSql(Type type)
        {
            string tableName = type.Name;

            if (tableName.Contains("Model", StringComparison.OrdinalIgnoreCase))
            {
                tableName = tableName.Replace("Model", "", StringComparison.OrdinalIgnoreCase);
            }

            string createTableSql = $"CREATE TABLE {tableName} (";

            foreach (PropertyInfo property in type.GetProperties())
            {
                string columnName = $"'{property.Name}'";
                string columnType = GetSqliteType(property.PropertyType);

                createTableSql += $"{columnName} {columnType}, ";
            }
            createTableSql = createTableSql.TrimEnd(',', ' ') + ")"; // Remove the trailing comma and space

            return createTableSql;
        }

        private static string GetSqliteType(Type type)
        {
            if (type == typeof(int) || type == typeof(long) || type == typeof(short) || type == typeof(byte) || type == typeof(sbyte) || type == typeof(bool))
            {
                return "INTEGER";
            }

            if (type == typeof(float) || type == typeof(double))
            {
                return "REAL";
            }

            if (type == typeof(DateTime))
            {
                return "TEXT";
            }

            return "TEXT";
        }


    }
}
