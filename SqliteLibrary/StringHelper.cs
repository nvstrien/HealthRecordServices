using System.Globalization;

namespace SqliteLibrary
{
    internal static class StringHelper
    {

        public static string ConvertBoolToString(bool value)
        {
            return value ? "true" : "false";
        }

        public static List<string> AddSingleQuotes(List<string> values)
        {
            if (values == null || values.Count == 0)
                throw new ArgumentException("Invalid input values.");

            return values.Select(value =>
            {
                if (value.StartsWith("'") || value.EndsWith("'"))
                    throw new ArgumentException("Input values must not contain single quotes.");

                return $"'{value}'";
            }).ToList();
        }

        public static string DetectDateFormat(string dateString)
        {
            var dateFormats = new List<string>
            {
                // Year-Month-Day formats
                "yyyy-MM-dd",
                "yyyy/MM/dd",
                "yyyy.MM.dd",
                "yyyyMMdd",

                // Day-Month-Year formats
                "dd-MM-yyyy",
                "dd/MM/yyyy",
                "dd.MM.yyyy",
                "ddMMyyyy",

                // Month-Day-Year formats
                "MM-dd-yyyy",
                "MM/dd/yyyy",
                "MM.dd.yyyy",
                "MMddyyyy",

                // Year-Month-Day formats with time
                "yyyy-MM-dd HH:mm:ss",
                "yyyy-MM-ddTHH:mm:ss",
                "yyyy/MM/dd HH:mm:ss",
                "yyyy.MM.dd HH:mm:ss",

                // Day-Month-Year formats with time
                "dd-MM-yyyy HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss",
                "dd.MM.yyyy HH:mm:ss",

                // Month-Day-Year formats with time
                "MM-dd-yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss",
                "MM.dd.yyyy HH:mm:ss",

                // Formats with abbreviated month names
                "dd-MMM-yyyy",
                "MMM-dd-yyyy",
                "yyyy-MMM-dd",

                // Other common formats
                "yyyy-M-d",
                "M-d-yyyy",
                "d-M-yyyy",
                "d-M-yyyy H:mm:ss"

                // Add more formats as needed
            };

            foreach (string format in dateFormats)
            {
                if (DateTime.TryParseExact(dateString, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime tempDate))
                {
                    return format;
                }
            }

            return "";
        }
    }
}
