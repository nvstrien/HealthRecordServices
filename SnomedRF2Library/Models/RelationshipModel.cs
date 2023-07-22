namespace SnomedRF2Library.Models
{
    public class RelationshipModel
    {
        public long Id { get; set; }
        public DateTime EffectiveTime { get; set; }
        public bool Active { get; set; }
        public long ModuleId { get; set; }
        public long SourceId { get; set; }
        public long DestinationId { get; set; }
        public int RelationshipGroup { get; set; }
        public long TypeId { get; set; }
        public long CharacteristicTypeId { get; set; }
        public long ModifierId { get; set; }
    }
}


