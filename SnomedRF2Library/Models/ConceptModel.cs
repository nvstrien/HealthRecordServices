using System;
using System.Collections.Generic;

using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CsvHelper.Configuration.Attributes;

using SnomedRF2Library.Converters;

namespace SnomedRF2Library.Models
{
    public class ConceptModel
    {
        public long Id { get; set; }
        public DateTime EffectiveTime { get; set; }
        public bool Active { get; set; }
        public long ModuleId { get; set; }
        public long DefinitionStatusId { get; set; }
    }
}


