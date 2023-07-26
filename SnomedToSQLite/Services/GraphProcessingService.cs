using ShellProgressBar;

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
        //private const long _nlRefsetId = 31000146106;
        //private const long _enGBRefsetId = 900000000000508004;
        //private const long _enUSRefsetId = 900000000000509007;
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


        // Method to extract |is a| relationships from the RelationshipModel data
        public Dictionary<long, HashSet<long>> GetIsARelationships(IEnumerable<RelationshipModel> relationships)
        {
            var isARelationships = relationships
                .Where(relationship => relationship.TypeId == _isARelationshipTypeId && relationship.Active == true)
                .GroupBy(relationship => relationship.SourceId)
                .ToDictionary(
                    group => group.Key,
                    group => new HashSet<long>(group.Select(relationship => relationship.DestinationId))
                );

            return isARelationships;
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
        /// Computes the transitive closure of the given relationships.
        /// </summary>
        /// <param name="relationships">The relationships to compute the transitive closure for.</param>
        /// <param name="pbar">A progress bar to update as the computation progresses.</param>
        /// <returns>A dictionary where each key is a concept and the corresponding value is a set of all concepts that the key concept has a direct or indirect relationship with.</returns>
        public async Task<Dictionary<long, HashSet<long>>> ComputeTransitiveClosureAsync(IEnumerable<RelationshipModel> relationships, IProgressBar pbar)
        {
            // Get the active |is a| relationships
            var isARelationships = GetIsARelationships(relationships);

            // Set the maximum value for the progress bar
            pbar.MaxTicks += isARelationships.Count();

            // Initialize the transitive closure table
            var transitiveClosureTable = new Dictionary<long, HashSet<long>>();

            // Calculate the transitive closure table
            foreach (var concept in isARelationships.Keys)
            {
                // Initialize the set of related concepts for this concept
                transitiveClosureTable[concept] = new HashSet<long>();

                // Add all concepts that this concept has a direct or indirect relationship with to the set
                AddTransitiveRelationships(isARelationships, transitiveClosureTable, concept, concept);

                // Update the progress bar
                pbar.Tick();

                // Yield to prevent UI freezing
                await Task.Yield();
            }

            return transitiveClosureTable;
        }

        /// <summary>
        /// Recursively adds all concepts that the original concept has a direct or indirect relationship with to the transitive closure table.
        /// This method implements a depth-first traversal of the graph represented by the |is a| relationships to compute the transitive closure.
        /// </summary>
        /// <param name="isARelationships">The |is a| relationships.</param>
        /// <param name="transitiveClosureTable">The transitive closure table to update.</param>
        /// <param name="originalConcept">The concept to add the relationships for.</param>
        /// <param name="currentConcept">The concept currently being processed.</param>
        private void AddTransitiveRelationships(Dictionary<long, HashSet<long>> isARelationships, Dictionary<long, HashSet<long>> transitiveClosureTable, long originalConcept, long currentConcept)
        {
            // If the current concept has no |is a| relationships, return immediately
            if (!isARelationships.ContainsKey(currentConcept)) return;

            // For each concept that the current concept has a |is a| relationship with
            foreach (var relatedConcept in isARelationships[currentConcept])
            {
                // If the related concept has not already been added to the set of related concepts for the original concept
                if (transitiveClosureTable[originalConcept].Add(relatedConcept))
                {
                    // Recursively add all concepts that the related concept has a direct or indirect relationship with to the set
                    AddTransitiveRelationships(isARelationships, transitiveClosureTable, originalConcept, relatedConcept);
                }
            }
        }
    }
}
