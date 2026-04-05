namespace MPIntegration.Core.Models
{
    public class Track
    {
        public string Title { get; set; } = string.Empty;
        public string Artist { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public TimeSpan Duration { get; set; }
        public string DisplayPath => string.IsNullOrWhiteSpace(FilePath)
            ? string.Empty : $"Tracks\\{Path.GetFileName(FilePath)}";
    }
}
