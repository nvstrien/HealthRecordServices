using SnomedRF2Library.Models;

namespace SnomedToSQLite.Services
{
    public interface IGraphProcessingService
    {
        Dictionary<long, HashSet<long>> ComputeTransitiveClosure(Dictionary<long, Dictionary<long, long>> adjacencyMatrix);
        Dictionary<long, HashSet<long>> ComputeTransitiveClosureParallel(Dictionary<long, Dictionary<long, long>> adjacencyMatrix);
        Dictionary<long, Dictionary<long, long>> CreateAdjacencyMatrix(IEnumerable<RelationshipModel> relationships);
        Dictionary<long, Dictionary<long, long>> CreateAdjacencyMatrix(IEnumerable<RelationshipModel> relationships, IEnumerable<ConceptModel> concepts, IEnumerable<DescriptionModel> descriptions, IEnumerable<LanguageRefsetModel> languageRefsets);
    }
}