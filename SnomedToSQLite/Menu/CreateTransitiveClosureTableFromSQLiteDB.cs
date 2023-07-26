using Microsoft.Extensions.Logging;

using ShellProgressBar;

using SnomedToSQLite.Menu.ConvertRf2ToSQLite;
using SnomedToSQLite.Services;

using SqliteLibrary;

namespace SnomedToSQLite.Menu
{
    internal class CreateTransitiveClosureTableFromSQLiteDB : IMenuOption
    {
        private readonly IConversionHelper _conversionHelper;
        private readonly ILogger<CreateTransitiveClosureTableFromSQLiteDB> _logger;
        private readonly IConvertRf2ToSQLiteRunner _runner;
        private readonly IConnectionStringService _connectionStringService;

        public CreateTransitiveClosureTableFromSQLiteDB(
            IConversionHelper conversionHelper,
            ILogger<CreateTransitiveClosureTableFromSQLiteDB> logger,
            IConvertRf2ToSQLiteRunner runner,
            IConnectionStringService connectionStringService)
        {
            _conversionHelper = conversionHelper;
            _logger = logger;
            _runner = runner;
            _connectionStringService = connectionStringService;
        }

        public string Description => "Create a transitive closure table for the snomed snapshot DB";

        public async Task<bool> ExecuteAsync()
        {
            while (true)
            {
                Console.Clear();
                _conversionHelper.WriteColoredMessage("Transitive closure calculation:\n------------------------\n", ConsoleColor.Green);
                _conversionHelper.WriteColoredMessage("Enter the root path of the snapshot sqlite database file or type 'exit' to go back to the main menu:\n", ConsoleColor.White);

                var filePath = Console.ReadLine()?.Trim('"');

                if (string.Equals(filePath?.ToLower(), "exit"))
                {
                    break;
                }

                if (File.Exists(filePath))
                {
                    try
                    {
                        using var pbar = new ProgressBar(2, "Creating transitive closure from database...", new ProgressBarOptions { ProgressCharacter = '─' });
                        _connectionStringService.SetConnectionString(@$"Data Source={filePath}");
                        await _runner.CreateTransitiveClosureTableFromDB(filePath, pbar);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error: {ex.Message}\n{ex.StackTrace}", ex.Message, ex.StackTrace);
                        _conversionHelper.WriteColoredMessage("\nAn error occurred during conversion. Press any key to try again...\n", ConsoleColor.Red);
                    }
                }
                else
                {
                    _conversionHelper.WriteColoredMessage("\n--------------------------------------------------------------------------------\nThe entered file does not exist or is not a directory.\nPlease enter a valid directory path, or type 'exit' to go back to the main menu:\n--------------------------------------------------------------------------------\n", ConsoleColor.Red);
                    await Task.Delay(2000);
                }
            }

            Console.Clear();
            return true;
        }
    }
}
