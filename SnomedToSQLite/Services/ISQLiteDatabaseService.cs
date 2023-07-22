using SnomedRF2Library.Models;

namespace SnomedToSQLite.Services
{
    public interface ISQLiteDatabaseService
    {
        void CreateSnowMedSQLiteDb(string path);
        Task GenerateTransitiveClosureTable(IEnumerable<RelationshipModel> relationships);
        Task<bool> WriteConceptData(IEnumerable<ConceptModel> data);
        Task<bool> WriteDescriptionData(IEnumerable<DescriptionModel> data);
        Task<bool> WriteRelationshipData(IEnumerable<RelationshipModel> data);
    }
}