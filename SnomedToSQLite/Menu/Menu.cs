using Microsoft.Extensions.Logging;

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
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine(@"SNOMED-2-SQLITE");
                Console.ResetColor();

                Console.WriteLine($"\nPlease select an option:");
                int i = 1;
                foreach (var option in _menuOptions)
                {
                    Console.WriteLine($"{i++}. {option.Description}");
                }
                Console.WriteLine($"{i}. Exit");

                if (!int.TryParse(Console.ReadLine(), out var selectedOption) || selectedOption < 1 || selectedOption > _menuOptions.Count() + 1)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("Invalid selection. Please try again.");
                    Console.ResetColor();
                    continue;
                }

                if (selectedOption == _menuOptions.Count() + 1)
                {
                    Console.WriteLine("Goodbye!");
                    break;
                }

                var menuOption = _menuOptions.ElementAt(selectedOption - 1);
                Console.WriteLine("Processing your selection...");
                // You can add a delay or animation here

                if (await menuOption.ExecuteAsync())
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("Would you like to go the main menu? (Y/N)");
                    Console.ResetColor();
                    var userResponse = Console.ReadLine()?.ToLower();

                    if (userResponse != "y")
                    {
                        Console.WriteLine("Goodbye!");
                        break;
                    }
                }
            }
        }
    }


}
