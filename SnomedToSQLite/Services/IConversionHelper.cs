using SnomedRF2Library.Enums;

namespace SnomedToSQLite.Services
{
    public interface IConversionHelper
    {
        string GetDbName(Enumeration.ConversionType conversionType);
        bool IsValidDirectory(string? path);
        void WriteColoredMessage(string message, ConsoleColor color);
    }
}