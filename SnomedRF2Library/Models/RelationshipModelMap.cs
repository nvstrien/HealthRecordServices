using CsvHelper.Configuration;

using SnomedRF2Library.Converters;

namespace SnomedRF2Library.Models
{
    public class RelationshipModelMap : ClassMap<RelationshipModel>
    {
        public RelationshipModelMap()
        {
            Map(m => m.Id).Name("id").Index(0);
            Map(m => m.EffectiveTime).Name("effectiveTime").Index(1).TypeConverter<SnomedDateTimeConverter>();
            Map(m => m.Active).Name("active").Index(2).TypeConverter<IntegerToBooleanConverter>();
            Map(m => m.ModuleId).Name("moduleId").Index(3);
            Map(m => m.SourceId).Name("sourceId").Index(4);
            Map(m => m.DestinationId).Name("destinationId").Index(5);
            Map(m => m.RelationshipGroup).Name("relationshipGroup").Index(6);
            Map(m => m.TypeId).Name("typeId").Index(7);
            Map(m => m.CharacteristicTypeId).Name("characteristicTypeId").Index(8);
            Map(m => m.ModifierId).Name("modifierId").Index(8);
        }
    }
}


