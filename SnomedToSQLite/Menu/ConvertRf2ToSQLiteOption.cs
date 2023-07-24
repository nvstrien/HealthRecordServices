using Microsoft.Extensions.Logging;

using SnomedToSQLite.Menu.ConvertRf2ToSQLite;
using SnomedToSQLite.Services;

namespace SnomedToSQLite.Menu
{
    public class ConvertRf2ToSQLiteOption : IMenuOption
    {
        private readonly IConvertRf2ToSQLiteRunner _runner;
        private readonly IFileFinder _fileFinder;
        private readonly ILogger<ConvertRf2ToSQLiteOption> _logger;

        public ConvertRf2ToSQLiteOption(IConvertRf2ToSQLiteRunner runner, IFileFinder fileFinder, ILogger<ConvertRf2ToSQLiteOption> logger)
        {
            _runner = runner;
            _fileFinder = fileFinder;
            _logger = logger;
        }

        public string Description => "Convert RF2 to SQLite";

        public async Task<bool> ExecuteAsync()
        {
            while (true)
            {
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("RF2 to SQLite converter:");
                Console.WriteLine("------------------------");
                Console.ResetColor();
                Console.WriteLine();
                Console.WriteLine("Enter the root path of the Full release files or type 'exit' to go back to the main menu:");
                var rootFilePath = Console.ReadLine()?.Trim('"');

                if (string.Equals(rootFilePath?.ToLower(), "exit"))
                {
                    break;
                }

                if (IsValidDirectory(rootFilePath))
                {
                    try
                    {
                        await ExecuteConversion(rootFilePath);
                        break;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError("Error: {ex.Message}\n{ex.StackTrace}", ex.Message, ex.StackTrace);
                        Console.WriteLine("\nAn error occurred during conversion. Press any key to try again...");
                    }
                }
                else
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    Console.WriteLine("The entered path does not exist or is not a directory.");
                    Console.WriteLine("Please enter a valid directory path, or type 'exit' to go back to the main menu:");
                    Console.WriteLine("--------------------------------------------------------------------------------");
                    Console.ResetColor();
                    await Task.Delay(2000);
                }
            }

            return true;
        }

        private bool IsValidDirectory(string? path)
        {
            return Directory.Exists(path);
        }

        private async Task ExecuteConversion(string rootFilePath)
        {
            string conceptPath = _fileFinder.FindFileInDirectory(rootFilePath, "*Concept_Full*.txt");
            List<string> descriptionPaths = _fileFinder.FindFilesInDirectory(rootFilePath, "*Description_Full*.txt");
            string relationshipPath = _fileFinder.FindFileInDirectory(rootFilePath, "*Relationship_Full*.txt");

            _logger.LogInformation("\nStarting conversion process...");
            _logger.LogInformation("\nFiles found for conversion:");
            _logger.LogInformation("\nConcept file: {conceptPath}", conceptPath);
            _logger.LogInformation("\nDescription files:");
            foreach (var descriptionPath in descriptionPaths)
            {
                _logger.LogInformation(descriptionPath);
            }
            _logger.LogInformation("\nRelationship file: {relationshipPath}", relationshipPath);

            await _runner.ConvertRf2ToSQLIte(rootFilePath, conceptPath, descriptionPaths, relationshipPath);
            _logger.LogInformation("\nConversion completed successfully!");
        }
    }
}
