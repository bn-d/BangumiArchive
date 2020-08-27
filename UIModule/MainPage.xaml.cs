using System;
using Windows.UI.Xaml.Controls;

namespace BangumiArchive.UIModule
{
    public sealed partial class MainPage : Page
    {

        private static Pivot static_MainPivot;
        private static Frame static_MainFrame;
        private static Frame static_ToWatchFrame;

        public MainPage()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();

            static_MainPivot = MainPivot;
            static_MainFrame = MainFrame;
            static_ToWatchFrame = ToWatchFrame;

            // Navigate to AnimeGridView
            MainFrame.Navigate(typeof(MainView));
            ToWatchFrame.Navigate(typeof(ToWatchView));
            ScheduleFrame.Navigate(typeof(ScheduleView));
            StatisticsFrame.Navigate(typeof(StatisticsView));
        }

        /// <summary>
        /// Reload all frames
        /// </summary>
        private void ReloadAll()
        {
            MainFrame.Navigate(typeof(MainView));
            ((MainView)MainFrame.Content).Refresh();

            ToWatchFrame.Navigate(typeof(ToWatchView));
            ((ToWatchView)ToWatchFrame.Content).Refresh();

            ScheduleFrame.Navigate(typeof(ScheduleView));
            ((ScheduleView)ScheduleFrame.Content).Refresh();

            StatisticsFrame.Navigate(typeof(StatisticsView));
            ((StatisticsView)StatisticsFrame.Content).Refresh();
        }

        /// <summary>
        /// Navigate to the MainView
        /// </summary>
        public static void NavigateMainView()
        {
            static_MainFrame.Navigate(typeof(MainView));
            static_MainPivot.SelectedIndex = 0;
        }

        /// <summary>
        /// Navigate to the ToWatchView
        /// </summary>
        public static void NavigateToWatchView()
        {
            static_ToWatchFrame.Navigate(typeof(ToWatchView));
            static_MainPivot.SelectedIndex = 1;
        }

        /// <summary>
        /// Navigate to Series DetailView
        /// </summary>
        /// <param name="index"></param>
        public static void NavigateDetailView(SeriesIndex index)
        {
            if (index.Watched)
            {
                static_MainFrame.Navigate(typeof(DetailView), index);
                static_MainPivot.SelectedIndex = 0;
            }
            else
            {
                static_ToWatchFrame.Navigate(typeof(DetailView), index);
                static_MainPivot.SelectedIndex = 1;
            }
        }

        /// <summary>
        /// Import archive file and reload all
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void ImportClickasync(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            if (await DataManager.ImportArchive()) ReloadAll();

            Bindings.Update();
        }

        /// <summary>
        /// Add new Notebook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void AddNotebookClickAsync(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            // Show input dialog to get the new Notebook name
            var dialog = new InputDialog("Add Nootbook");
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            DataManager.Notebook.Add(dialog.Text);

            ReloadAll();
        }

        /// <summary>
        /// Rename the current Notebook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void RenameNotebookClickAsync(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;
            BangumiNotebook notebook = (BangumiNotebook)button.DataContext;

            // Show input dialog to get the new Notebook name
            var dialog = new InputDialog("Rename Nootbook", notebook.Title);
            var result = await dialog.ShowAsync();

            if (result != ContentDialogResult.Primary) return;

            notebook.Title = dialog.Text;
        }

        /// <summary>
        /// Delete the current Notebook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DeleteNotebookClick(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            AppBarButton button = (AppBarButton)sender;
            if (DataManager.Notebook.Remove((BangumiNotebook)button.DataContext)) ReloadAll();
        }

        /// <summary>
        /// Change the current Notebook
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotebookItemClick(object sender, ItemClickEventArgs e)
        {
            if (DataManager.Notebook.Change((BangumiNotebook)e.ClickedItem)) ReloadAll();
        }
    }
}
