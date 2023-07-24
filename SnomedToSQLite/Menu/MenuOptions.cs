namespace SnomedToSQLite.Menu
{
    public class MenuOptions
    {
        private readonly List<IMenuOption> _menuOptions;

        public MenuOptions(IEnumerable<IMenuOption> menuOptions)
        {
            _menuOptions = menuOptions.ToList();
        }

        public List<IMenuOption> GetOrderedMenuOptions()
        {
            // You may want to add logic to order your menu options here
            return _menuOptions;
        }
    }

}
