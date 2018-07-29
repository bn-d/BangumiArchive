using System;
using System.Linq;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;


namespace AnimeArchive.UIModule
{
    /// <summary>
    /// The view of the detailed information of an anime
    /// </summary>
    public sealed partial class AnimeInfoView : Page
    {
        // The object of current anime
        private Anime _curAnime;

        public AnimeInfoView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }

        /// <summary>
        /// Receive the current anime index from previous frame
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int indx = (int) e.Parameter;
            _curAnime = Global.Animes[indx];
            RankBar.Fill = Anime.GetRankColorBrush(_curAnime.Rank);
        }

        /// <summary>
        /// Go back to main page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void GoBackToMain(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AnimeGridView));
        }

        /// <summary>
        /// Add a new season
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSeason(object sender, RoutedEventArgs e)
        {
            _curAnime.Seasons.Add(new Season());
        }

        /// <summary>
        /// Remove the last season, if the season list is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSeason(object sender, RoutedEventArgs e)
        {
            if (_curAnime.Seasons.Any())
                _curAnime.Seasons.RemoveAt(_curAnime.Seasons.Count - 1);
        }

        /// <summary>
        /// Change the review rank of the current anime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRank(object sender, RoutedEventArgs e)
        {
            _curAnime.Rank = 5 - int.Parse(((MenuFlyoutItem)sender).Text);
            RankBar.Fill = Anime.GetRankColorBrush(_curAnime.Rank);
        }

        /// <summary>
        /// Change the flag of the current anime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ChangeFlag(object sender, RoutedEventArgs e)
        {
            // Load the flag of the anime through file picker
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
            _curAnime.FlagByte = new byte[stream.Size];
            reader.ReadBytes(_curAnime.FlagByte);

            Bindings.Update();
        }

        /// <summary>
        /// Navigate to previous anime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void PreviousAnime(object sender, RoutedEventArgs e)
        {
            if (_curAnime.Index > 1)
                Frame.Navigate(typeof(AnimeInfoView), _curAnime.Index - 2);
        }

        /// <summary>
        /// Navigate to next anime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void NextAnimeAsync(object sender, RoutedEventArgs e)
        {
            if (_curAnime.Index < Global.Animes.Count)
                Frame.Navigate(typeof(AnimeInfoView), _curAnime.Index);
            else
            {
                // Show input dialog to get the new anime name
                var dialog = new InputDialog();
                var result = await dialog.ShowAsync();

                if (result != ContentDialogResult.Primary) return;

                int indx = Global.Animes.Count;

                // Create new anime
                string name = (string)dialog.Text;
                Global.Animes.Add(new Anime(indx + 1, name));

                Frame.Navigate(typeof(AnimeInfoView), _curAnime.Index);
            }
        }

        private void SaveAnime(object sender, RoutedEventArgs e) =>
            DataManager.SaveData();

        private void ExportAnime(object sender, RoutedEventArgs e) =>
            DataManager.ExportAnime();

        private void ImportAnime(object sender, RoutedEventArgs e) =>
            DataManager.ImportAnime();

        private void TrimText(object sender, RoutedEventArgs e) =>
            UIDictionary.TrimTextHelper(sender, e);
    }
}
