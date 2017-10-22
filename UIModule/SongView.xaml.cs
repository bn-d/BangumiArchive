using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AnimeArchive.UIModule
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SongView : Page
    {
        private ObservableCollection<string> songList => StatisticsView.SongList;

        public SongView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Go back to statistics 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoBackToStatistics(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(StatisticsView));
        }
    }
}
