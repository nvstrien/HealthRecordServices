using CsvHelper.Configuration;

using SnomedRF2Library.Converters;

namespace SnomedRF2Library.Models
{
    public class ConceptModelMap : ClassMap<ConceptModel>
    {
        public ConceptModelMap()
        {
            Map(m => m.Id).Name("id");
            Map(m => m.EffectiveTime).Name("effectiveTime").TypeConverter<SnomedDateTimeConverter>();
            Map(m => m.Active).Name("active").TypeConverter<IntegerToBooleanConverter>();
            Map(m => m.ModuleId).Name("moduleId");
            Map(m => m.DefinitionStatusId).Name("definitionStatusId");
        }
    }
}


