using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The to-watch list view
    /// </summary>
    public sealed partial class ToWatchView : Page
    {
        public ToWatchView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }

        /// <summary>
        /// Update the to-watch series list
        /// </summary>
        public void Refresh()
        {
            ToWatchGrid.ItemsSource = null;
            ToWatchGrid.ItemsSource = DataManager.ToWatchIdx;
        }

        /// <summary>
        /// Add new series to to-watch list
        /// </summary>
        public static async void AddToWatchClickAsync()
        {
            // Show input dialog to get the new Series name
            var dialog = new InputDialog("Add To-Watch Series");
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            // Create new Series
            var si = DataManager.ToWatch.Add(dialog.Text);

            // Display the Series' details
            MainPage.NavigateDetailView(si);
        }

        /// <summary>
        /// Move the current series to watched list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToMainClick(object sender, RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;
            SeriesIndex idx = (SeriesIndex)button.DataContext;
            var si = DataManager.ToWatch.MoveToWatched(idx.Index);
            if (!MainView.IsFiltered)
            {
                MainView.StaticIndices.Add(si);
            }

            MainPage.NavigateDetailView(si);
        }

        /// <summary>
        /// Delete the current series from to-watch list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteToWatchClick(object sender, RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;
            SeriesIndex idx = (SeriesIndex)button.DataContext;
            DataManager.ToWatch.Remove(idx.Index);
        }

        private void ToWatchItemClick(object sender, ItemClickEventArgs e) =>
            MainPage.NavigateDetailView((SeriesIndex)e.ClickedItem);
    }
}
