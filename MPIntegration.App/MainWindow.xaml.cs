using MPIntegration.App.Services;
using MPIntegration.Core.Models;
using MPIntegration.Infrastructure.Providers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using System.Linq;
using System.Xml.Serialization;

namespace MPIntegration.App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<Track> _tracks = new();
        private readonly AudioPlayerService _audioPlayerService = new();
        private readonly DispatcherTimer _timer = new();
        private bool _isDraggingSlider = false;
        public MainWindow()
        {
            InitializeComponent();
            _timer.Interval = TimeSpan.FromMilliseconds(500);
            _timer.Tick += Timer_Tick;
            _audioPlayerService.PlaybackEnded += AudioPlayerService_PlaybackEnded;
            VolumeSlider.Value = 50;
            _audioPlayerService.SetVolume(0.5);
        }

        private async void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK)
            {
                return;
            }
            var provider = new LocalMusicProvider(dialog.SelectedPath);
            _tracks = await provider.GetTracksAsync();
            //TracksListBox.ItemsSource = _tracks;
            ApplyTrackFilter();
            CurrentTrackTextBlock.Text = $"Загружено треков: {_tracks.Count}";
        }

        private void TracksListBox_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (TracksListBox.SelectedItem is not Track selectedTrack)
                return;
            PlaySelectedTrack(selectedTrack);
        }

        private void PlayButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (TracksListBox.SelectedItem is Track selectedTrack)
                {
                    PlaySelectedTrack(selectedTrack);
                    return;
                }
                _audioPlayerService.Resume();
                _timer.Start();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка запуска: {ex.Message}");
            }
        }
        private void PauseButton_Click(object sender, RoutedEventArgs e)
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
        private void StopButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                _audioPlayerService.Stop();
                _timer.Stop();
                TrackPositionSlider.Value = 0;
                CurrentTimeTextBlock.Text = "00:00";
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка остановки: {ex.Message}");
            }
        }
        private void PrevButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_tracks.Count == 0)
                    return;
                if (_audioPlayerService.CurrentTrack == null)
                {
                    var firstTrack = _tracks[0];
                    TracksListBox.SelectedItem = firstTrack;
                    PlaySelectedTrack(firstTrack);
                    return;
                }
                int currentIndex = _tracks.IndexOf(_audioPlayerService.CurrentTrack);
                if (currentIndex <= 0)
                    return;
                var previousTrack = _tracks[currentIndex - 1];
                TracksListBox.SelectedItem = previousTrack;
                PlaySelectedTrack(previousTrack);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка перехода к предыдущему треку: {ex.Message}");
            }
        }
        private void NextButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (_tracks.Count == 0)
                    return;
                if ( _audioPlayerService.CurrentTrack == null)
                {
                    var firstTrack = _tracks[0];
                    TracksListBox.SelectedItem = firstTrack;
                    PlaySelectedTrack(firstTrack);
                    return;
                }
                int currentIndex = _tracks.IndexOf( _audioPlayerService.CurrentTrack);
                if (currentIndex == -1 || currentIndex >= _tracks.Count - 1)
                    return;
                var nextTrack = _tracks[currentIndex + 1];
                TracksListBox.SelectedItem = nextTrack;
                PlaySelectedTrack(nextTrack);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка перехода к следующему треку: {ex.Message}");
            }
        }
        private void PlaySelectedTrack(Track track)
        {
            try
            {
                _audioPlayerService.Play(track);
                _timer.Start();
                CurrentTrackTextBlock.Text = $"Сейчас играет: {track.Artist} - {track.Title}";
                CurrentTimeTextBlock.Text = "00:00";
                TotalTimeTextBlock.Text = "00:00";
                TrackPositionSlider.Value = 0;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка воспроизведения: {ex.Message}");
            }
        }
        private string FormatTime(TimeSpan time)
        {
            return $"{(int)time.TotalMinutes:00}:{time.Seconds:00}";
        }
        private void Timer_Tick(object? sender, EventArgs e)
        {
            try
            {
                if (!_audioPlayerService.HasNaturalDuration)
                    return;
                var position = _audioPlayerService.GetPosition();
                var duration = _audioPlayerService.GetNaturalDuration();
                CurrentTimeTextBlock.Text = FormatTime(position);
                TotalTimeTextBlock.Text = FormatTime(duration);
                if (!_isDraggingSlider && duration.TotalSeconds > 0)
                {
                    TrackPositionSlider.Maximum = duration.TotalSeconds;
                    TrackPositionSlider.Value = position.TotalSeconds;
                }
            }
            catch { }
        }
        private void TrackPositionSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

        }

        private void TrackPositionSlider_PreviewMouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            try
            {
                if (!_audioPlayerService.HasNaturalDuration)
                    return;

                var newPosition = TimeSpan.FromSeconds(TrackPositionSlider.Value);
                _audioPlayerService.SetPosition(newPosition);
                _isDraggingSlider = false;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка перемотки: {ex.Message}");
            }
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double volume = VolumeSlider.Value / 100;
            _audioPlayerService.SetVolume(volume);
        }
        protected override void OnPreviewMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnPreviewMouseLeftButtonDown(e);
            if (e.OriginalSource is DependencyObject dependencyObject)
            {
                var parent = System.Windows.Media.VisualTreeHelper.GetParent(dependencyObject);
                while (parent != null)
                {
                    if (parent == TrackPositionSlider)
                    {
                        _isDraggingSlider = true;
                        break;
                    }
                    parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
                }
            }
        }
        private void AudioPlayerService_PlaybackEnded(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                PlayNextTrackAutomaticaly();
            });
        }
        private void PlayNextTrackAutomaticaly()
        {
            try
            {
                if (_tracks.Count == 0 || _audioPlayerService.CurrentTrack == null)
                {
                    _timer.Stop();
                    return;
                }
                int currentIndex = _tracks.IndexOf(_audioPlayerService.CurrentTrack);
                if (currentIndex == -1)
                {
                    _timer.Stop();
                    return;
                }
                if (currentIndex >=  _tracks.Count - 1)
                {
                    _timer.Stop();
                    CurrentTimeTextBlock.Text = "00:00";
                    TrackPositionSlider.Value = 0;
                    CurrentTrackTextBlock.Text = "Воспроизведение завершено";
                    return;
                }
                var nextTrack = _tracks[currentIndex + 1];
                TracksListBox.SelectedItem = nextTrack;
                TracksListBox.ScrollIntoView(nextTrack);
                PlaySelectedTrack(nextTrack);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка автоперехода к следующему треку: {ex.Message}");
            }
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ApplyTrackFilter();
        }
        private void ApplyTrackFilter()
        {
            try
            {
                string searchText = SearchTextBox.Text.Trim();
                if (string.IsNullOrWhiteSpace(searchText))
                {
                    TracksListBox.ItemsSource = _tracks;
                    return;
                }
                var filteredTracks = _tracks
                    .Where(t =>
                    (!string.IsNullOrWhiteSpace(t.Title) &&
                    t.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)) ||
                    (!string.IsNullOrWhiteSpace(t.Artist) &&
                    t.Artist.Contains(searchText, StringComparison.OrdinalIgnoreCase)))
                    .ToList();
                TracksListBox.ItemsSource = filteredTracks;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Ошибка поиска: {ex.Message}");
            }
        }
    }
}