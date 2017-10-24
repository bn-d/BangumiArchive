using Windows.UI.Xaml.Controls;
using AnimeArchive.UIModule;

namespace AnimeArchive
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            // Navigate to AnimeGridView
            MainPageFrame.Navigate(typeof(AnimeGridView));
            StatisticsFrame.Navigate(typeof(StatisticsView));
            OtherFrame.Navigate(typeof(OtherView));
        }
    }
}
