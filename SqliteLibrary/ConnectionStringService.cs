using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqliteLibrary
{
    public class ConnectionStringService : IConnectionStringService
    {
        private string? _connectionString;

        public void SetConnectionString(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public string? GetConnectionString()
        {
            return _connectionString;
        }
    }
}
