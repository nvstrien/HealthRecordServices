namespace SnomedToSQLite.Menu
{
    public class MenuUI
    {
        private readonly IEnumerable<IMenuOption> _menuOptions;

        public MenuUI(IEnumerable<IMenuOption> menuOptions)
        {
            _menuOptions = menuOptions ?? throw new ArgumentNullException(nameof(menuOptions));
        }

        public async Task Run()
        {
            while (true)
            {
                Console.WriteLine("Please select an option:");

                int i = 1;
                foreach (var option in _menuOptions)
                {
                    Console.WriteLine($"{i++}. {option.Description}");
                }

                if (!int.TryParse(Console.ReadLine(), out var selectedOption) || selectedOption < 1 || selectedOption > _menuOptions.Count())
                {
                    Console.WriteLine("Invalid selection. Please try again.");
                    continue;
                }

                var menuOption = _menuOptions.ElementAt(selectedOption - 1);

                if (await menuOption.ExecuteAsync())
                {
                    break;
                }
            }
        }
    }


}
