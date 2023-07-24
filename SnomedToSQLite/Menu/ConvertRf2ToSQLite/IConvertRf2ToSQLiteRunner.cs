namespace SnomedToSQLite.Menu.ConvertRf2ToSQLite
{
    public interface IConvertRf2ToSQLiteRunner
    {
        Task ConvertRf2ToSQLIte(string fullConceptPath, string fullDescriptionPath, string fullRelationshipPath);
        Task ConvertRf2ToSQLIte(string fullConceptPath, List<string> fullDescriptionPaths, string fullRelationshipPath);
    }
}