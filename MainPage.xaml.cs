using Windows.UI.Xaml.Controls;
using AnimeArchive.UIModule;

namespace AnimeArchive
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            // TODO List
            // Statistic Page
            //     Item Click

            this.InitializeComponent();
            // Iniitialize all the data
            DataManager.InitializeData();
            // Navigate to AnimeGridView
            MainPageFrame.Navigate(typeof(AnimeGridView));
            StatisticsFrame.Navigate(typeof(StatisticsView));
            OtherFrame.Navigate(typeof(OtherView));
        }
    }
}
