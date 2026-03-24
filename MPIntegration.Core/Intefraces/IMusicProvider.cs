using MPIntegration.Core.Models;

namespace MPIntegration.Core.Intefraces
{
    public interface IMusicProvider
    {
        Task<List<Track>> GetTracksAsync();
    }
}
