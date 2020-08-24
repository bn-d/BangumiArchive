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
        /// Add new series to to-watch list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddToWatchClickAsync(object sender, RoutedEventArgs e)
        {
            // Show input dialog to get the new Series name
            var dialog = new InputDialog();
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            // Create new Series
            string name = dialog.Text;
            SeriesIndex si = DataManager.AddToWatchSeries(name);
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
            var si = DataManager.MoveToWatchedSeries(idx.Index);
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
            DataManager.RemoveToWatchSeries(idx.Index);
        }

        private void ToWatchItemClick(object sender, ItemClickEventArgs e) =>
            MainPage.NavigateDetailView((SeriesIndex)e.ClickedItem);
    }
}
