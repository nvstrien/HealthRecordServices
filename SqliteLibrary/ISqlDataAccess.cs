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