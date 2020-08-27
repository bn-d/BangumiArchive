using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The detailed Series info view
    /// </summary>
    public sealed partial class DetailView : Page
    {
        private bool Watched;
        private ObservableCollection<SeriesIndex> Indices =>
            Watched ? DataManager.WatchedIdx : DataManager.ToWatchIdx;
        private SeriesIndex Index => Indices[SeriesDetail.SelectedIndex];
        private Series Series => Index.Series;

        public DetailView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }

        /// <summary>
        /// Receive the current SeriesIndex from previous frame
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            Watched = ((SeriesIndex)e.Parameter).Watched;
            if (!Watched) { SeriesSearchBox.Visibility = Visibility.Collapsed; }

            SeriesDetail.SelectedIndex = ((SeriesIndex)e.Parameter).Index;
        }

        /// <summary>
        /// Add a new season
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSeasonClick(object sender, RoutedEventArgs e)
        {
            Series.Seasons.Add(new Season());
        }

        /// <summary>
        /// Remove the last season, if the season list is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSeasonClick(object sender, RoutedEventArgs e)
        {
            if (Series.Seasons.Any())
                Series.Seasons.RemoveAt(Series.Seasons.Count - 1);
        }

        /// <summary>
        /// Add a new song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSongClick(object sender, RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;
            Season s = (Season)button.DataContext;
            s.Songs.Add(new Song());
        }

        /// <summary>
        /// Delete the current song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteSongClick(object sender, RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;
            Song song = (Song)button.DataContext;

            ItemsStackPanel parent = (ItemsStackPanel)VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent(VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent((AppBarButton)sender))));
            Season s = (Season)parent.DataContext;

            int index = s.Songs.ToList().FindIndex(cur => cur == song);
            s.Songs.RemoveAt(index);
        }

        /// <summary>
        /// Change the review rank of the current Series
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRankItemClick(object sender, ItemClickEventArgs e)
        {
            Series.Review = (Review)e.ClickedItem;
        }

        /// <summary>
        /// Change the flag of the current Series
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeFlagClick(object sender, RoutedEventArgs e)
        {
            // Load the flag of the Series through file picker
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.PicturesLibrary,
                FileTypeFilter = { ".jpg", ".png" }
            };

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file == null) return;

            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            DataReader reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint)stream.Size);
            Series.FlagByte = new byte[stream.Size];
            reader.ReadBytes(Series.FlagByte);

            Series.OnPropertyChanged("Flag");
        }

        /// <summary>
        /// Back to previous page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackClick(object sender, RoutedEventArgs e)
        {
            if (Watched) { MainPage.NavigateMainView(); }
            else { MainPage.NavigateToWatchView(); }
        }
    }
}
