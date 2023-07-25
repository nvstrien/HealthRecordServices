using Microsoft.Extensions.Logging;

using SnomedToSQLite.Menu.ConvertRf2ToSQLite;
using SnomedToSQLite.Services;

using static SnomedRF2Library.Enums.Enumeration;

namespace SnomedToSQLite.Menu
{
    public class ConvertRf2FullReleaseToSQLiteOption : IMenuOption
    {
        private readonly IConversionHelper _conversionHelper;
        private readonly IConvertRf2ToSQLiteRunner _runner;
        private readonly ILogger<ConvertRf2FullReleaseToSQLiteOption> _logger;

        public ConvertRf2FullReleaseToSQLiteOption(
            IConversionHelper conversionHelper,
            IConvertRf2ToSQLiteRunner runner, 
            ILogger<ConvertRf2FullReleaseToSQLiteOption> logger)
        {
            _conversionHelper = conversionHelper;
            _runner = runner;
            _logger = logger;
        }

        public string Description => "Convert RF2 Full release to SQLite (only imports the data tables)";

        public async Task<bool> ExecuteAsync()
        {
            while (true)
            {
                Console.Clear();
                _conversionHelper.WriteColoredMessage("RF2 Full Release to SQLite converter:\n------------------------\n", ConsoleColor.Green);
                _conversionHelper.WriteColoredMessage("Enter the root path of the Full release files or type 'exit' to go back to the main menu:\n", ConsoleColor.White);

                var rootFilePath = Console.ReadLine()?.Trim('"');

                if (string.Equals(rootFilePath?.ToLower(), "exit"))
                {
                    break;
                }

                if (_conversionHelper.IsValidDirectory(rootFilePath))
                {
                    try
                    {
                        await _runner.ExecuteConversion(rootFilePath, ConversionType.FullConversion);
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
                    _conversionHelper.WriteColoredMessage("\n--------------------------------------------------------------------------------\nThe entered path does not exist or is not a directory.\nPlease enter a valid directory path, or type 'exit' to go back to the main menu:\n--------------------------------------------------------------------------------\n", ConsoleColor.Red);
                    await Task.Delay(2000);
                }
            }

            Console.Clear();
            return true;
        }
    }
}
