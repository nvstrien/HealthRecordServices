using SnomedRF2Library.Models;

namespace SnomedToSQLite.Services
{
    public interface IImportService
    {
        IEnumerable<ConceptModel> ImportRf2Concept(string rf2source);
        IEnumerable<DescriptionModel> ImportRf2Description(string rf2source);
        IEnumerable<RelationshipModel> ImportRf2Relationship(string rf2source);
        void SomeMethod();
    }
}