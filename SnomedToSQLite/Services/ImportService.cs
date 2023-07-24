using CsvHelper;
using System.Globalization;

using SnomedRF2Library.Models;
using CsvHelper.Configuration;

namespace SnomedToSQLite.Services
{
    public class ImportService : IImportService
    {
        public void SomeMethod()
        {
            Console.WriteLine("Hello World!");
        }

        public IEnumerable<ConceptModel> ImportRf2Concept(string rf2source)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t"
            };

            using var reader = new StreamReader(rf2source);
            
            using var csv = new CsvReader(reader, config);
            
            csv.Context.RegisterClassMap<ConceptModelMap>();
            
            var records = csv.GetRecords<ConceptModel>().ToList();

            return records;
        }

        public IEnumerable<DescriptionModel> ImportRf2Description(string rf2source)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t",
                Mode = CsvMode.NoEscape
            };

            using var reader = new StreamReader(rf2source);

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<DescriptionModelMap>();

            try
            {
                var records = csv.GetRecords<DescriptionModel>().ToList();
                return records;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public IEnumerable<RelationshipModel> ImportRf2Relationship(string rf2source)
        {
            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                Delimiter = "\t"
            };

            using var reader = new StreamReader(rf2source);

            using var csv = new CsvReader(reader, config);

            csv.Context.RegisterClassMap<RelationshipModelMap>();

            var records = csv.GetRecords<RelationshipModel>().ToList();

            return records;
        }

    }

}

