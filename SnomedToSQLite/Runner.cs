using SnomedToSQLite.Services;

using SqliteLibrary;

namespace SnomedToSQLite
{
    public class Runner
    {
        private readonly IImportService _importService;
        private readonly ISQLiteDatabaseService _databaseService;

        public Runner(IImportService importService, ISQLiteDatabaseService databaseService)
        {
            _importService = importService;
            _databaseService = databaseService;
        }

        public async Task ConvertRf2ToSQLIte(string fullConceptPath, string fullDescriptionPath, string fullRelationshipPath)
        {
            var importedConceptData = _importService.ImportRf2Concept(fullConceptPath);

            var importedDescriptionData = _importService.ImportRf2Description(fullDescriptionPath);

            var importedRelationshipData = _importService.ImportRf2Relationship(fullRelationshipPath);

            _databaseService.CreateSnowMedSQLiteDb(@"R:\SnowMed.db");

            await _databaseService.WriteConceptData(importedConceptData);

            await _databaseService.WriteDescriptionData(importedDescriptionData);

            await _databaseService.WriteRelationshipData(importedRelationshipData);
        }
    }
}