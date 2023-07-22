using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;

namespace SnomedRF2Library.Converters
{
    public class IntegerToBooleanConverter : DefaultTypeConverter
    {
        public override object ConvertFromString(string? text, IReaderRow row, MemberMapData memberMapData)
        {
            if (string.IsNullOrEmpty(text))
            {
                return false;
            }

            if (int.TryParse(text, out var result))
            {
                return result != 0;
            }

            throw new TypeConverterException(this, memberMapData, text, row.Context, $"Cannot convert '{text}' to Boolean.");
        }
    }
}
