using System.Diagnostics;

using Microsoft.Extensions.Logging;

using SnomedRF2Library.Models;

using SqliteLibrary;

namespace SnomedToSQLite.Services
{
    public partial class SQLiteDatabaseService : ISQLiteDatabaseService
    {
        private readonly ISqlDataAccess _db;
        private readonly ILogger<SQLiteDatabaseService> _logger;
        private readonly IGraphProcessingService _graphProcessingService;

        public SQLiteDatabaseService(ISqlDataAccess db, ILogger<SQLiteDatabaseService> logger, IGraphProcessingService graphProcessingService)
        {
            _db = db;
            _logger = logger;
            _graphProcessingService = graphProcessingService;
        }

        public void CreateSnowMedSQLiteDb(string path)
        {
            // Check if the path is null
            if (path == null)
            {
                Console.WriteLine("Provided path is null. Please provide a valid path.");
                return; // Return from the method, as the path is null
            }

            // Check if the file already exists
            if (File.Exists(path))
            {
                try
                {
                    // Try to delete the file
                    File.Delete(path);
                    Console.WriteLine($"\nExisting file {path} deleted.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error deleting file {path}: {ex.Message}");
                    return; // Return from the method, as there was an error deleting the file
                }
            }

            // Call the CreateSQLiteDatabase method to create the new database
            SQLiteDBHelper.CreateSQLiteDatabase(path, typeof(ConceptModel), typeof(DescriptionModel), typeof(RelationshipModel), typeof(LanguageRefsetModel));
        }

        public async Task<bool> WriteConceptData(IEnumerable<ConceptModel> data)
        {
            string sql = @"INSERT INTO Concept (Id, EffectiveTime, Active, ModuleId, DefinitionStatusId) VALUES (@Id, @EffectiveTime, @Active, @ModuleId, @DefinitionStatusId)";

            await _db.InsertData(sql, data, "Default");

            return true;
        }

        public async Task<bool> WriteDescriptionData(IEnumerable<DescriptionModel> data)
        {
            string sql = @"INSERT INTO Description (Id, EffectiveTime, Active, ModuleId, ConceptId, LanguageCode, TypeId, Term, CaseSignificanceId) 
                         VALUES (@Id, @EffectiveTime, @Active, @ModuleId, @ConceptId, @LanguageCode, @TypeId, @Term, @CaseSignificanceId)";

            await _db.InsertData(sql, data, "Default");

            return true;
        }

        public async Task<bool> WriteRelationshipData(IEnumerable<RelationshipModel> data)
        {
            string sql = @"INSERT INTO Relationship (Id, EffectiveTime, Active, ModuleId, SourceId, DestinationId, RelationshipGroup, TypeId, CharacteristicTypeId, ModifierId) 
                         VALUES (@Id, @EffectiveTime, @Active, @ModuleId, @SourceId, @DestinationId, @RelationshipGroup, @TypeId, @CharacteristicTypeId, @ModifierId)";

            await _db.InsertData(sql, data, "Default");

            return true;
        }

        public async Task<IEnumerable<RelationshipModel>> GetRelationshipData()
        {
            string sql = "SELECT * FROM Relationship";
            var result = await _db.LoadData<RelationshipModel, dynamic>(sql, new { }, "Default");
            return result;
        }


        public async Task<bool> WriteLanguageRefsetData(IEnumerable<LanguageRefsetModel> data)
        {
            string sql = @"INSERT INTO LanguageRefset (Id, EffectiveTime, Active, ModuleId, RefsetId, ReferencedComponentId, AcceptabilityId) 
                         VALUES (@Id, @EffectiveTime, @Active, @ModuleId, @RefsetId, @ReferencedComponentId, @AcceptabilityId)";

            await _db.InsertData(sql, data, "Default");

            return true;
        }

        public async Task GenerateTransitiveClosureTable(IEnumerable<RelationshipModel> relationships, ShellProgressBar.IProgressBar pbar)
        {
            var stopwatch = Stopwatch.StartNew();

            pbar.Message = "Computing Transitive Closure table (Parallel)";
            stopwatch.Restart();
            var transitiveClosureParallel = await _graphProcessingService.ComputeTransitiveClosureAsync(relationships, pbar);
            stopwatch.Stop();
            pbar.Tick($"Computing Transitive Closure table (Parallel) took {stopwatch.ElapsedMilliseconds} milliseconds");
            await Task.Delay(500);

            pbar.Message = "Creating Transitive Closure table in SQLite database";
            await CreateTransitiveClosureTable("Default");
            await Task.Delay(500);

            pbar.Message = "Write to Transitive Closure table in SQLite database";
            await WriteTransitiveClosureToDB(transitiveClosureParallel, "Default");
            pbar.Tick("Write to Transitive Closure table in SQLite database - completed");
            await Task.Delay(500);

            pbar.Message = "Creating indexes";
            await CreateIndexes("Default", pbar);
            pbar.Tick("Creating indexes - completed");
            await Task.Delay(500);

            pbar.Message = "Creating views";
            await CreateViews("Default");
            pbar.Tick("Creating views - completed");
            await Task.Delay(500);
        }

