using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
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
        /// Give search result for search box 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Any())
            {
                var suggestions = new List<SeriesIndex>();

                foreach (SeriesIndex i in DataManager.SIs)
                {
                    if (i.Series.Title.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0
                        || i.Series.SubTitle.IndexOf(sender.Text, StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        suggestions.Add(i);
                    }
                }
                if (suggestions.Count > 0)
                    sender.ItemsSource = suggestions.OrderByDescending(i => i.Series.Title.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase) ||
                                                                            i.Series.SubTitle.StartsWith(sender.Text, StringComparison.CurrentCultureIgnoreCase)).ThenBy(i => i.Series.Title);
                else
                    sender.ItemsSource = new string[] { "No results found" };
            }
        }

        /// <summary>
        /// Handle the search result
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public static void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (args.ChosenSuggestion != null && args.ChosenSuggestion is SeriesIndex index)
            {
                MainPage.NavigateDetailView(index);
            }
        }

        /// <summary>
        /// Trim text for text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void TrimText(object sender, RoutedEventArgs e)
        {
            TextBox TB = (TextBox)sender;
            TB.Text = TB.Text.Trim();
        }

        /// <summary>
        /// Trim text for auto suggests box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void TrimTextSuggest(object sender, RoutedEventArgs e)
        {
            AutoSuggestBox TB = (AutoSuggestBox)sender;
            TB.Text = TB.Text.Trim();
        }

        /// <summary>
        /// Allow only digit in text box
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnlyDigitKeyDown(object sender, KeyRoutedEventArgs e)
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
        /// <param name="e"></param>
        public static void CompanyTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e)
        {
            // Find suggestions for search
            if (e.Reason == AutoSuggestionBoxTextChangeReason.UserInput && sender.Text.Any())
            {
                var suggestions = new List<string>();

                foreach (string s in DataManager.CompanyHashSet)
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

    /// <summary>
    /// Convert review rank to rank color
    /// </summary>
    public class RankColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return (Review)value switch
            {
                Review.Rank5 => new SolidColorBrush(Colors.White),
                Review.Rank4 => new SolidColorBrush(Color.FromArgb(255, 91, 235, 226)),
                Review.Rank3 => new SolidColorBrush(Color.FromArgb(255, 224, 231, 16)),
                Review.Rank2 => new SolidColorBrush(Color.FromArgb(255, 191, 191, 191)),
                Review.Rank1 => new SolidColorBrush(Color.FromArgb(255, 240, 133, 61)),
                _ => new SolidColorBrush(Colors.Black),
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// A simple pair
    /// </summary>
    public class Pair<FirstT, SecondT> : INotifyPropertyChanged
    {
        public FirstT First;
        private SecondT second;
        public SecondT Second
        {
            get { return second; }
            set {
                second = value;
                OnPropertyChanged("Second");
            }
        }

        public Pair(FirstT f, SecondT s)
        {
            First = f;
            Second = s;
        }

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
