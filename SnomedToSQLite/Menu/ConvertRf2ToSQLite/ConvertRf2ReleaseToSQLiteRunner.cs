using ShellProgressBar;

using SnomedRF2Library.Models;

using SnomedToSQLite.Services;

using SqliteLibrary;

using static SnomedRF2Library.Enums.Enumeration;

namespace SnomedToSQLite.Menu.ConvertRf2ToSQLite
{
    public class ConvertRf2ReleaseToSQLiteRunner : IConvertRf2ToSQLiteRunner
    {
        private readonly IImportService _importService;
        private readonly ISQLiteDatabaseService _databaseService;
        private readonly IConnectionStringService _connectionStringService;
        private readonly IConversionHelper _conversionHelper;
        private readonly IFileFinder _fileFinder;

        public ConvertRf2ReleaseToSQLiteRunner(
            IImportService importService,
            ISQLiteDatabaseService databaseService,
            IConnectionStringService connectionStringService,
            IConversionHelper conversionHelper,
            IFileFinder fileFinder)
        {
            _importService = importService;
            _databaseService = databaseService;
            _connectionStringService = connectionStringService;
            _conversionHelper = conversionHelper;
            _fileFinder = fileFinder;
        }

        public async Task ExecuteConversion(string rootFilePath, ConversionType conversionType)
        {
            string filePrefix = conversionType == ConversionType.FullConversion ? "Full" : "Snapshot";

            string conceptPath = _fileFinder.FindFileInDirectory(rootFilePath, $"*Concept_{filePrefix}*.txt");
            List<string> descriptionPaths = _fileFinder.FindFilesInDirectory(rootFilePath, $"*Description_{filePrefix}*.txt");
            string relationshipPath = _fileFinder.FindFileInDirectory(rootFilePath, $"*Relationship_{filePrefix}*.txt");
            List<string> languageRefsetPaths = _fileFinder.FindFilesInDirectory(rootFilePath, $"*Language{filePrefix}*.txt");

            _conversionHelper.WriteColoredMessage("---------------------------", ConsoleColor.Green);
            _conversionHelper.WriteColoredMessage("Starting conversion process...", ConsoleColor.Green);
            _conversionHelper.WriteColoredMessage("---------------------------", ConsoleColor.Green);

            _conversionHelper.WriteColoredMessage("Files found for conversion:", ConsoleColor.Yellow);
            _conversionHelper.WriteColoredMessage($"Concept file:\n{conceptPath}\n", ConsoleColor.White);

            _conversionHelper.WriteColoredMessage("Description files:", ConsoleColor.Yellow);
            foreach (var descriptionPath in descriptionPaths)
            {
                _conversionHelper.WriteColoredMessage($"{descriptionPath}\n", ConsoleColor.White);
            }

            _conversionHelper.WriteColoredMessage("Relationship file:", ConsoleColor.Yellow);
            _conversionHelper.WriteColoredMessage($"{relationshipPath}\n", ConsoleColor.White);

            _conversionHelper.WriteColoredMessage("Language Refset files:", ConsoleColor.Yellow);
            foreach (var languagePath in languageRefsetPaths)
            {
                _conversionHelper.WriteColoredMessage($"{languagePath}\n", ConsoleColor.White);
            }

            _conversionHelper.WriteColoredMessage("---------------------------", ConsoleColor.Green);
            _conversionHelper.WriteColoredMessage($"SQLite database will be created in {rootFilePath}\\{_conversionHelper.GetDbName(conversionType)}.", ConsoleColor.Cyan);
            _conversionHelper.WriteColoredMessage("---------------------------\n", ConsoleColor.Green);

            await ConvertRf2ToSQLIte(rootFilePath, conceptPath, descriptionPaths, relationshipPath, languageRefsetPaths, conversionType);

            _conversionHelper.WriteColoredMessage("Conversion completed successfully!\n", ConsoleColor.Green);
        }

