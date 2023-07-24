using Dapper;


using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using System.Data;
using System.Data.SQLite;

namespace SqliteLibrary
{
    public class SQLiteDataAccess : ISqlDataAccess
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SQLiteDataAccess> _logger;
        private readonly IConnectionStringService _connectionStringService;

        public SQLiteDataAccess(IConfiguration config, ILogger<SQLiteDataAccess> logger, IConnectionStringService connectionStringService)
        {
            _config = config;
            _logger = logger;
            _connectionStringService = connectionStringService;
        }

        /// <summary>
        /// Asynchronously loads data of type T from the database using the provided SQL query, parameters, and connection string.
        /// </summary>
        /// <typeparam name="T">The type of objects to return.</typeparam>
        /// <typeparam name="U">The type of the parameters object.</typeparam>
        /// <param name="sql">The SQL query to execute.</param>
        /// <param name="parameters">The parameters object containing parameter values for the SQL query.</param>
        /// <param name="connectionStringName">The name of the connection string in the configuration.</param>
        /// <returns>A task representing the asynchronous operation, with a result containing an IEnumerable of type T containing the loaded data.</returns>
        /// <exception cref="ArgumentException">Thrown when the connection string is null or empty.</exception>
        /// <exception cref="Exception">Thrown when the SQLite operation fails.</exception>
        public async Task<IEnumerable<T>> LoadData<T, U>(string sql, U parameters, string connectionStringName)
        {
            try
            {
                // Retrieve the connection string from the configuration.
                //string? connectionString = _config.GetConnectionString(connectionStringName);
                string? connectionString = _connectionStringService.GetConnectionString();

                // Validate the connection string.
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionStringName));
                }

                // Create a new SQLiteConnection instance.
                using IDbConnection connection = new SQLiteConnection(connectionString);

                // Execute the SQL query asynchronously and store the results.
                var rows = await connection.QueryAsync<T>(sql, parameters, commandType: CommandType.Text).ConfigureAwait(false);

                // Return the results.
                return rows;
            }
            catch (Exception ex)
            {
                // Log the error and throw an exception with the original exception as the inner exception.
                _logger.LogError(ex, "The SQLite operation failed with {query}, {message}", sql, ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

        /// <summary>
        /// Saves data to an SQLite database using the provided SQL command, parameters, and connection string.
        /// </summary>
        /// <typeparam name="T">The type of parameters to use in the SQL command.</typeparam>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="parameters">The parameters to use in the SQL command.</param>
        /// <param name="connectionStringName">The name of the connection string to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous save operation.</returns>
        public async Task SaveData<T>(string sql, T parameters, string connectionStringName)
        {
            try
            {
                // Get the connection string from the configuration.
                //string? connectionString = _config.GetConnectionString(connectionStringName);
                string? connectionString = _connectionStringService.GetConnectionString();

                if (string.IsNullOrEmpty(connectionString))
                {
                    // Throw an ArgumentException if the connection string is null or empty.
                    throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionStringName));
                }

                // Create a new SQLite connection using the connection string.
                using IDbConnection connection = new SQLiteConnection(connectionString);

                // Execute the SQL command asynchronously.
                await connection.ExecuteAsync(sql, parameters, commandType: CommandType.Text).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                // Log the exception and re-throw it.
                _logger.LogError(ex, "The SQLite operation failed with {query}, {message}", sql, ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }


        //public async Task<int> InsertData<T, U>(string sql, U parameters, string connectionStringName) 
        //    where T : class 
        //    where U : T, not IEnumerable
        //{
        //    string? connectionString = _config.GetConnectionString(connectionStringName);

        //    if (string.IsNullOrEmpty(connectionString))
        //    {
        //        throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionStringName));
        //    }

        //    using IDbConnection connection = new SqliteConnection(connectionString);

        //    int result = await connection.ExecuteScalarAsync<int>(sql, parameters, commandType: CommandType.Text);

        //    return result;
        //}

        /// <summary>
        /// Inserts a batch of data into an SQLite database using the provided SQL command, data, and connection string.
        /// </summary>
        /// <typeparam name="T">The type of data to insert.</typeparam>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="data">The data to insert.</param>
        /// <param name="connectionStringName">The name of the connection string to use.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous insert operation, which returns a collection of the IDs of the inserted records.</returns>
        public async Task<IEnumerable<int>> InsertData<T>(string sql, IEnumerable<T> data, string connectionStringName)
        {
            //string? connectionString = _config.GetConnectionString(connectionStringName);
            string? connectionString = _connectionStringService.GetConnectionString();

            if (string.IsNullOrEmpty(connectionString))
            {
                _logger.LogError("InsertData failed: Connection string cannot be null or empty.");
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionStringName));
            }

            using IDbConnection connection = new SQLiteConnection(connectionString);
            connection.Open();

            using IDbTransaction transaction = connection.BeginTransaction();

            try
            {
                List<int> insertedIds = new List<int>();
                int iteration = 0;

                foreach (T item in data)
                {
                    int insertedId = await connection.ExecuteScalarAsync<int>(sql, item, transaction).ConfigureAwait(false);
                    insertedIds.Add(insertedId);
                    iteration++;
                }

                transaction.Commit();

                _logger.LogInformation("Inserted {ItemCount} items into the database using SQL: {Sql}", iteration, sql);

                return insertedIds;
            }
            catch (Exception ex)
            {
                transaction.Rollback();

                _logger.LogError(ex, "InsertData failed: The SQLite operation failed with SQL: {Sql}. Error message: {ErrorMessage}", sql, ex.Message);

                throw new Exception("InsertData failed: The SQLite operation failed.", ex);
            }
        }

        public async Task<bool> DeleteData(string connectionStringName, string tableName, string columnName, int id)
        {
            //string? connectionString = _config.GetConnectionString(connectionStringName);
            string? connectionString = _connectionStringService.GetConnectionString();

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentException("Connection string cannot be null or empty.", nameof(connectionStringName));
            }

            using IDbConnection connection = new SQLiteConnection(connectionString);
            connection.Open();

            using IDbTransaction transaction = connection.BeginTransaction();

            try
            {
                int rowsAffected = await connection.ExecuteAsync($"DELETE FROM {tableName} WHERE {columnName} = @Id;", new { Id = id }, transaction).ConfigureAwait(false);

                transaction.Commit();

                return rowsAffected > 0;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                _logger.LogError(ex, "The SQLite operation failed with message: {message}", ex.Message);
                throw new Exception(ex.Message, ex);
            }
        }

    }
}
