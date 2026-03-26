using MPIntegration.App.ViewModels;
using System.Windows;
using System.Windows.Input;

namespace MPIntegration.App
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
            TrackPositionSlider.PreviewMouseLeftButtonDown += TrackPositionSlider_PreviewMouseLeftButtonDown;
            TrackPositionSlider.PreviewMouseLeftButtonUp += TrackPositionSlider_PreviewMouseLeftButtonUp;
        }
        private void TracksListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.TrackDoubleClickCommand.CanExecute(null))
            {
                ViewModel.TrackDoubleClickCommand.Execute(null);
            }
        }
        private void TrackPositionSlider_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ViewModel.BeginSliderDrag();
        }
        private void TrackPositionSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            ViewModel.EndSliderDrag();
        }
    }
}