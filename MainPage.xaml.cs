using BangumiArchive.UIModule;
using System.Globalization;
using Windows.UI.Xaml.Controls;

namespace BangumiArchive
{
    public sealed partial class MainPage : Page
    {

        private static Pivot static_MainPivot;
        private static Frame static_MainFrame;
        private static Frame static_ToWatchFrame;

        public MainPage()
        {
            InitializeComponent();

            static_MainPivot = MainPivot;
            static_MainFrame = MainPageFrame;
            static_ToWatchFrame = ToWatchFrame;

            // Navigate to AnimeGridView
            MainPageFrame.Navigate(typeof(MainView));
            ToWatchFrame.Navigate(typeof(ToWatchView));
            ScheduleFrame.Navigate(typeof(ScheduleView));
            StatisticsFrame.Navigate(typeof(StatisticsView));
        }

        public static void NavigateMainView()
        {
            static_MainFrame.Navigate(typeof(MainView));
            static_MainPivot.SelectedIndex = 0;
        }

        public static void NavigateToWatchView()
        {
            static_ToWatchFrame.Navigate(typeof(ToWatchView));
            static_MainPivot.SelectedIndex = 1;
        }

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
    }
}
