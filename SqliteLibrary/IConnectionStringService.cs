namespace SqliteLibrary
{
    public interface IConnectionStringService
    {
        string? GetConnectionString();
        void SetConnectionString(string connectionString);
    }
}