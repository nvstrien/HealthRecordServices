using SnomedRF2Library.Models;

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
            _databaseService.CreateSnowMedSQLiteDb(@"R:\SnowMed.db");

            await ImportAndWriteConceptData(fullConceptPath);
            await ImportAndWriteDescriptionData(fullDescriptionPath);
            var relationshipData = ImportRelationshipData(fullRelationshipPath);
            await WriteRelationshipDataAndGenerateClosure(relationshipData);
        }

        private async Task ImportAndWriteConceptData(string fullConceptPath)
        {
            var importedConceptData = _importService.ImportRf2Concept(fullConceptPath);
            await _databaseService.WriteConceptData(importedConceptData);
        }

        private async Task ImportAndWriteDescriptionData(string fullDescriptionPath)
        {
            var importedDescriptionData = _importService.ImportRf2Description(fullDescriptionPath);
            await _databaseService.WriteDescriptionData(importedDescriptionData);
        }

        private IEnumerable<RelationshipModel> ImportRelationshipData(string fullRelationshipPath)
        {
            var importedRelationshipData = _importService.ImportRf2Relationship(fullRelationshipPath);
            return importedRelationshipData;
        }

        private async Task WriteRelationshipDataAndGenerateClosure(IEnumerable<RelationshipModel> relationshipData)
        {
            await _databaseService.WriteRelationshipData(relationshipData);
            await _databaseService.GenerateTransitiveClosureTable(relationshipData);
        }


    }
}