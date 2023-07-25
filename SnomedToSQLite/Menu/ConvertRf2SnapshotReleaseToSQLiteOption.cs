using Microsoft.Extensions.Logging;

using SnomedToSQLite.Menu.ConvertRf2ToSQLite;
using SnomedToSQLite.Services;

using static SnomedRF2Library.Enums.Enumeration;

namespace SnomedToSQLite.Menu
{
    internal class ConvertRf2SnapshotReleaseToSQLiteOption : IMenuOption
    {
        private readonly IConversionHelper _conversionHelper;
        private readonly ILogger<ConvertRf2SnapshotReleaseToSQLiteOption> _logger;
        private readonly IConvertRf2ToSQLiteRunner _runner;

        public ConvertRf2SnapshotReleaseToSQLiteOption(
            IConversionHelper conversionHelper,
            ILogger<ConvertRf2SnapshotReleaseToSQLiteOption> logger,
            IConvertRf2ToSQLiteRunner runner)
        {
            _conversionHelper = conversionHelper;
            _logger = logger;
            _runner = runner;
        }

        public string Description => "Convert RF2 Snapshot release to SQLite (imports data tables + creates transitive closure table)";

        public async Task<bool> ExecuteAsync()
        {
            while (true)
            {
                Console.Clear();
                _conversionHelper.WriteColoredMessage("RF2 Snapshot Release to SQLite converter:\n------------------------\n", ConsoleColor.Green);
                _conversionHelper.WriteColoredMessage("Enter the root path of the Snapshot release files or type 'exit' to go back to the main menu:\n", ConsoleColor.White);

                var rootFilePath = Console.ReadLine()?.Trim('"');

                if (string.Equals(rootFilePath?.ToLower(), "exit"))
                {
                    break;
                }

                if (_conversionHelper.IsValidDirectory(rootFilePath))
                {
                    try
                    {
                        await _runner.ExecuteConversion(rootFilePath, ConversionType.SnapshotConversion);
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
