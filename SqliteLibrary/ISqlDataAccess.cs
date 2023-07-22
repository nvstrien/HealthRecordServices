namespace SqliteLibrary
{
    public interface ISqlDataAccess
    {
        /// <summary>
        /// Inserts a batch of data into an SQL database using the provided SQL command, data, and connection string.
        /// </summary>
        /// <typeparam name="T">The type of data to insert.</typeparam>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="data">The data to insert.</param>
        /// <param name="connectionStringName">The name of the connection string to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous insert operation, which returns a collection of the IDs of the inserted records.</returns>
        Task<IEnumerable<int>> InsertData<T>(string sql, IEnumerable<T> data, string connectionStringName);

        /// <summary>
        /// Asynchronously loads data of type T from the database using the provided SQL query, parameters, and connection string.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <typeparam name="U">The type of the parameters object.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters object containing parameter values for the SQL query.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration.</param>
        /// <returns>A task representing the asynchronous operation, with a result containing an IEnumerable of type T containing the loaded data.</returns>
        Task<IEnumerable<T>> LoadData<T, U>(string storedProcedureName, U parameters, string connectionStringName);


        /// <summary>
        /// Saves data to an SQLite database using the provided SQL command, parameters, and connection string.
        /// </summary>
        /// <typeparam name="T">The type of parameters to use in the SQL command.</typeparam>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters to use in the SQL command.</param>
        /// <param name="connectionStringName">The name of the connection string to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous save operation.</returns>
        Task SaveData<T>(string storedProcedure, T parameters, string connectionStringName);
    }
}

//Both QuerySingleOrDefaultAsync and ExecuteScalarAsync are methods provided by Dapper that can be used to execute SQL statements and retrieve a single value from the result set. However, they have some differences in how they work:

//    Return value:

//    QuerySingleOrDefaultAsync returns a single value of a specified type from the result set, or a default value if the result set is empty.
//    ExecuteScalarAsync returns a single value of a specified type from the first row of the first column of the result set, or a default value if the result set is empty.

//    Usage:

//    QuerySingleOrDefaultAsync is typically used when you want to retrieve a single value from a result set that may contain multiple rows or columns. For example, you might use this method to retrieve the count of records that match a certain condition in a table.
//    ExecuteScalarAsync is typically used when you want to retrieve a single value from a specific column in a specific row. For example, you might use this method to retrieve the primary key value of a newly inserted record.

//    Input parameters:

//    QuerySingleOrDefaultAsync takes an SQL statement or stored procedure name, parameters to be passed to the query, and optionally a command type (e.g., Text, StoredProcedure, TableDirect).
//    ExecuteScalarAsync takes an SQL statement or stored procedure name, parameters to be passed to the query, and optionally a command type (e.g., Text, StoredProcedure, TableDirect).

//In general, if you want to retrieve a single value from a result set that may contain multiple rows or columns, use QuerySingleOrDefaultAsync. If you want to retrieve a single value from a specific column in a specific row, use ExecuteScalarAsync.