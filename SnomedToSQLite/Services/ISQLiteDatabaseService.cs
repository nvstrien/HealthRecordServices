using SnomedRF2Library.Models;

namespace SnomedToSQLite.Services
{
    public interface ISQLiteDatabaseService
    {
        void CreateSnowMedSQLiteDb(string path);
        Task GenerateTransitiveClosureTable(IEnumerable<RelationshipModel> relationships, ShellProgressBar.IProgressBar pbar);
        Task<IEnumerable<RelationshipModel>> GetRelationshipData();
        Task<bool> WriteConceptData(IEnumerable<ConceptModel> data);
        Task<bool> WriteDescriptionData(IEnumerable<DescriptionModel> data);
        Task<bool> WriteLanguageRefsetData(IEnumerable<LanguageRefsetModel> data);
        Task<bool> WriteRelationshipData(IEnumerable<RelationshipModel> data);
    }
}