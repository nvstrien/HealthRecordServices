using System.Globalization;

using CsvHelper.Configuration;
using CsvHelper;
using CsvHelper.TypeConversion;

namespace SnomedRF2Library.Converters
{
    public class SnomedDateTimeConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (text == null)
            {
                return default(DateTime);
            }

            return DateTime.ParseExact(text, "yyyyMMdd", CultureInfo.InvariantCulture);
        }
    }
}
