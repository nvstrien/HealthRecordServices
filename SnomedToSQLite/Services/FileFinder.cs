namespace SnomedToSQLite.Services
{
    public class FileFinder : IFileFinder
    {
        public string FindFileInDirectory(string directoryPath, string pattern)
        {
            string[] files = Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                Console.WriteLine($"No file found matching pattern {pattern} in directory {directoryPath}.");
                return null;
            }
            else
            {
                return files[0]; // Return the first file matching the pattern
            }
        }

        public List<string> FindFilesInDirectory(string directoryPath, string pattern)
        {
            string[] files = Directory.GetFiles(directoryPath, pattern, SearchOption.AllDirectories);

            if (files.Length == 0)
            {
                Console.WriteLine($"No files found matching pattern {pattern} in directory {directoryPath}.");
                return null;
            }
            else
            {
                return files.ToList(); // Return all files matching the pattern
            }
        }
    }

}

