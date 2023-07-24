using ShellProgressBar;

using SnomedRF2Library.Models;

using SnomedToSQLite.Services;

using SqliteLibrary;

namespace SnomedToSQLite.Menu.ConvertRf2ToSQLite
{
    public class ConvertRf2ToSQLiteRunner : IConvertRf2ToSQLiteRunner
    {
        private readonly IImportService _importService;
        private readonly ISQLiteDatabaseService _databaseService;
        private readonly IConnectionStringService _connectionStringService;


        public ConvertRf2ToSQLiteRunner(IImportService importService, ISQLiteDatabaseService databaseService, IConnectionStringService connectionStringService)
        {
            _importService = importService;
            _databaseService = databaseService;
            _connectionStringService = connectionStringService;
        }

        public async Task ConvertRf2ToSQLIte(string rootFilePath, string fullConceptPath, List<string> fullDescriptionPaths, string fullRelationshipPath)
        {
            var dbOutput = Path.Combine(rootFilePath, "SnoMed.db");
            int maxTicks = fullDescriptionPaths.Count + 15;

            using (var pbar = new ProgressBar(maxTicks, "Creating SQLite database...", new ProgressBarOptions { ProgressCharacter = '─' }))
            {
                pbar.Message = "Creating database at location: " + dbOutput;
                _databaseService.CreateSnowMedSQLiteDb(dbOutput);
                pbar.Tick("Database created successfully at location: " + dbOutput);

                pbar.Message = "Setting new database as connection string...";
                _connectionStringService.SetConnectionString($"Data Source={dbOutput}");
                pbar.Tick("New database set as connection string.");

                pbar.Message = "Starting import and write process for concept data...";
                await ImportAndWriteConceptData(fullConceptPath, pbar);
                pbar.Tick("Concept data processed successfully.");

                foreach (var item in fullDescriptionPaths)
                {
                    pbar.Message = $"Starting import and write process for description data from {item}...";
                    await ImportAndWriteDescriptionData(item, pbar);
                    pbar.Tick("Description data processed successfully.");
                }

                pbar.Message = "Starting import of relationship data...";
                var relationshipData = ImportRelationshipData(fullRelationshipPath, pbar);
                pbar.Tick("Relationship data imported successfully.");

                pbar.Message = "Writing relationship data and generating transitive closure table.";
                await WriteRelationshipDataAndGenerateClosure(relationshipData, pbar);
                pbar.Tick("Relationship data and transitive closure table generated successfully.");

                Console.Clear();

                Console.WriteLine("SNOMED SQLite Database  created successfully at location: " + dbOutput);

                await Task.Delay(2000);
            }
        }

        private async Task ImportAndWriteConceptData(string fullConceptPath, IProgressBar pbar)
        {
            var importedConceptData = _importService.ImportRf2Concept(fullConceptPath);
            pbar.Message = "Writing concept data...";
            await _databaseService.WriteConceptData(importedConceptData);
            pbar.Tick("Completed: Writing concept data...");
        }

        private async Task ImportAndWriteDescriptionData(string fullDescriptionPath, IProgressBar pbar)
        {
            var importedDescriptionData = _importService.ImportRf2Description(fullDescriptionPath);
            pbar.Message = $"Writing description data from {fullDescriptionPath}...";
            await _databaseService.WriteDescriptionData(importedDescriptionData);
            pbar.Tick($"Completed: Writing description data from {fullDescriptionPath}...");
        }

        private IEnumerable<RelationshipModel> ImportRelationshipData(string fullRelationshipPath, IProgressBar pbar)
        {
            pbar.Message = "Importing relationship data...";
            var importedRelationshipData = _importService.ImportRf2Relationship(fullRelationshipPath);
            pbar.Tick("Completed: Importing relationship data...");
            return importedRelationshipData;
        }

        private async Task WriteRelationshipDataAndGenerateClosure(IEnumerable<RelationshipModel> relationshipData, IProgressBar pbar)
        {
            pbar.Message = "Writing relationship data...";
            await _databaseService.WriteRelationshipData(relationshipData);
            pbar.Tick("Completed: Writing relationship data...");

            pbar.Message = "Generating transitive closure table";
            await _databaseService.GenerateTransitiveClosureTable(relationshipData, pbar);
            pbar.Tick("Completed: Generating transitive closure table");
        }
    }
}