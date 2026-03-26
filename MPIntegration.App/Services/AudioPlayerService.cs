using MPIntegration.Core.Models;
using System.Windows.Media;

namespace MPIntegration.App.Services
{
    public class AudioPlayerService
    {
        private readonly MediaPlayer _player = new();
        public Track? CurrentTrack { get; private set; }
        public event EventHandler? PlaybackEnded;
        public AudioPlayerService()
        {
            _player.MediaEnded += Player_MediaEnded;
        }
        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
        public void Play(Track track)
        {
            if (track == null || string.IsNullOrWhiteSpace(track.FilePath))
                return;
            CurrentTrack = track;
            _player.Open(new Uri(track.FilePath));
            _player.Play();
        }
        public void Pause() { _player.Pause(); }
        public void Resume() { _player.Play(); }
        public void Stop() { _player.Stop(); }
        public void SetVolume(double volume)
        {
            if (volume < 0)
                volume = 0;
            if (volume > 1)
                volume = 1;
            _player.Volume = volume;
        }
        public TimeSpan GetPosition()
        {
            return _player.Position;
        }
        public void SetPosition(TimeSpan position)
        {
            _player.Position = position;
        }
        public bool HasNaturalDuration =>
            _player.NaturalDuration.HasTimeSpan;
        public TimeSpan GetNaturalDuration()
        {
            return _player.NaturalDuration.HasTimeSpan
                ? _player.NaturalDuration.TimeSpan
                : TimeSpan.Zero;
        }
    }
}
