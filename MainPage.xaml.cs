using Windows.UI.Xaml.Controls;
using BangumiArchive.UIModule;

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
            MainPageFrame.Navigate(typeof(AnimeGridView));
            ScheduleFrame.Navigate(typeof(ScheduleView));
            StatisticsFrame.Navigate(typeof(StatisticsView));
            OtherFrame.Navigate(typeof(OtherView));
            
        }

        public static void NavigateAnimeGridView()
        {
            static_MainPivot.SelectedIndex = 0;
        }

        public static void ShowAnimeDetail(int index)
        {
            static_MainPageFrame.Navigate(typeof(AnimeInfoView), index);
        }
    }
}
