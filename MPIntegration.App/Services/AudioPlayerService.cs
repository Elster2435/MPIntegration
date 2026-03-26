using MPIntegration.Core.Models;
using System.Windows.Media;

namespace MPIntegration.App.Services
{
    public enum PlayerState
    {
        Stopped,
        Playing,
        Paused
    }
    public class AudioPlayerService
    {
        private readonly MediaPlayer _player = new();
        public Track? CurrentTrack { get; private set; }
        public PlayerState State { get; private set; } = PlayerState.Stopped;
        public bool IsPlaying => State == PlayerState.Playing;
        public bool IsPaused => State == PlayerState.Paused;
        public bool IsStopped => State == PlayerState.Stopped;
        public event EventHandler? PlaybackEnded;
        public AudioPlayerService()
        {
            _player.MediaEnded += Player_MediaEnded;
        }
        private void Player_MediaEnded(object? sender, EventArgs e)
        {
            State = PlayerState.Stopped;
            PlaybackEnded?.Invoke(this, EventArgs.Empty);
        }
        public void Play(Track track)
        {
            if (track == null || string.IsNullOrWhiteSpace(track.FilePath))
                return;
            CurrentTrack = track;
            _player.Open(new Uri(track.FilePath));
            _player.Play();
            State = PlayerState.Playing;
        }
        public void Pause()
        {
            if (CurrentTrack == null)
                return;
            _player.Pause();
            State = PlayerState.Paused;
        }
        public void Resume()
        {
            if (CurrentTrack == null)
                return;
            _player.Play();
            State = PlayerState.Playing;
        }
        public void Stop() 
        { 
            _player.Stop();
            State = PlayerState.Stopped;
        }
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
