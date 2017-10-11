using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;


namespace AnimeArchive.UIModule
{
    /// <summary>
    /// The main anime grid view of the app
    /// </summary>
    public sealed partial class AnimeGridView : Page
    {
        private ObservableCollection<Anime> Animes => Global.Animes;

        public AnimeGridView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Display detailed anime info in another frame
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AnimeListView_ItemClick(object sender, ItemClickEventArgs e)
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

            int indx = Animes.Count();

            // Create new anime
            string name = (string) dialog.Text;
            Animes.Add(new Anime(indx + 1, name));

            // Scroll to the new anime
            AnimeGrid.ScrollIntoView(Animes[indx]);

            // Display the anime details
            ShowAnimeDetail(Animes[indx]);
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
        /// Scroll to the top of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollToTop(object sender, RoutedEventArgs e)
        {
            if (Animes.Any())
                AnimeGrid.ScrollIntoView(Animes[0]);
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
        /// Give search result for search box 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Any())
            {
                var suggestions = new List<Anime>();

                foreach (Anime a in Animes)
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
                int indx = Math.Max(Math.Min(int.Parse(dialog.Text), Animes.Count() - 1), 0);
                AnimeGrid.ScrollIntoView(Animes[indx]);
            }
        }
    }
}
