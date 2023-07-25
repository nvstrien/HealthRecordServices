using CsvHelper.Configuration;

using SnomedRF2Library.Converters;

namespace SnomedRF2Library.Models
{
    public class LanguageRefsetModelMap : ClassMap<LanguageRefsetModel>
    {
        public LanguageRefsetModelMap()
        {
            Map(m => m.Id).Name("id").Index(0);
            Map(m => m.EffectiveTime).Name("effectiveTime").Index(1).TypeConverter<SnomedDateTimeConverter>();
            Map(m => m.Active).Name("active").Index(2).TypeConverter<IntegerToBooleanConverter>();
            Map(m => m.ModuleId).Name("moduleId").Index(3);
            Map(m => m.RefsetId).Name("refsetId").Index(4);
            Map(m => m.ReferencedComponentId).Name("referencedComponentId").Index(5);
            Map(m => m.AcceptabilityId).Name("acceptabilityId").Index(6);
        }
    }
}
