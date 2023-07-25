using static SnomedRF2Library.Enums.Enumeration;

namespace SnomedToSQLite.Menu.ConvertRf2ToSQLite
{
    public interface IConvertRf2ToSQLiteRunner
    {
        Task ConvertRf2ToSQLIte(string rootFilePath, string fullConceptPath, List<string> fullDescriptionPaths, string fullRelationshipPath, List<string> languageRefsetPaths, ConversionType conversionType);
        Task ExecuteConversion(string rootFilePath, ConversionType conversionType);
    }
}