        /// <summary>
        /// Asynchronously creates indexes on the Description, LanguageRefset, Relationship, and TransitiveClosure tables to improve query performance.
        /// </summary>
        /// <remarks>
        /// This method will issue CREATE INDEX commands for indexes that do not already exist. 
        /// Indexes are created on various columns of the Description, LanguageRefset, Relationship, and TransitiveClosure tables, based on the columns used in common queries. 
        /// Indexes can significantly improve read performance by reducing the amount of time it takes to look up values.
        /// However, they also consume additional storage space. As this method is intended for a read-only database, the trade-off is generally favorable.
        /// </remarks>
        /// <param name="connectionStringName">The name of the connection string for the database where the indexes will be created.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains no value.</returns>
        private async Task CreateIndexes(string connectionStringName, ShellProgressBar.IProgressBar pbar)
        {
            string[] indexCommands = new string[]
            {
                "DROP INDEX IF EXISTS idx_description_conceptid;",
                "CREATE INDEX IF NOT EXISTS idx_description_conceptid ON Description(ConceptId);",
                "DROP INDEX IF EXISTS idx_description_id;",
                "CREATE INDEX IF NOT EXISTS idx_description_id ON Description (Id);",
                "DROP INDEX IF EXISTS idx_description_languagecode;",
                "CREATE INDEX IF NOT EXISTS idx_description_languagecode ON Description (LanguageCode);",
                "DROP INDEX IF EXISTS idx_description_term;",
                "CREATE INDEX IF NOT EXISTS idx_description_term ON Description (Term);",
                "DROP INDEX IF EXISTS idx_langrefset_acceptabilityId;",
                "CREATE INDEX IF NOT EXISTS idx_langrefset_acceptabilityId ON LanguageRefset (AcceptabilityId);",
                "DROP INDEX IF EXISTS idx_langrefset_referencedcomponentid;",
                "CREATE INDEX IF NOT EXISTS idx_langrefset_referencedcomponentid ON LanguageRefset (ReferencedComponentId);",
                "DROP INDEX IF EXISTS idx_relationship_destinationid;",
                "CREATE INDEX IF NOT EXISTS idx_relationship_destinationid ON Relationship(DestinationId);",
                "DROP INDEX IF EXISTS idx_relationship_sourceid;",
                "CREATE INDEX IF NOT EXISTS idx_relationship_sourceid ON Relationship(SourceId);",
                "DROP INDEX IF EXISTS idx_transitiveclosure_destinationid;",
                "CREATE INDEX IF NOT EXISTS idx_transitiveclosure_destinationid ON TransitiveClosure(DestinationId);",
                "DROP INDEX IF EXISTS idx_transitiveclosure_sourceid;",
                "CREATE INDEX IF NOT EXISTS idx_transitiveclosure_sourceid ON TransitiveClosure(SourceId);",
            };

            pbar.MaxTicks += indexCommands.Length;

            foreach (var sql in indexCommands)
            {
                pbar.Tick(sql);
                await _db.SaveData(sql, new { }, connectionStringName);
            }
        }

        private async Task CreateViews(string connectionStringName)
        {
            string[] sqlCommands = new string[]
            {
                "DROP VIEW IF EXISTS Subsumption;",
                "CREATE VIEW Subsumption AS SELECT tc.SourceId, ds.Term as SourceTerm, tc.DestinationId, dt.Term as TargetTerm FROM TransitiveClosure tc LEFT JOIN Description ds ON tc.SourceId = ds.ConceptId LEFT JOIN Description dt ON tc.DestinationId = dt.ConceptId;",
            };


            foreach (var sql in sqlCommands)
            {
                await _db.SaveData(sql, new { }, connectionStringName);
            }
        }


        public async Task WriteTransitiveClosureToDB(Dictionary<long, HashSet<long>> transitiveClosure, string connectionStringName)
        {
            var dataToInsert = transitiveClosure.SelectMany(kvp => kvp.Value.Select(destinationId => new TransitiveClosureModel
            {
                SourceId = kvp.Key,
                DestinationId = destinationId
            }));

            string sql = @"
                        INSERT INTO TransitiveClosure
                        (
                            SourceId,
                            DestinationId
                        )
                        VALUES
                        (
                            @SourceId,
                            @DestinationId
                        );";

            await _db.InsertData(sql, dataToInsert, connectionStringName);
        }

        private async Task CreateTransitiveClosureTable(string connectionStringName)
        {
            string[] sqlCommands = new string[]
            {
                "DROP TABLE IF EXISTS TransitiveClosure;",
                @"CREATE TABLE IF NOT EXISTS TransitiveClosure
                  (
                      SourceId INTEGER NOT NULL,
                      DestinationId INTEGER NOT NULL
                  );"
            };

            foreach (var sql in sqlCommands)
            {
                await _db.SaveData(sql, new { }, connectionStringName);
            }
        }
    }
}
