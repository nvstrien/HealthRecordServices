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
            int maxTicks = fullDescriptionPaths.Count + 10;

            using (var pbar = new ProgressBar(maxTicks, "Creating SQLite database...", new ProgressBarOptions { ProgressCharacter = '─' }))
            {
                _databaseService.CreateSnowMedSQLiteDb(dbOutput);
                pbar.Tick("Database created successfully at location: " + dbOutput);
                await Task.Delay(1000); // just to let user read the message, remove if not needed

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

                pbar.Message = "Writing relationship data and generating closure...";
                await WriteRelationshipDataAndGenerateClosure(relationshipData, pbar);
                pbar.Tick("Relationship data and closure generated successfully.");
            }
        }

        private async Task ImportAndWriteConceptData(string fullConceptPath, IProgressBar pbar)
        {
            var importedConceptData = _importService.ImportRf2Concept(fullConceptPath);
            pbar.Tick("Writing concept data...");
            await _databaseService.WriteConceptData(importedConceptData);
        }

        private async Task ImportAndWriteDescriptionData(string fullDescriptionPath, IProgressBar pbar)
        {
            var importedDescriptionData = _importService.ImportRf2Description(fullDescriptionPath);
            pbar.Tick($"Writing description data from {fullDescriptionPath}...");
            await _databaseService.WriteDescriptionData(importedDescriptionData);
        }

        private IEnumerable<RelationshipModel> ImportRelationshipData(string fullRelationshipPath, IProgressBar pbar)
        {
            pbar.Tick("Importing relationship data...");
            var importedRelationshipData = _importService.ImportRf2Relationship(fullRelationshipPath);
            return importedRelationshipData;
        }

        private async Task WriteRelationshipDataAndGenerateClosure(IEnumerable<RelationshipModel> relationshipData, IProgressBar pbar)
        {
            pbar.Tick("Writing relationship data...");
            await _databaseService.WriteRelationshipData(relationshipData);

            pbar.Tick("Generating transitive closure table");
            await _databaseService.GenerateTransitiveClosureTable(relationshipData);
        }
    }
}