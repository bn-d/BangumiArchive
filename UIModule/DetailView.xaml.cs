using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The detailed Series info view
    /// </summary>
    public sealed partial class DetailView : Page
    {
        private ObservableCollection<SeriesIndex> IndexList => DataManager.SeriesIndices;
        private SeriesIndex Index => IndexList[SeriesDetail.SelectedIndex];
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
            StackPanel parent = (StackPanel)VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent((AppBarButton)sender));
            ListView songList = (ListView)VisualTreeHelper.GetChild(parent, 0);
            ObservableCollection<Song> songs = (ObservableCollection<Song>)songList.ItemsSource;
            songs.Add(new Song());
        }

        /// <summary>
        /// Remove the last song, if the list is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSongClick(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel)VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent((AppBarButton)sender));
            ListView songList = (ListView)VisualTreeHelper.GetChild(parent, 0);
            ObservableCollection<Song> songs = (ObservableCollection<Song>)songList.ItemsSource;
            if (songs.Any())
                songs.RemoveAt(songs.Count - 1);
        }

        /// <summary>
        /// Change the review rank of the current Series
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRankClick(object sender, RoutedEventArgs e)
        {
            Series.Review = (Review)(5 - int.Parse(((MenuFlyoutItem)sender).Text));
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
                FileTypeFilter = { ".jpg" , ".png" }
            };

            StorageFile file = await openPicker.PickSingleFileAsync();

            if (file == null) return;

            IRandomAccessStream stream = await file.OpenAsync(FileAccessMode.Read);
            DataReader reader = new DataReader(stream.GetInputStreamAt(0));
            await reader.LoadAsync((uint) stream.Size);
            Series.FlagByte = new byte[stream.Size];
            reader.ReadBytes(Series.FlagByte);

            Series.OnPropertyChanged("Flag");
        }

        private void BackClick(object sender, RoutedEventArgs e) => 
            MainPage.NavigateMainView();

        private void SaveClick(object sender, RoutedEventArgs e) =>
            DataManager.SaveData();

        private void OnlyDigitKeyDown(object sender, KeyRoutedEventArgs e) =>
            UIDictionary.OnlyDigitKeyDown(sender, e);

        private void TrimText(object sender, RoutedEventArgs e) =>
            UIDictionary.TrimTextHelper(sender, e);

        private void TrimTextSuggest(object sender, RoutedEventArgs e) =>
            UIDictionary.TrimTextSuggest(sender, e);

        private void CompanyTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e) =>
            UIDictionary.CompanyTextChanged(sender, e);

        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e) =>
            UIDictionary.SearchTextChanged(sender, e);

        private void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e) =>
            UIDictionary.SearchQuerySubmitted(sender, e);
    }
}
