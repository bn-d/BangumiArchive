using Windows.UI.Xaml.Controls;
using BangumiArchive.UIModule;

namespace BangumiArchive
{
    public sealed partial class MainPage : Page
    {

        private static Pivot StaticMainPivot;

        public MainPage()
        {
            InitializeComponent();

            StaticMainPivot = MainPivot;

            // Navigate to AnimeGridView
            MainPageFrame.Navigate(typeof(AnimeGridView));
            StatisticsFrame.Navigate(typeof(StatisticsView));
            OtherFrame.Navigate(typeof(OtherView));
            
        }

        public static void NavigateAnimeGridView()
        {
            StaticMainPivot.SelectedIndex = 0;
        }
    }
}
