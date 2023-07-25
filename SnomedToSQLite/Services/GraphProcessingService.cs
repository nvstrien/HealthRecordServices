using System.Collections.Concurrent;

using SnomedRF2Library.Models;

namespace SnomedToSQLite.Services
{
    public class GraphProcessingService : IGraphProcessingService
    {
        //Relationship properties
        private const long _isARelationshipTypeId = 116680003; // |is a|
        
        //Description properties
        private const long _synonymTypeId = 900000000000013009; // |Synonym|
        
        //Refset properties
        private const long _nlRefsetId = 31000146106;
        private const long _enGBRefsetId = 900000000000508004;
        private const long _enUSRefsetId = 900000000000509007;
        private const long _acceptabilityId = 900000000000548007; // | Preferred|

        public Dictionary<long, Dictionary<long, long>> CreateAdjacencyMatrix(IEnumerable<RelationshipModel> relationships)
        {
            var adjacencyMatrix = new Dictionary<long, Dictionary<long, long>>();

            foreach (var relationship in relationships)
            {
                if (relationship.TypeId != _isARelationshipTypeId)
                    continue; // Skip non-"is a" relationships

                if (!adjacencyMatrix.ContainsKey(relationship.SourceId))
                {
                    adjacencyMatrix[relationship.SourceId] = new Dictionary<long, long>();
                }

                adjacencyMatrix[relationship.SourceId][relationship.DestinationId] = relationship.TypeId;
            }

            return adjacencyMatrix;
        }

        /// <summary>
        /// Returns the active concept ids from the given list of concept models.
        /// </summary>
        /// <param name="concepts">The enumerable of ConceptModel instances.</param>
        /// <returns>A HashSet of active concept ids.</returns>
        private HashSet<long> GetActiveConceptIds(IEnumerable<ConceptModel> concepts)
        {
            return new HashSet<long>(concepts.Where(c => c.Active).Select(c => c.Id));
        }

        /// <summary>
        /// Returns a dictionary that maps tuples of description id and case significance id to type ids.
        /// This function groups descriptions by term and selects the most recent (by effective time) in each group.
        /// </summary>
        /// <param name="descriptions">The enumerable of DescriptionModel instances.</param>
        /// <returns>A dictionary mapping tuples of id and case significance id to type id.</returns>
        private Dictionary<Tuple<long, long>, long> GroupDescriptionsByTerm(IEnumerable<DescriptionModel> descriptions)
        {
            return descriptions
                .GroupBy(d => Tuple.Create(d.Id, d.CaseSignificanceId))
                .Select(g => g.OrderByDescending(d => d.EffectiveTime).First())
                .ToDictionary(d => Tuple.Create(d.Id, d.CaseSignificanceId), d => d.TypeId);
        }

        /// <summary>
        /// Returns a hash set of preferred description ids from the given list of language refset models.
        /// This function filters the language refset models for active instances that reference a synonym type description.
        /// </summary>
        /// <param name="languageRefsets">The enumerable of LanguageRefsetModel instances.</param>
        /// <param name="descriptionsGroupedByTerm">A dictionary mapping tuples of id and case significance id to type id.</param>
        /// <returns>A HashSet of preferred description ids.</returns>
        private HashSet<Tuple<long, long>> GetPreferredDescriptionIds(IEnumerable<LanguageRefsetModel> languageRefsets, Dictionary<Tuple<long, long>, long> descriptionsGroupedByTerm)
        {
            return new HashSet<Tuple<long, long>>(languageRefsets
                .Where(l => l.Active
                            && l.AcceptabilityId == _acceptabilityId
                            && descriptionsGroupedByTerm.ContainsKey(Tuple.Create(l.ReferencedComponentId, l.AcceptabilityId))
                            && descriptionsGroupedByTerm[Tuple.Create(l.ReferencedComponentId, l.AcceptabilityId)] == _synonymTypeId)
                .Select(l => Tuple.Create(l.ReferencedComponentId, l.AcceptabilityId)));
        }

