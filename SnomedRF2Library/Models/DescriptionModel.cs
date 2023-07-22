namespace SnomedRF2Library.Models
{
    public class DescriptionModel
    {
        public long Id { get; set; }
        public DateTime EffectiveTime { get; set; }
        public bool Active { get; set; }
        public long ModuleId { get; set; }
        public long ConceptId { get; set; }
        public string LanguageCode { get; set; }
        public long TypeId { get; set; }
        public string Term { get; set; }
        public long CaseSignificanceId { get; set; }
    }
}


