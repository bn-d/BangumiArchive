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
        private Anime curAnime;

        public AnimeInfoView()
        {
            this.InitializeComponent();
            this.DataContextChanged += (s, e) => this.Bindings.Update();
        }

        /// <summary>
        /// Receive the current anime index from previous frame
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            int indx = (int) e.Parameter;
            curAnime = Global.Animes[indx];
            RankBar.Fill = Anime.GetRankColorBrush(curAnime.Rank);
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
            curAnime.Seasons.Add(new Season());
        }

        /// <summary>
        /// Remove the last season, if the season list is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSeason(object sender, RoutedEventArgs e)
        {
            if (curAnime.Seasons.Any())
                curAnime.Seasons.RemoveAt(curAnime.Seasons.Count() - 1);
        }

        /// <summary>
        /// Save the entire anime list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SaveAnime(object sender, RoutedEventArgs e)
        {
            AnimeManager.WriteAnime();
        }

        /// <summary>
        /// Export anime to user selected location
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ExportAnime(object sender, RoutedEventArgs e)
        {
            AnimeManager.ExportAnime();
        }

        /// <summary>
        /// Import anime from user selected file
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ImportAnime(object sender, RoutedEventArgs e)
        {
            AnimeManager.ImportAnime();
        }

        /// <summary>
        /// Change the review rank of the current anime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeRank(object sender, RoutedEventArgs e)
        {
            curAnime.Rank = int.Parse(((MenuFlyoutItem)sender).Text);
            RankBar.Fill = Anime.GetRankColorBrush(curAnime.Rank);
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
            curAnime.FlagByte = new byte[stream.Size];
            reader.ReadBytes(curAnime.FlagByte);

            Bindings.Update();
        }

        /// <summary>
        /// Trim text for text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrimText(object sender, RoutedEventArgs e)
        {
            TextBox TB = (TextBox)sender;
            TB.Text = TB.Text.Trim();
        }

        private void PreviousAnime(object sender, RoutedEventArgs e)
        {
            if (curAnime.Index > 1)
                Frame.Navigate(typeof(AnimeInfoView), curAnime.Index - 2);
        }

        private void NextAnime(object sender, RoutedEventArgs e)
        {
            if (curAnime.Index < Global.Animes.Count())
                Frame.Navigate(typeof(AnimeInfoView), curAnime.Index);
        }
    }
}