        /// <summary>
        /// Creates an adjacency matrix representing the relationships between active concepts using preferred synonym descriptions.
        /// </summary>
        /// <param name="relationships">The enumerable of RelationshipModel instances.</param>
        /// <param name="concepts">The enumerable of ConceptModel instances.</param>
        /// <param name="descriptions">The enumerable of DescriptionModel instances.</param>
        /// <param name="languageRefsets">The enumerable of LanguageRefsetModel instances.</param>
        /// <returns>An adjacency matrix as a Dictionary of Dictionary, where the key of the outer dictionary is the source concept id, the key of the inner dictionary is the destination concept id, and the value of the inner dictionary is the relationship type id.</returns>
        /// <exception cref="Exception">Throws any exceptions that may occur during processing.</exception>
        public Dictionary<long, Dictionary<long, long>> CreateAdjacencyMatrix(IEnumerable<RelationshipModel> relationships, IEnumerable<ConceptModel> concepts, IEnumerable<DescriptionModel> descriptions, IEnumerable<LanguageRefsetModel> languageRefsets)
        {
            try
            {
                // Getting all the active concept ids
                var activeConceptIds = GetActiveConceptIds(concepts);

                // Grouping the descriptions by term and ordering by Effective Time
                var descriptionsGroupedByTerm = GroupDescriptionsByTerm(descriptions);

                // Getting all the preferred description ids
                var preferredDescriptionIds = GetPreferredDescriptionIds(languageRefsets, descriptionsGroupedByTerm);

                // Create the adjacency matrix
                var adjacencyMatrix = new Dictionary<long, Dictionary<long, long>>();

                foreach (var relationship in relationships)
                {
                    // Ignore relationships with wrong type id
                    if (relationship.TypeId != _isARelationshipTypeId)
                        continue;

                    // Ignore inactive relationships or relationships with inactive source or destination concepts
                    if (!relationship.Active || !activeConceptIds.Contains(relationship.SourceId) || !activeConceptIds.Contains(relationship.DestinationId))
                        continue;

                    var destinationKey = Tuple.Create(relationship.DestinationId, relationship.TypeId);

                    // Ignore relationships with non-preferred destination descriptions
                    if (!preferredDescriptionIds.Contains(destinationKey))
                        continue;

                    // Add the source concept to the adjacency matrix if it doesn't already exist
                    if (!adjacencyMatrix.ContainsKey(relationship.SourceId))
                    {
                        adjacencyMatrix[relationship.SourceId] = new Dictionary<long, long>();
                    }

                    // Map the source concept to the destination concept in the adjacency matrix
                    adjacencyMatrix[relationship.SourceId][relationship.DestinationId] = relationship.TypeId;
                }

                return adjacencyMatrix;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public Dictionary<long, HashSet<long>> ComputeTransitiveClosure(Dictionary<long, Dictionary<long, long>> adjacencyMatrix)
        {
            var transitiveClosure = new Dictionary<long, HashSet<long>>();

            foreach (var node in adjacencyMatrix.Keys)
            {
                if (!transitiveClosure.ContainsKey(node))
                    transitiveClosure[node] = new HashSet<long>();

                var queue = new Queue<long>(adjacencyMatrix[node].Keys);
                while (queue.Any())
                {
                    var nextNode = queue.Dequeue();
                    if (adjacencyMatrix.ContainsKey(nextNode))
                    {
                        foreach (var transitNode in adjacencyMatrix[nextNode].Keys)
                        {
                            if (!transitiveClosure[node].Contains(transitNode))
                            {
                                transitiveClosure[node].Add(transitNode);
                                queue.Enqueue(transitNode);
                            }
                        }
                    }
                }
            }

            return transitiveClosure;
        }

        public Dictionary<long, HashSet<long>> ComputeTransitiveClosureParallel(Dictionary<long, Dictionary<long, long>> adjacencyMatrix)
        {
            var transitiveClosure = new ConcurrentDictionary<long, HashSet<long>>();

            Parallel.ForEach(adjacencyMatrix.Keys, node =>
            {
                if (!transitiveClosure.ContainsKey(node))
                    transitiveClosure[node] = new HashSet<long>();

                var queue = new ConcurrentQueue<long>(adjacencyMatrix[node].Keys);
                while (!queue.IsEmpty)
                {
                    if (queue.TryDequeue(out var nextNode))
                    {
                        if (adjacencyMatrix.TryGetValue(nextNode, out var transitNodes))
                        {
                            foreach (var transitNode in transitNodes.Keys)
                            {
                                if (!transitiveClosure[node].Contains(transitNode))
                                {
                                    transitiveClosure[node].Add(transitNode);
                                    queue.Enqueue(transitNode);
                                }
                            }
                        }
                    }
                }
            });

            return transitiveClosure.ToDictionary(kvp => kvp.Key, kvp => new HashSet<long>(kvp.Value));
        }
    }
}
