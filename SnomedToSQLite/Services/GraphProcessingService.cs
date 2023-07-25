using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnomedRF2Library.Models;

namespace SnomedToSQLite.Services
{
    public class GraphProcessingService : IGraphProcessingService
    {
        private const long _isARelationshipTypeId = 116680003; // |is a|
        private const long _preferredAcceptabilityId = 900000000000548007; // | Preferred|
        private const long _synonymTypeId = 900000000000013009; // |Synonym|

        public Dictionary<long, Dictionary<long, long>> CreateAdjacencyMatrix(IEnumerable<RelationshipModel> relationships)
        {
            var adjacencyMatrix = new Dictionary<long, Dictionary<long, long>>();

            foreach (var relationship in relationships)
            {
                if (relationship.TypeId != 116680003)
                    continue; // Skip non-"is a" relationships

                if (!adjacencyMatrix.ContainsKey(relationship.SourceId))
                {
                    adjacencyMatrix[relationship.SourceId] = new Dictionary<long, long>();
                }

                adjacencyMatrix[relationship.SourceId][relationship.DestinationId] = relationship.TypeId;
            }

            return adjacencyMatrix;
        }

        public Dictionary<long, Dictionary<long, long>> CreateAdjacencyMatrix(IEnumerable<RelationshipModel> relationships, IEnumerable<ConceptModel> concepts, IEnumerable<DescriptionModel> descriptions, IEnumerable<LanguageRefsetModel> languageRefsets)
        {
            try
            {
                var activeConceptIds = new HashSet<long>(concepts.Where(c => c.Active).Select(c => c.Id));

                var descriptionsGroupedByTerm = descriptions
                    .GroupBy(d => Tuple.Create(d.Id, d.CaseSignificanceId))
                    .Select(g => g.OrderByDescending(d => d.EffectiveTime).First())
                    .ToDictionary(d => Tuple.Create(d.Id, d.CaseSignificanceId), d => d.TypeId);

                var preferredDescriptionIds = new HashSet<Tuple<long, long>>(languageRefsets
                    .Where(l => l.Active && l.AcceptabilityId == _preferredAcceptabilityId && descriptionsGroupedByTerm.ContainsKey(Tuple.Create(l.ReferencedComponentId, l.AcceptabilityId)))
                    .Select(l => Tuple.Create(l.ReferencedComponentId, l.AcceptabilityId)));

                var adjacencyMatrix = new Dictionary<long, Dictionary<long, long>>();

                foreach (var relationship in relationships)
                {
                    if (relationship.TypeId != _isARelationshipTypeId)
                        continue;

                    if (!relationship.Active || !activeConceptIds.Contains(relationship.SourceId) || !activeConceptIds.Contains(relationship.DestinationId))
                        continue;

                    var destinationKey = Tuple.Create(relationship.DestinationId, relationship.TypeId);
                    if (!preferredDescriptionIds.Contains(destinationKey))
                        continue;

                    if (!adjacencyMatrix.ContainsKey(relationship.SourceId))
                    {
                        adjacencyMatrix[relationship.SourceId] = new Dictionary<long, long>();
                    }

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
