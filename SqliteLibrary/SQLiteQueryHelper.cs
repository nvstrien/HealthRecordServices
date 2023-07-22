
using System.Dynamic;
using System.Reflection;
using System.Text;

namespace SqliteLibrary
{
    public class SQLiteQueryHelper
    {
        #region Insert

        public string GetSQLInsertQuery<T>(string tableName, string primaryKeyName, bool insertPrimaryKey = false) where T : class, new()
        {
            List<string> columns = new();

            PropertyInfo[] propInfos = typeof(T).GetPublicProperties();

            foreach (PropertyInfo property in propInfos)
            {
                if (insertPrimaryKey)
                {
                    columns.Add(property.Name);
                }
                else if (property.Name.Equals(primaryKeyName, StringComparison.Ordinal) == false)
                {
                    columns.Add(property.Name);
                }
            }

            string output = GenerateSQLInsertQuery(tableName, columns);
            return output;
        }


        /// <summary>
        /// Generates a parameterized SQLite insert query.
        /// </summary>
        /// <param name="tableName">The name of the table to insert data into.</param>
        /// <param name="columns">A list of column names to insert data into.</param>
        /// <returns>A parameterized SQLite insert query string.</returns>
        private static string GenerateSQLInsertQuery(string tableName, List<string> columns)
        {
            // Validate the input to ensure the provided tableName and columns are valid.
            if (string.IsNullOrEmpty(tableName) || columns == null || columns.Count == 0)
                throw new ArgumentException("Invalid table name or columns.");

            // Join the column names into a single string.
            List<string> columnsWithQuotes = StringHelper.AddSingleQuotes(columns);
            var columnNames = string.Join(", ", columnsWithQuotes);

            // Create a list of parameter names based on the column names.
            var parameters = new List<string>();
            foreach (var column in columns)
            {
                parameters.Add($"@{column}");
            }

            // Join the parameter names into a single string.
            var parameterNames = string.Join(", ", parameters);

            // Build the parameterized SQLite insert query.
            var query = new StringBuilder();
            query.AppendFormat("INSERT INTO {0} ({1}) VALUES ({2});", tableName, columnNames, parameterNames);

            // Return the generated query.
            return query.ToString();
        }

        #endregion

        #region Update

        public string GetSQLUpdateQuery<T>(string tableName, string primaryKeyName, T instance, IEnumerable<string> whereColumns)
        {
            if (string.IsNullOrEmpty(tableName) || string.IsNullOrEmpty(primaryKeyName) || instance == null || whereColumns == null || !whereColumns.Any())
                throw new ArgumentException("Invalid arguments.");

            PropertyInfo[] properties = typeof(T).GetProperties();
            var updateColumns = new List<string>();

            foreach (PropertyInfo property in properties)
            {
                if (property.Name != primaryKeyName && whereColumns.Contains(property.Name) == false)
                {
                    updateColumns.Add(property.Name);
                }
            }

            string query = GenerateSQLUpdateQuery(tableName, updateColumns, whereColumns);
            return query;
        }

        /// <summary>
        /// Generates a parameterized SQLite update query.
        /// </summary>
        /// <param name="tableName">The name of the table to update data in.</param>
        /// <param name="updateColumns">A list of column names to update data in.</param>
        /// <param name="whereColumns">A list of column names to be used in the WHERE clause.</param>
        /// <returns>A parameterized SQLite update query string.</returns>
        private static string GenerateSQLUpdateQuery(string tableName, IEnumerable<string> updateColumns, IEnumerable<string> whereColumns)
        {
            // Validate the input to ensure the provided tableName and columns are valid.
            if (string.IsNullOrEmpty(tableName) || updateColumns == null || !updateColumns.Any() || whereColumns == null || !whereColumns.Any())
                throw new ArgumentException("Invalid table name or columns.");


            var updateList = updateColumns.ToList();
            var whereList = whereColumns.ToList();

            // Build the SET clause with parameterized column names and values.
            var setClause = new StringBuilder();
            for (int i = 0; i < updateList.Count; i++)
            {
                if (i > 0) setClause.Append(", ");
                setClause.AppendFormat("{0} = @{0}", updateList[i]);
            }

            // Build the WHERE clause with parameterized column names and values.
            var whereClause = new StringBuilder();
            for (int i = 0; i < whereList.Count; i++)
            {
                if (i > 0) whereClause.Append(" AND ");
                whereClause.AppendFormat("{0} = @{0}", whereList[i]);
            }

            // Combine the SET and WHERE clauses to create the parameterized SQLite update query.
            var query = new StringBuilder();
            query.AppendFormat("UPDATE {0} SET {1} WHERE {2};", tableName, setClause.ToString(), whereClause.ToString());

            // Return the generated query.
            return query.ToString();
        }

        #endregion

        #region Getting Data

        public IEnumerable<dynamic> GetModelData<T>(IEnumerable<T> models) where T : class, new()
        {
            List<dynamic> dataList = new();

            foreach (T model in models)
            {
                PropertyInfo[] properties = typeof(T).GetProperties();

                dynamic data = new ExpandoObject();

                foreach (PropertyInfo property in properties)
                {
                    var value = property.GetValue(model);

                    AddProperty(data, property.Name, value);
                }

                dataList.Add(data);
            }

            return dataList;
        }

        public dynamic GetModelData<T>(T model) where T : class, new()
        {
            PropertyInfo[] properties = typeof(T).GetProperties();

            dynamic data = new ExpandoObject();

            foreach (PropertyInfo property in properties)
            {

                var value = property.GetValue(model);

                if (value != null)
                {
                    AddProperty(data, property.Name, value);
                }
                else
                {
                    // adding null property?
                    AddProperty(data, property.Name, value);
                }
            }

            return data;
        }

        private static void AddProperty(ExpandoObject expando, string propertyName, object propertyValue)
        {
            //// ExpandoObject supports IDictionary so we can extend it like this
            //// From: https://www.oreilly.com/content/building-c-objects-dynamically/

            var expandoDict = expando as IDictionary<string, object>;

            // Check if the property already exists in the dictionary.
            if (expandoDict.ContainsKey(propertyName))
            {
                // Update the property value (which can be null).
                expandoDict[propertyName] = propertyValue;
            }
            else
            {
                // Add the property with the specified name and value (which can be null).
                expandoDict.Add(propertyName, propertyValue);
            }
        }

        #endregion
    }
}
