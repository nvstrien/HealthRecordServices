using SnomedToSQLite.Menu.ConvertRf2ToSQLite;
using SnomedToSQLite.Services;

namespace SnomedToSQLite.Menu
{
    public class ConvertRf2ToSQLiteOption : IMenuOption
    {
        private readonly IConvertRf2ToSQLiteRunner _runner;
        private readonly IFileFinder _fileFinder;

        public ConvertRf2ToSQLiteOption(IConvertRf2ToSQLiteRunner runner, IFileFinder fileFinder)
        {
            _runner = runner;
            _fileFinder = fileFinder;
        }

        public string Description => "Convert RF2 to SQLite";

        public async Task<bool> ExecuteAsync()
        {
            Console.Write("Enter the root path of the Full or Snapshot release files: ");
            var filePath = Console.ReadLine();
            filePath = filePath.Trim('"');

            //parse files in directory
            if (Directory.Exists(filePath))
            {
                string conceptPath = _fileFinder.FindFileInDirectory(filePath, "*Concept_Full*.txt");
                List<string> descriptionPaths = _fileFinder.FindFilesInDirectory(filePath, "*Description_Full*.txt");
                string relationshipPath = _fileFinder.FindFileInDirectory(filePath, "*Relationship_Full*.txt");
                
                try
                {
                    await _runner.ConvertRf2ToSQLIte(conceptPath, descriptionPaths, relationshipPath);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

            return true;
        }
    }

}
