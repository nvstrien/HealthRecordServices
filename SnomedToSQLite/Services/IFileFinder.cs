namespace SnomedToSQLite.Services
{
    public interface IFileFinder
    {
        string FindFileInDirectory(string directoryPath, string pattern);
        List<string> FindFilesInDirectory(string directoryPath, string pattern);
    }

}

