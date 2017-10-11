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
            //     Layout
            //     Refresh function etc
            // Other page
            // Anime filter
            // CVs

            this.InitializeComponent();
            // Iniitialize all the data
            AnimeManager.InitializeData();
            // Navigate to AnimeGridView
            MainPageFrame.Navigate(typeof(AnimeGridView));
        }
    }
}
