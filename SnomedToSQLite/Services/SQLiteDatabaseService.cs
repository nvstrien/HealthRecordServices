using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SnomedRF2Library.Models;

using SqliteLibrary;

namespace SnomedToSQLite.Services
{
    public class SQLiteDatabaseService : ISQLiteDatabaseService
    {
        private readonly ISqlDataAccess _db;

        public SQLiteDatabaseService(ISqlDataAccess db)
        {
            _db = db;
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
                    Console.WriteLine($"Existing file {path} deleted.");
                }
                catch (IOException ex)
                {
                    Console.WriteLine($"Error deleting file {path}: {ex.Message}");
                    return; // Return from the method, as there was an error deleting the file
                }
            }

            // Call the CreateSQLiteDatabase method to create the new database
            SQLiteDBHelper.CreateSQLiteDatabase(path, typeof(ConceptModel), typeof(DescriptionModel), typeof(RelationshipModel));
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
    }
}
