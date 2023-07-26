using ShellProgressBar;

using SnomedRF2Library.Models;

namespace SnomedToSQLite.Services
{
    public interface IGraphProcessingService
    {
        Task<Dictionary<long, HashSet<long>>> ComputeTransitiveClosureAsync(IEnumerable<RelationshipModel> relationships, IProgressBar pbar);
    }
}