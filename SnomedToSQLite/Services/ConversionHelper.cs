using Microsoft.Extensions.Logging;

using static SnomedRF2Library.Enums.Enumeration;

namespace SnomedToSQLite.Services
{
    public partial class ConversionHelper : IConversionHelper
    {
        private readonly ILogger<ConversionHelper> _logger;

        public ConversionHelper(ILogger<ConversionHelper> logger)
        {
            _logger = logger;
        }

        public bool IsValidDirectory(string? path)
        {
            return Directory.Exists(path);
        }

        public string GetDbName(ConversionType conversionType)
        {
            return conversionType switch
            {
                ConversionType.FullConversion => "SnomedFull.db",
                ConversionType.SnapshotConversion => "SnomedSnapshot.db",
                _ => throw new ArgumentException(message: "Invalid enum value", paramName: nameof(conversionType)),
            };
        }

        public void WriteColoredMessage(string message, ConsoleColor color)
        {
            Console.ForegroundColor = color;
            Console.WriteLine(message);
            Console.ResetColor();
        }

    }
}
