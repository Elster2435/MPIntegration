using System.Diagnostics;

namespace MPIntegration.Infrastructure.Services
{
    public class TrackStorageService
    {
        private readonly string _trackFolderPath;
        public TrackStorageService()
        {
            var appFolder = AppDomain.CurrentDomain.BaseDirectory;
            _trackFolderPath = Path.Combine(appFolder, "Tracks");
        }
        public string GetTracksFolderPath()
        {
            return _trackFolderPath;
        }
        public void EnsureTracksFolderExists()
        {
            if (!Directory.Exists(_trackFolderPath))
            {
                Directory.CreateDirectory(_trackFolderPath);
            }
        }
        public List<string> AddTracks(IEnumerable<string> sourceFilePaths)
        {
            EnsureTracksFolderExists();
            var addedFiles = new List<string>();
            foreach (var sourceFilePath in sourceFilePaths)
            {
                if (string.IsNullOrWhiteSpace(sourceFilePath) || !File.Exists(sourceFilePath))
                    continue;
                var fileName = Path.GetFileName(sourceFilePath);
                var destinationPath = Path.Combine(_trackFolderPath, fileName);
                destinationPath = GetUniqueFilePath(destinationPath);
                File.Copy(sourceFilePath, destinationPath, false);
                addedFiles.Add(destinationPath);
            }
            return addedFiles;
        }
        public bool DeleteTrack(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                return false;
            if (!File.Exists(filePath))
                return false;
            File.Delete(filePath);
            return true;
        }
        public void OpenTracksFolder()
        {
            EnsureTracksFolderExists();
            Process.Start(new ProcessStartInfo
            {
                FileName = _trackFolderPath,
                UseShellExecute = true
            });
        }
        private string GetUniqueFilePath(string filePath)
        {
            if (!File.Exists(filePath))
                return filePath;
            var directory = Path.GetDirectoryName(filePath) ?? string.Empty;
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(filePath);
            var extension = Path.GetExtension(filePath);
            int counter = 1;
            string newPath;
            do
            {
                newPath = Path.Combine(directory, $"{fileNameWithoutExtension}_{counter}{extension}");
                counter++;
            }
            while (File.Exists(newPath));
            return newPath;
        }
    }
}
