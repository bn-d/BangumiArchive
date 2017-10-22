using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Popups;
using Windows.UI.Xaml.Navigation;

namespace AnimeArchive.UIModule
{
    /// <summary>
    /// The main anime grid view of the app
    /// </summary>
    public sealed partial class AnimeGridView : Page
    {
        private ObservableCollection<Anime> _filteredAnime;
        private bool _isFiltered;

        public AnimeGridView()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Refresh binding
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            AnimeGrid.ItemsSource = null;
            if (!_isFiltered)
                AnimeGrid.ItemsSource = Global.Animes;
            else
                AnimeGrid.ItemsSource = _filteredAnime;
        }

        /// <summary>
        /// Display detailed anime info in another frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimeItemClick(object sender, ItemClickEventArgs e)
        {
            ShowAnimeDetail((Anime) e.ClickedItem);
        }

        /// <summary>
        /// Display detailed anime info in another frame
        /// </summary>
        /// <param name="anime"></param>
        private void ShowAnimeDetail(Anime anime)
        {
            Frame.Navigate(typeof(AnimeInfoView), anime.Index - 1);
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
            Global.Animes.Add(new Anime(indx + 1, name));

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
                var suggestions = new List<Anime>();

                foreach (Anime a in Global.Animes)
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
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is Anime)
            {
                ShowAnimeDetail((Anime)args.ChosenSuggestion);
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
        private void FilterAnime(object sender, RoutedEventArgs e)
        {
            _filteredAnime = new ObservableCollection<Anime>();

            foreach (Anime a in Global.Animes)
            {
                if (FilterAnimeHelper(a))
                    _filteredAnime.Add(a);
            }
            _isFiltered = true;
            AnimeGrid.ItemsSource = _filteredAnime;
        }

        private bool FilterAnimeHelper(Anime a)
        {
            if (UIDictionary.NullBToBool(WatchingCB.IsChecked) && 
                !UIDictionary.NullBToBool(a.IsWatching))
                return false;

            bool neverWatch = false;
            if (!UIDictionary.NullBToBool(NWatchedCB.IsChecked))
                neverWatch = true;
            else
            {
                foreach (Season s in a.Seasons)
                {
                    if (s.Time == 0)
                    {
                        neverWatch = true;
                        break;
                    }
                }
            }
            if (!neverWatch)
                return false;

            if (!((a.Rank == 0 && UIDictionary.NullBToBool(Rank5CB.IsChecked)) ||
                  (a.Rank == 1 && UIDictionary.NullBToBool(Rank4CB.IsChecked)) ||
                  (a.Rank == 2 && UIDictionary.NullBToBool(Rank3CB.IsChecked)) ||
                  (a.Rank == 3 && UIDictionary.NullBToBool(Rank2CB.IsChecked)) ||
                  (a.Rank == 4 && UIDictionary.NullBToBool(Rank1CB.IsChecked)) ||
                  (a.Rank == 5 && UIDictionary.NullBToBool(NoRankCB.IsChecked))))
                return false;

            bool company = false;
            if (CompanyTB.Text == "")
                company = true;
            else
            {
                foreach (Season s in a.Seasons)
                {
                    if (s.Company == CompanyTB.Text)
                    {
                        company = true;
                        break;
                    }
                }
            }
            if (!company)
                return false;

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
                Rank5CB.IsChecked = true;
                Rank1CB.IsChecked = true;
                Rank2CB.IsChecked = true;
                Rank3CB.IsChecked = true;
                Rank4CB.IsChecked = true;
                Rank5CB.IsChecked = true;
                NoRankCB.IsChecked = true;
            }
            else
            {
                Rank1CB.IsChecked = false;
                Rank2CB.IsChecked = false;
                Rank3CB.IsChecked = false;
                Rank4CB.IsChecked = false;
                Rank5CB.IsChecked = false;
                NoRankCB.IsChecked = false;
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
        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            if (_isFiltered)
            {
                AnimeGrid.ItemsSource = Global.Animes;
                _isFiltered = false;
            }
        }

        private void TrimTextSuggest(object sender, RoutedEventArgs e) =>
            UIDictionary.TrimTextSuggestHelper(sender, e);

        private void SaveData(object sender, RoutedEventArgs e) =>
            DataManager.SaveData();

        private void ExportAnime(object sender, RoutedEventArgs e) =>
            DataManager.ExportAnime();

        private void ImportAnime(object sender, RoutedEventArgs e) =>
            DataManager.ImportAnime();

        private void CompanyTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) =>
            UIDictionary.CompanyTextChangedHelper(sender, args);
    }
}