        public async Task ConvertRf2ToSQLIte(string rootFilePath, string fullConceptPath, List<string> fullDescriptionPaths, string fullRelationshipPath, List<string> languageRefsetPaths, ConversionType conversionType)
        {
            string dbName = _conversionHelper.GetDbName(conversionType);

            var dbOutput = Path.Combine(rootFilePath, dbName);
            int maxTicks = fullDescriptionPaths.Count + 15;

            using var pbar = new ProgressBar(maxTicks, "Creating SQLite database...", new ProgressBarOptions { ProgressCharacter = '─' });

            pbar.Message = "Creating database at location: " + dbOutput;
            _databaseService.CreateSnowMedSQLiteDb(dbOutput);
            pbar.Tick("Database created successfully at location: " + dbOutput);

            pbar.Message = "Setting new database as connection string...";
            _connectionStringService.SetConnectionString($"Data Source={dbOutput}");
            pbar.Tick("New database set as connection string.");

            pbar.Message = "Starting import and write process for concept data...";
            var concepts = await ImportAndWriteConceptData(fullConceptPath, pbar);
            pbar.Tick("Concept data processed successfully.");

            List<DescriptionModel> descriptions = new();
            foreach (var item in fullDescriptionPaths)
            {
                pbar.Message = $"Starting import and write process for description data from {item}...";
                var descriptionData = await ImportAndWriteDescriptionData(item, pbar);
                descriptions.AddRange(descriptionData);
                pbar.Tick("Description data processed successfully.");
            }

            pbar.Message = "Starting import of relationship data...";
            var relationshipData = ImportRelationshipData(fullRelationshipPath, pbar);
            pbar.Tick("Relationship data imported successfully.");

            List<LanguageRefsetModel> languageRefsets = new();
            foreach (var item in languageRefsetPaths)
            {
                pbar.Message = $"Starting import and write process for language refset data from {item}...";
                var languageRefsetData = await ImportAndWriteLanguageRefsetData(item, pbar);
                languageRefsets.AddRange(languageRefsetData);
                pbar.Tick("Language refset data processed successfully.");
            }

            pbar.Message = "Writing relationship data.";
            await WriteRelationshipData(relationshipData, pbar);
            pbar.Tick("Relationship data written successfully.");

            if (conversionType == ConversionType.SnapshotConversion)
            {
                pbar.Message = "Creating transitive closure table.";
                await CreateTransitiveClosureTable(relationshipData, pbar);
                pbar.Tick("Completed creating transitive closure table.");
            }

            Console.Clear();
        }

        private async Task<IEnumerable<ConceptModel>> ImportAndWriteConceptData(string fullConceptPath, IProgressBar pbar)
        {
            var importedConceptData = _importService.ImportRf2Concept(fullConceptPath);
            pbar.Message = "Writing concept data...";
            await _databaseService.WriteConceptData(importedConceptData);
            pbar.Tick("Completed: Writing concept data...");
            return importedConceptData;
        }

        private async Task<IEnumerable<DescriptionModel>> ImportAndWriteDescriptionData(string fullDescriptionPath, IProgressBar pbar)
        {
            var importedDescriptionData = _importService.ImportRf2Description(fullDescriptionPath);
            pbar.Message = $"Writing description data from {fullDescriptionPath}...";
            await _databaseService.WriteDescriptionData(importedDescriptionData);
            pbar.Tick($"Completed: Writing description data from {fullDescriptionPath}...");
            return importedDescriptionData;
        }

        private async Task<IEnumerable<LanguageRefsetModel>> ImportAndWriteLanguageRefsetData(string item, IProgressBar pbar)
        {
            var imported = _importService.ImportRf2LanguageRefset(item);
            pbar.Message = $"Writing language refset data from {item}...";
            await _databaseService.WriteLanguageRefsetData(imported);
            pbar.Tick($"Completed: Writing language refset data from {item}...");
            return imported;
        }

        private IEnumerable<RelationshipModel> ImportRelationshipData(string fullRelationshipPath, IProgressBar pbar)
        {
            pbar.Message = "Importing relationship data...";
            var importedRelationshipData = _importService.ImportRf2Relationship(fullRelationshipPath);
            pbar.Tick("Completed: Importing relationship data...");
            return importedRelationshipData;
        }

        private async Task WriteRelationshipData(IEnumerable<RelationshipModel> relationshipData, IProgressBar pbar)
        {
            pbar.Message = "Writing relationship data...";
            await _databaseService.WriteRelationshipData(relationshipData);
            pbar.Tick("Completed: Writing relationship data...");
        }

        private async Task CreateTransitiveClosureTable(IEnumerable<RelationshipModel> relationshipData, IProgressBar pbar)
        {
            pbar.Message = "Generating transitive closure table";
            await _databaseService.GenerateTransitiveClosureTable(relationshipData, pbar);
            pbar.Tick("Completed: Generating transitive closure table");
        }


        public async Task CreateTransitiveClosureTableFromDB(string filePath, IProgressBar pbar)
        {
            _conversionHelper.WriteColoredMessage("Loading data:\n------------------------\n", ConsoleColor.Green);
            var relationshipData = await _databaseService.GetRelationshipData();
            var relationsCount = relationshipData.Count();
            _conversionHelper.WriteColoredMessage($"Loaded {relationsCount} relations:\n------------------------\n", ConsoleColor.Green);

            await CreateTransitiveClosureTable(relationshipData, pbar);
        }
    }
}