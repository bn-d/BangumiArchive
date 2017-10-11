using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace AnimeArchive.UIModule
{
    public partial class UIDictionary
    {
        public UIDictionary()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Trim text for text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrimText(object sender, RoutedEventArgs e)
        {
            TextBox TB = (TextBox) sender;
            TB.Text = TB.Text.Trim();
        }

        /// <summary>
        /// Trim text for auto suggests box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrimTextSuggest(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox TB = (AutoSuggestBox)sender;
            TB.Text = TB.Text.Trim();
        }

        /// <summary>
        /// Give suggestion to company input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CompanyTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Find suggestions for search
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Count() > 1)
            {
                var suggestions = new List<string>();

                foreach (string s in Global.CompanyHashSet)
                {
                    if (s.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                        suggestions.Add(s);
                }
                if (suggestions.Count > 0)
                    sender.ItemsSource = suggestions.OrderByDescending(i => i.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase));
                else
                    sender.ItemsSource = new string[] { "No results found" };
            }
        }

        /// <summary>
        /// Add a new song
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddSong(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel) VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent((AppBarButton) sender));
            ListView songList = (ListView) VisualTreeHelper.GetChild(parent, 0);
            ObservableCollection<Song> songs = (ObservableCollection<Song>) songList.ItemsSource;
            songs.Add(new Song());
        }

        /// <summary>
        /// Remove the last song, if the season list is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveSong(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel)VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent((AppBarButton)sender));
            ListView songList = (ListView)VisualTreeHelper.GetChild(parent, 0);
            ObservableCollection<Song> songs = (ObservableCollection<Song>)songList.ItemsSource;
            if (songs.Any())
                songs.RemoveAt(songs.Count() - 1);
        }
    }
}
