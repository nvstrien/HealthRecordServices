namespace SnomedToSQLite.Menu
{
    public class ExitOption : IMenuOption
    {
        public string Description => "Exit";

        public Task<bool> ExecuteAsync()
        {
            // No operation needed
            return Task.FromResult(true);
        }
    }

}
