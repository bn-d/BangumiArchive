using BangumiArchive.UIModule;
using Windows.UI.Xaml.Controls;

namespace BangumiArchive
{
    public sealed partial class MainPage : Page
    {

        private static Pivot static_MainPivot;
        private static Frame static_MainPageFrame;

        public MainPage()
        {
            InitializeComponent();

            static_MainPivot = MainPivot;
            static_MainPageFrame = MainPageFrame;

            // Navigate to AnimeGridView
            MainPageFrame.Navigate(typeof(MainView));
            ScheduleFrame.Navigate(typeof(ScheduleView));
            StatisticsFrame.Navigate(typeof(StatisticsView));
        }

        public static void NavigateMainView()
        {
            static_MainPageFrame.Navigate(typeof(MainView));
            static_MainPivot.SelectedIndex = 0;
        }

        public static void NavigateDetailView(SeriesIndex index)
        {
            static_MainPageFrame.Navigate(typeof(DetailView), index);
            static_MainPivot.SelectedIndex = 0;
        }
    }
}
