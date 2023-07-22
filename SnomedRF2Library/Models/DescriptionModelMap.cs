using CsvHelper.Configuration;

using SnomedRF2Library.Converters;

namespace SnomedRF2Library.Models
{
    public class DescriptionModelMap : ClassMap<DescriptionModel>
    {
        public DescriptionModelMap()
        {
            Map(m => m.Id).Name("id").Index(0);
            Map(m => m.EffectiveTime).Name("effectiveTime").Index(1).TypeConverter<SnomedDateTimeConverter>();
            Map(m => m.Active).Name("active").Index(2).TypeConverter<IntegerToBooleanConverter>();
            Map(m => m.ModuleId).Name("moduleId").Index(3);
            Map(m => m.ConceptId).Name("conceptId").Index(4);
            Map(m => m.LanguageCode).Name("languageCode").Index(5);
            Map(m => m.TypeId).Name("typeId").Index(6);
            Map(m => m.Term).Name("term").Index(7);
            Map(m => m.CaseSignificanceId).Name("caseSignificanceId").Index(8);
        }
    }
}