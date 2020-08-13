using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The main anime grid view of the app
    /// </summary>
    public sealed partial class AnimeGridView : Page
    {

        public static GridView StaticAnimeGrid;

        public AnimeGridView()
        {
            InitializeComponent();

            StaticAnimeGrid = AnimeGrid;
        }

        /// <summary>
        /// Refresh binding
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AnimeGrid.ItemsSource = null;
            if (!Global.IsFiltered)
                AnimeGrid.ItemsSource = Global.Animes;
            else
                AnimeGrid.ItemsSource = Global.FilteredAnimes;
        }

        /// <summary>
        /// Upfate the anime grid
        /// </summary>
        public static void UpdateGrid()
        {
            if (!Global.IsFiltered)
                StaticAnimeGrid.ItemsSource = Global.Animes;
            else
                StaticAnimeGrid.ItemsSource = Global.FilteredAnimes;
        }

        /// <summary>
        /// Display detailed anime info in another frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimeItemClick(object sender, ItemClickEventArgs e)
        {
            ShowAnimeDetail((Series) e.ClickedItem);
        }

        /// <summary>
        /// Display detailed anime info in another frame
        /// </summary>
        /// <param name="series"></param>
        public void ShowAnimeDetail(Series series)
        {
            Frame.Navigate(typeof(AnimeInfoView), series.Index - 1);
        }

        public void ShowAnimeDetail(int index)
        {
            Frame.Navigate(typeof(AnimeInfoView), index);
        }

        /// <summary>
        /// Add new anime
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddAnimeAsync(object sender, RoutedEventArgs e)
        {
            // Show input dialog to get the new anime name
            var dialog = new InputDialog();
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            int indx = Global.Animes.Count;

            // Create new anime
            string name = (string) dialog.Text;
            Global.Animes.Add(new Series(indx + 1, name));

            // Scroll to the new anime
            AnimeGrid.ScrollIntoView(Global.Animes[indx]);

            // Display the anime details
            ShowAnimeDetail(Global.Animes[indx]);
        }

        /// <summary>
        /// Scroll to the top of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollToTop(object sender, RoutedEventArgs e)
        {
            if (Global.Animes.Any())
                AnimeGrid.ScrollIntoView(Global.Animes[0]);
        }

        /// <summary>
        /// Give search result for search box 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Any())
            {
                var suggestions = new List<Series>();

                foreach (Series a in Global.Animes)
                {
                    if (a.Title.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0
                        || a.SubTitle.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        suggestions.Add(a);
                    }
                }
                if (suggestions.Count > 0)
                    sender.ItemsSource = suggestions.OrderByDescending(i => i.Title.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase) ||
                                                                            i.SubTitle.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Title);
                else
                    sender.ItemsSource = new string[] { "No results found" };
            }
        }

        /// <summary>
        /// Handle the search result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is Series series)
            {
                ShowAnimeDetail(series);
            }
        }

        /// <summary>
        /// Scroll to the anime with given index
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ScrollTo(object sender, RoutedEventArgs e)
        {
            // Show input dialog to get the new anime name
            var dialog = new InputDialog
            {
                Title = "Scroll to",
                Inbox = {PlaceholderText = "Anime Index"},
                PrimaryButtonText = "Scroll"
            };
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                int indx = Math.Max(Math.Min(int.Parse(dialog.Text), Global.Animes.Count - 1), 0);
                AnimeGrid.ScrollIntoView(Global.Animes[indx]);
            }
        }

        /// <summary>
        /// Open/Colse the filter split view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeFilterSplitView(object sender, RoutedEventArgs e)
        {
            FilterSplitView.IsPaneOpen = !FilterSplitView.IsPaneOpen;
        }

        /// <summary>
        /// Filter the anime according to the user's specification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterAnimeClick(object sender, RoutedEventArgs e) => FilterAnime();

        /// <summary>
        /// Filter the anime according to the user's specification
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FilterAnime()
        {
            Global.FilteredAnimes = new ObservableCollection<Series>();

            foreach (Series a in Global.Animes)
            {
                if (FilterAnimeHelper(a))
                {
                    Global.FilteredAnimes.Add(a);
                }
            }
            Global.IsFiltered = true;
            AnimeGrid.ItemsSource = Global.FilteredAnimes;
        }

        private bool FilterAnimeHelper(Series a)
        {
            // Company
            if (CompanyTB.Text != "")
            {
                bool company = false;
                foreach (Season s in a.Seasons)
                {
                    if (s.Company == CompanyTB.Text)
                    {
                        company = true;
                        break;
                    }
                }
                if (!company)
                    return false;
            }

            // Rank
            if (!((a.Rank == 0 && UIDictionary.NullBToBool(Rank5CB.IsChecked)) ||
                  (a.Rank == 1 && UIDictionary.NullBToBool(Rank4CB.IsChecked)) ||
                  (a.Rank == 2 && UIDictionary.NullBToBool(Rank3CB.IsChecked)) ||
                  (a.Rank == 3 && UIDictionary.NullBToBool(Rank2CB.IsChecked)) ||
                  (a.Rank == 4 && UIDictionary.NullBToBool(Rank1CB.IsChecked)) ||
                  (a.Rank == 5 && UIDictionary.NullBToBool(NoRankCB.IsChecked))))
                return false;

            // Is Watching
            if (UIDictionary.NullBToBool(WatchingCB.IsChecked) && 
            !UIDictionary.NullBToBool(a.IsWatchingNullable))
            return false;

            // Never watched
            if (UIDictionary.NullBToBool(NWatchedCB.IsChecked))
            {
                bool neverWatch = false;
                foreach (Season s in a.Seasons)
                {
                    if (s.Time == 0)
                    {
                        neverWatch = true;
                        break;
                    }
                }
                if (!neverWatch)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Actions when rank all checkbox is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RankAllClick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (UIDictionary.NullBToBool(cb.IsChecked))
            {
                Rank5CB.IsChecked = Rank1CB.IsChecked = Rank2CB.IsChecked = Rank3CB.IsChecked =
                    Rank4CB.IsChecked = Rank5CB.IsChecked = NoRankCB.IsChecked = true;
            }
            else
            {
                Rank5CB.IsChecked = Rank1CB.IsChecked = Rank2CB.IsChecked = Rank3CB.IsChecked =
                    Rank4CB.IsChecked = Rank5CB.IsChecked = NoRankCB.IsChecked = false;
            }
        }

        /// <summary>
        /// Actions when rank checkbox is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RankClick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox) sender;
            if (!UIDictionary.NullBToBool(cb.IsChecked))
                RankAllCB.IsChecked = false;
            else
            {
                if (UIDictionary.NullBToBool(Rank1CB.IsChecked) &&
                    UIDictionary.NullBToBool(Rank2CB.IsChecked) &&
                    UIDictionary.NullBToBool(Rank3CB.IsChecked) &&
                    UIDictionary.NullBToBool(Rank4CB.IsChecked) &&
                    UIDictionary.NullBToBool(Rank5CB.IsChecked) &&
                    UIDictionary.NullBToBool(NoRankCB.IsChecked))
                    RankAllCB.IsChecked = true;
            }
        }

        /// <summary>
        /// Clear the filter
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearFilterClick(object sender, RoutedEventArgs e)
        {
            WatchingCB.IsChecked = NWatchedCB.IsChecked = false;
            RankAllCB.IsChecked = true;
            Rank5CB.IsChecked = Rank1CB.IsChecked = Rank2CB.IsChecked = Rank3CB.IsChecked =
                Rank4CB.IsChecked = Rank5CB.IsChecked = NoRankCB.IsChecked = true;

            CompanyTB.Text = "";

            if (Global.IsFiltered)
            {
                AnimeGrid.ItemsSource = Global.Animes;
                Global.IsFiltered = false;
            }
        }

        /// <summary>
        /// Import from file, if successfully import, refresh anime grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportAnime(object sender, RoutedEventArgs e)
        {
            bool imported = await DataManager.ImportAnime();
            if (imported)
            {
                AnimeGrid.ItemsSource = null;
                AnimeGrid.ItemsSource = Global.Animes;
            }
        }
        
        private void ExportAnime(object sender, RoutedEventArgs e) =>
            DataManager.ExportAnime();

        private void TrimTextSuggest(object sender, RoutedEventArgs e) =>
            UIDictionary.TrimTextSuggestHelper(sender, e);

        private void SaveData(object sender, RoutedEventArgs e) =>
            DataManager.SaveData();

        private void CompanyTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) =>
            UIDictionary.CompanyTextChangedHelper(sender, args);
    }
}
