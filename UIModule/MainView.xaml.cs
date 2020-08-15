using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Windows.UI.Xaml.Navigation;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The main Series grid view
    /// </summary>
    public sealed partial class MainView : Page
    {
        public static bool IsFiltered;
        public static string CompanyFilter = "";
        public static Review? ReviewFilter;
        public static ObservableCollection<SeriesIndex> StaticIndexList;

        public ObservableCollection<SeriesIndex> IndexList => StaticIndexList;
        public List<CheckBox> ReviewCBs;

        public MainView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();

            ReviewCBs = new List<CheckBox>();
            ReviewCBs.Add(Rank5CB);
            ReviewCBs.Add(Rank4CB);
            ReviewCBs.Add(Rank3CB);
            ReviewCBs.Add(Rank2CB);
            ReviewCBs.Add(Rank1CB);
            ReviewCBs.Add(NoRankCB);

            IsFiltered = false;

            ResetIndexList();
        }

        /// <summary>
        /// Refresh binding on navigated to
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (CompanyFilter.Length > 0 || ReviewFilter != null)
            {
                IsFiltered = true;
                ResetFilter();

                if (CompanyFilter.Length > 0)
                {
                    CompanyTB.Text = CompanyFilter;
                    CompanyFilter = "";
                }

                if (ReviewFilter != null)
                {
                    ReviewCBs.ForEach(cur => cur.IsChecked = false);
                    ReviewCBs[(int)(Review)ReviewFilter].IsChecked = true;
                    ReviewFilter = null;
                }
                FilterSeries();
            }
            //Bindings.Update();
        }

        /// <summary>
        /// Reset the index list to default
        /// </summary>
        private void ResetIndexList()
        {
            StaticIndexList = new ObservableCollection<SeriesIndex>(DataManager.SIs);
            Bindings.Update();
        }

        /// <summary>
        /// Reset the filter UI to default
        /// </summary>
        private void ResetFilter()
        {
            IsFiltered = false;

            WatchingCB.IsChecked = NWatchedCB.IsChecked = false;
            RankAllCB.IsChecked = true;
            ReviewCBs.ForEach(cur => cur.IsChecked = true);
            CompanyTB.Text = "";

            ResetIndexList();
        }

        /// <summary>
        /// Filter Series according to UI setting
        /// </summary>
        private void FilterSeries()
        {
            IsFiltered = true;

            StaticIndexList = new ObservableCollection<SeriesIndex>(
                DataManager.SIs.Where(i => CheckSeries(i.Series)));
            Bindings.Update();
        }

        /// <summary>
        /// Check whether a single Series should be filtered
        /// </summary>
        /// <param name="series"></param>
        /// <returns></returns>
        private bool CheckSeries(Series series)
        {
            // Company
            if (CompanyTB.Text != "" &&
                !series.Seasons.Any(cur => cur.Company == CompanyTB.Text))
                return false;

            // Rank
            if (!(bool)ReviewCBs[(int)series.Review].IsChecked)
                return false;

            // Is Watching
            if ((bool)WatchingCB.IsChecked &&
                !(bool)series.IsWatchingNullable)
                return false;

            // Never watched
            if ((bool)NWatchedCB.IsChecked &&
                series.Seasons.Any(cur => cur.Time == 0))
                return false;

            return true;
        }

        /// <summary>
        /// Add new series
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddSeriesAsyncClick(object sender, RoutedEventArgs e)
        {
            // Show input dialog to get the new Series name
            var dialog = new InputDialog();
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            // Create new Series
            string name = dialog.Text;
            SeriesIndex si = DataManager.AddSeries(name);
            if (!IsFiltered || CheckSeries(si.Series))
            {
                IndexList.Add(si);
            }

            // Scroll to the new Series
            SeriesGrid.ScrollIntoView(si);

            // Display the Series' details
            MainPage.NavigateDetailView(si);
        }

        /// <summary>
        /// Scroll to the top of the page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ScrollToTopClick(object sender, RoutedEventArgs e)
        {
            if (IndexList.Any())
                SeriesGrid.ScrollIntoView(IndexList[0]);
        }

        /// <summary>
        /// Toggle the filter split view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ChangeFilterSplitViewClick(object sender, RoutedEventArgs e)
        {
            FilterSplitView.IsPaneOpen = !FilterSplitView.IsPaneOpen;
        }

        /// <summary>
        /// Actions when rank all checkbox is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RankAllClick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if ((bool)cb.IsChecked) { ReviewCBs.ForEach(cur => cur.IsChecked = true); }
            else { ReviewCBs.ForEach(cur => cur.IsChecked = false); }
        }

        /// <summary>
        /// Actions when rank checkbox is clicked
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RankClick(object sender, RoutedEventArgs e)
        {
            CheckBox cb = (CheckBox)sender;
            if (!(bool)cb.IsChecked)
            {
                RankAllCB.IsChecked = false;
                return;
            }

            if (ReviewCBs.All(cur => (bool)cur.IsChecked))
                RankAllCB.IsChecked = true;
        }

        /// <summary>
        /// Import from file, if successfully import, refresh grid
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportAsynClick(object sender, RoutedEventArgs e)
        {
            bool imported = await DataManager.ImportSeries();
            if (imported) ResetIndexList();
        }

        private void ExportClick(object sender, RoutedEventArgs e) =>
            DataManager.ExportSeries();

        private void FilterClick(object sender, RoutedEventArgs e) => FilterSeries();

        private void ClearFilterClick(object sender, RoutedEventArgs e) => ResetFilter();

        private void SeriesItemClick(object sender, ItemClickEventArgs e) =>
            MainPage.NavigateDetailView((SeriesIndex)e.ClickedItem);

        private void TrimTextSuggest(object sender, RoutedEventArgs e) =>
            UIDictionary.TrimTextSuggest(sender, e);

        private void SaveClick(object sender, RoutedEventArgs e) =>
            DataManager.SaveData();

        private void CompanyTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e) =>
            UIDictionary.CompanyTextChanged(sender, e);

        private void SearchTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs e) =>
            UIDictionary.SearchTextChanged(sender, e);

        private void SearchQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs e) =>
            UIDictionary.SearchQuerySubmitted(sender, e);
    }
}
