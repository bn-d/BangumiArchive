using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using ListView = Windows.UI.Xaml.Controls.ListView;
using TextBox = Windows.UI.Xaml.Controls.TextBox;

namespace BangumiArchive.UIModule
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
        private void TrimText(object sender, RoutedEventArgs e) =>
            TrimTextHelper(sender, e);

        public static void TrimTextHelper(object sender, RoutedEventArgs e)
        {
            TextBox TB = (TextBox)sender;
            TB.Text = TB.Text.Trim();
        }

        /// <summary>
        /// Trim text for auto suggests box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TrimTextSuggest(object sender, RoutedEventArgs e) =>
            TrimTextSuggestHelper(sender, e);

        public static void TrimTextSuggestHelper(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox TB = (AutoSuggestBox)sender;
            TB.Text = TB.Text.Trim();
        }

        /// <summary>
        /// Allow only digit
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnlyDigitKeyDown(object sender, KeyRoutedEventArgs e)
        {
            if ((e.Key < VirtualKey.NumberPad0 || e.Key > VirtualKey.NumberPad9) & 
                (e.Key < VirtualKey.Number0 || e.Key > VirtualKey.Number9))
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Give suggestion to company input
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CompanyTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args) => 
            CompanyTextChangedHelper(sender, args);

        public static void CompanyTextChangedHelper(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            // Find suggestions for search
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Any())
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
        /// Remove the last song, if the list is not empty
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
                songs.RemoveAt(songs.Count - 1);
        }

        /// <summary>
        /// Add a new item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddListItem(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel)VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent((AppBarButton)sender));
            GridView nameList = (GridView)VisualTreeHelper.GetChild(parent, 1);
            ObservableCollection<StringWrap> names = (ObservableCollection<StringWrap>)nameList.ItemsSource;
            names.Add(new StringWrap(names.Count + 1));
        }

        /// <summary>
        /// Remove the last item, if the list is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveListItem(object sender, RoutedEventArgs e)
        {
            StackPanel parent = (StackPanel)VisualTreeHelper.GetParent(
                VisualTreeHelper.GetParent((AppBarButton)sender));
            GridView nameList = (GridView)VisualTreeHelper.GetChild(parent, 1);
            ObservableCollection<StringWrap> names = (ObservableCollection<StringWrap>)nameList.ItemsSource;
            if (names.Any())
                names.RemoveAt(names.Count - 1);
        }

        /// <summary>
        /// Convert bool? to bool
        /// </summary>
        /// <param name="b"></param>
        /// <returns></returns>
        public static bool NullBToBool(bool? b)
        {
            return b != null && (bool)b;
        }

        /// <summary>
        /// Reassign the index number after reorder
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void OtherListReorderCompleted(ListViewBase sender, DragItemsCompletedEventArgs args)
        {
            ObservableCollection<StringWrap> l = (ObservableCollection<StringWrap>) sender.ItemsSource;
            for (int i = 0; i < l.Count(); i++)
            {
                l[i].Index = i + 1;
            }
            sender.ItemsSource = null;
            sender.ItemsSource = l;
        }
    }
}
