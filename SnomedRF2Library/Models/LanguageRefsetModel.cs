namespace SnomedRF2Library.Models
{
    public class LanguageRefsetModel
    {
        public Guid Id { get; set; }
        public DateTime EffectiveTime { get; set; }
        public bool Active { get; set; }
        public long ModuleId { get; set; }
        public long RefsetId { get; set; }
        public long ReferencedComponentId { get; set; }
        public long AcceptabilityId { get; set; }
    }
}
