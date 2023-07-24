namespace SnomedToSQLite.Menu
{
    public interface IMenuOption
    {
        string Description { get; }
        Task<bool> ExecuteAsync();
    }

}
