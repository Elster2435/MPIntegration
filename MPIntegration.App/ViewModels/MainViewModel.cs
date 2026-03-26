using MPIntegration.App.Commands;
using MPIntegration.App.Services;
using MPIntegration.Core.Models;
using MPIntegration.Infrastructure.Providers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Threading;
using System.Xml.Serialization;

namespace MPIntegration.App.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private readonly AudioPlayerService _audioPlayerService;
        private readonly DispatcherTimer _timer;
        private List<Track> _allTracks = new();
        private string _searchText = string.Empty;
        private string _currentTrackText = "Текущий трек: ничего не выбрано";
        private Track? _selectedTrack;
        private double _volume = 50;
        private double _trackPosition;
        private double _trackMaximum = 100;
        private string _currentTimeText = "00:00";
        private string _totalTimeText = "00:00";
        private bool _isDraggingSlider;
        public ObservableCollection<Track> Tracks { get; } = new();
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyTrackFilter();
            }
        }
        public string CurrentTrackText
        {
            get => _currentTrackText;
            set
            {
                _currentTrackText = value;
                OnPropertyChanged();
            }
        }
        public Track? SelectedTrack
        {
            get => _selectedTrack;
            set
            {
                _selectedTrack = value;
                OnPropertyChanged();
            }
        }
        public double Volume
        {
            get => _volume;
            set
            {
                _volume = value;
                OnPropertyChanged();
                _audioPlayerService.SetVolume(_volume / 100.0);
            }
        }
        public double TrackPosition
        {
            get => _trackPosition;
            set
            {
                _trackPosition = value;
                OnPropertyChanged();
            }
        }
        public double TrackMaximum
        {
            get => _trackMaximum;
            set
            {
                _trackMaximum = value;
                OnPropertyChanged();
            }
        }
        public string CurrentTimeText
        {
            get => _currentTimeText;
            set
            {
                _currentTimeText = value;
                OnPropertyChanged();
            }
        }
        public string TotalTimeText
        {
            get => _totalTimeText;
            set
            {
                _totalTimeText = value;
                OnPropertyChanged();
            }
        }
        public ICommand SelectFolderCommand { get; }
        public ICommand PlayCommand { get; }
        public ICommand PauseCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand NextCommand { get; }
        public ICommand PrevCommand { get; }
        public ICommand TrackDoubleClickCommand { get; }
        public MainViewModel()
        {
            _audioPlayerService = new AudioPlayerService();
            _audioPlayerService.PlaybackEnded += AudioPlayerService_PlaybackEnded;
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
            SelectFolderCommand = new RelayCommand(async _ => await SelectFolderAsync());
            PlayCommand = new RelayCommand(_ => Play());
            PauseCommand = new RelayCommand(_ => Pause());
            StopCommand = new RelayCommand(_ => Stop());
            NextCommand = new RelayCommand(_ =>  Next());
            PrevCommand = new RelayCommand(_ =>  Prev());
            TrackDoubleClickCommand = new RelayCommand(_ => PlaySelectedTrack());
            Volume = 50;
        }
        private async Task SelectFolderAsync()
        {
            try
            {
                using var dialog = new FolderBrowserDialog();
                var result = dialog.ShowDialog();
                if (result != DialogResult.OK)
                    return;
                var provider = new LocalMusicProvider(dialog.SelectedPath);
                _allTracks = await provider.GetTracksAsync();
                ApplyTrackFilter();
                CurrentTrackText = $"Загружено треков: {_allTracks.Count}";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка загрузки папки: {ex.Message}");
            }
        }
        private void ApplyTrackFilter()
        {
            try
            {
                IEnumerable<Track> filteredTracks = _allTracks;
                if (!string.IsNullOrWhiteSpace(SearchText))
                {
                    filteredTracks = _allTracks.Where(t =>
                    (!string.IsNullOrWhiteSpace(t.Title) &&
                    t.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(t.Artist) &&
                    t.Artist.Contains(SearchText, StringComparison.OrdinalIgnoreCase)));
                }
                Tracks.Clear();
                foreach (var track in filteredTracks)
                {
                    Tracks.Add(track);
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка фильтрации: {ex.Message}");
            }
        }
        private void Play()
        {
            try
            {
                if (SelectedTrack != null)
                {
                    PlaySelectedTrack();
                    return;
                }
                _audioPlayerService.Resume();
                _timer.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка воспроизведения: {ex.Message}");
            }
        }
        private void PlaySelectedTrack()
        {
            try
            {
                if (SelectedTrack == null)
                    return;
                _audioPlayerService.Play(SelectedTrack);
                _timer.Start();
                CurrentTrackText = $"Сейчас играет: {SelectedTrack.Artist} - {SelectedTrack.Title}";
                CurrentTimeText = "00:00";
                TotalTimeText = "00:00";
                TrackPosition = 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка воспроизведения: {ex.Message}");
            }
        }
        private void Pause()
        {
            try
            {
                _audioPlayerService.Pause();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка паузы: {ex.Message}");
            }
        }
        private void Stop()
        {
            try
            {
                _audioPlayerService.Stop();
                _timer.Stop();
                TrackPosition = 0;
                CurrentTimeText = "00:00";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка остановки: {ex.Message}");
            }
        }
        private void Next()
        {
            try
            {
                if (Tracks.Count == 0)
                    return;
                if (SelectedTrack == null)
                {
                    SelectedTrack = Tracks[0];
                    Play();
                    return;
                }
                int currentIndex = Tracks.IndexOf(SelectedTrack);
                if (currentIndex == -1 || currentIndex >= Tracks.Count - 1)
                    return;
                SelectedTrack = Tracks[currentIndex + 1];
                PlaySelectedTrack();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка перехода к следующему треку: {ex.Message}");
            }
        }
        private void Prev()
        {
            try
            {
                if (Tracks.Count == 0)
                    return;
                if (SelectedTrack == null)
                {
                    SelectedTrack = Tracks[0];
                    Play();
                    return;
                }
                int currentIndex = Tracks.IndexOf(SelectedTrack);
                if (currentIndex <= 0)
                    return;
                SelectedTrack = Tracks[currentIndex - 1];
                PlaySelectedTrack();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка перехода к предыдущему треку: {ex.Message}");
            }
        }
        private void AudioPlayerService_PlaybackEnded(object? sender, EventArgs e)
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                PlayNextTrackAutomaticaly();
            });
        }
        private void PlayNextTrackAutomaticaly()
        {
            try
            {
                if (Tracks.Count ==  0 || SelectedTrack == null)
                {
                    _timer.Stop();
                    return;
                }
                int currentIndex = Tracks.IndexOf(SelectedTrack);
                if (currentIndex == -1)
                {
                    _timer.Stop();
                    return;
                }
                if (currentIndex >= Tracks.Count - 1)
                {
                    _timer.Stop();
                    TrackPosition = 0;
                    CurrentTimeText = "00:00";
                    CurrentTrackText = "Воспроизведение завершено";
                    return;
                }
                SelectedTrack = Tracks[currentIndex + 1];
                PlaySelectedTrack();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка автоперехода: {ex.Message}");
            }
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (!_audioPlayerService.HasNaturalDuration)
                    return;
                var position = _audioPlayerService.GetPosition();
                var duration = _audioPlayerService.GetNaturalDuration();
                CurrentTimeText = FormatTime(position);
                TotalTimeText = FormatTime(duration);
                if (!_isDraggingSlider && duration.TotalSeconds > 0)
                {
                    TrackMaximum = duration.TotalSeconds;
                    TrackPosition = position.TotalSeconds;
                }
            }
            catch { }
        }
        public void BeginSliderDrag()
        {
            _isDraggingSlider = true;
        }
        public void EndSliderDrag()
        {
            try
            {
                if (!_audioPlayerService.HasNaturalDuration)
                    return;
                var newPosition = TimeSpan.FromSeconds(TrackPosition);
                _audioPlayerService.SetPosition(newPosition);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка перемотки: {ex.Message}");
            }
            finally
            {
                _isDraggingSlider = false;
            }
        }
        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        }
    }
}
