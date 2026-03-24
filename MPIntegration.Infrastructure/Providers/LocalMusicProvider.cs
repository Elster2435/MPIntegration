using MPIntegration.Core.Intefraces;
using MPIntegration.Core.Models;

namespace MPIntegration.Infrastructure.Providers
{
    public class LocalMusicProvider : IMusicProvider
    {
        private readonly string _folderPath;
        public LocalMusicProvider(string folderPath)
        {
            _folderPath = folderPath;
        }
        public Task<List<Track>> GetTracksAsync()
        {
            var tracks = new List<Track>();
            if (string.IsNullOrWhiteSpace(_folderPath) || !Directory.Exists(_folderPath))
            {
                return Task.FromResult(tracks);
            }
            var files = Directory.GetFiles(_folderPath, "*.mp3", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                var fileName = Path.GetFileNameWithoutExtension(file);
                string title = fileName;
                string artist = "Неизвестный исполнитель";
                if (fileName.Contains(" - "))
                {
                    var parts = fileName.Split(" - ", 2);
                    title = parts[1];
                    artist = parts[0];
                }
                tracks.Add(new Track
                {
                    Title = title,
                    Artist = artist,
                    FilePath = file,
                    Duration = TimeSpan.Zero
                });
            }
            return Task.FromResult(tracks);
        }
    }
}
