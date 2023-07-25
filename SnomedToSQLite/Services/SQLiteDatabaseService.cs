﻿using System.Diagnostics;

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

        public async Task<bool> WriteLanguageRefsetData(IEnumerable<LanguageRefsetModel> data)
        {
            string sql = @"INSERT INTO LanguageRefset (Id, EffectiveTime, Active, ModuleId, RefsetId, ReferencedComponentId, AcceptabilityId) 
                         VALUES (@Id, @EffectiveTime, @Active, @ModuleId, @RefsetId, @ReferencedComponentId, @AcceptabilityId)";

            await _db.InsertData(sql, data, "Default");

            return true;
        }

        public async Task GenerateTransitiveClosureTable(IEnumerable<RelationshipModel> relationships, ShellProgressBar.IProgressBar pbar)
        {
            pbar.Message = "Creating Adjacency Matrix";
            var stopwatch = Stopwatch.StartNew();
            var adjacencyMatrix = _graphProcessingService.CreateAdjacencyMatrix(relationships);
            stopwatch.Stop();
            pbar.Tick($"Creating Adjacency Matrix took {stopwatch.ElapsedMilliseconds} milliseconds");
            await Task.Delay(500);

            pbar.Message = "Computing Transitive Closure table (Parallel)";
            stopwatch.Restart();
            var transitiveClosureParallel = _graphProcessingService.ComputeTransitiveClosureParallel(adjacencyMatrix);
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
            await CreateIndexes("Default");
            pbar.Tick("Creating indexes - completed");
            await Task.Delay(500);

            pbar.Message = "Creating views";
            await CreateViews("Default");
            pbar.Tick("Creating views - completed");
            await Task.Delay(500);
        }

        public async Task GenerateTransitiveClosureTable(IEnumerable<RelationshipModel> relationships, IEnumerable<ConceptModel> concepts, IEnumerable<DescriptionModel> descriptions, IEnumerable<LanguageRefsetModel> languageRefsets, ShellProgressBar.IProgressBar pbar)
        {
            pbar.Message = "Creating Adjacency Matrix";
            var stopwatch = Stopwatch.StartNew();
            var adjacencyMatrix = _graphProcessingService.CreateAdjacencyMatrix(relationships, concepts, descriptions, languageRefsets);
            stopwatch.Stop();
            pbar.Tick($"Creating Adjacency Matrix took {stopwatch.ElapsedMilliseconds} milliseconds");
            await Task.Delay(500);

            pbar.Message = "Computing Transitive Closure table (Parallel)";
            stopwatch.Restart();
            var transitiveClosureParallel = _graphProcessingService.ComputeTransitiveClosureParallel(adjacencyMatrix);
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
            await CreateIndexes("Default");
            pbar.Tick("Creating indexes - completed");
            await Task.Delay(500);

            pbar.Message = "Creating views";
            await CreateViews("Default");
            pbar.Tick("Creating views - completed");
            await Task.Delay(500);
        }

        private async Task CreateIndexes(string connectionStringName)
        {
            string[] indexCommands = new string[]
            {
                "CREATE INDEX IF NOT EXISTS idx_relationship_sourceid ON Relationship(SourceId);",
                "CREATE INDEX IF NOT EXISTS idx_relationship_destinationid ON Relationship(DestinationId);",
                "CREATE INDEX IF NOT EXISTS idx_transitiveclosure_sourceid ON TransitiveClosure(SourceId);",
                "CREATE INDEX IF NOT EXISTS idx_transitiveclosure_destinationid ON TransitiveClosure(DestinationId);",
                "CREATE INDEX IF NOT EXISTS idx_description_conceptid ON Description(ConceptId);"
            };

            foreach (var sql in indexCommands)
            {
                await _db.SaveData(sql, new { }, connectionStringName);
            }
        }

        private async Task CreateViews(string connectionStringName)
        {
            string[] sqlCommands = new string[]
            {
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
            string sql = @"
                        CREATE TABLE IF NOT EXISTS TransitiveClosure
                        (
                            SourceId INTEGER NOT NULL,
                            DestinationId INTEGER NOT NULL
                        );";

            await _db.SaveData(sql, new { }, connectionStringName);
        }
    }
}
