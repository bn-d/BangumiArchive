using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.Serialization;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class OtherView : Page
    {
        public ObservableCollection<OtherList> OtherLists => DataManager.OtherLists;

        public OtherView()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Add a new list
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddList(object sender, RoutedEventArgs e)
        {
            OtherLists.Add(new OtherList("New List"));
        }

        /// <summary>
        /// Remove the last list, if the list is not empty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveList(object sender, RoutedEventArgs e)
        {
            if (OtherLists.Any())
                OtherLists.RemoveAt(OtherLists.Count - 1);
        }

        private async void ImportOtherList(object sender, RoutedEventArgs e)
        {
            bool imported = await DataManager.ImportOtherList();
            if (imported)
            {
                OtherListView.ItemsSource = null;
                OtherListView.ItemsSource = DataManager.OtherLists;
            }
        }

        private void ExportOtherList(object sender, RoutedEventArgs e) =>
            DataManager.ExportOtherList();

        private void SaveData(object sender, RoutedEventArgs e) =>
            DataManager.SaveData();
    }

    /// <summary>
    /// A string list with name
    /// </summary>
    [DataContract]
    public class OtherList
    {
        [DataMember]
        public string Name;
        [DataMember]
        public ObservableCollection<StringWrap> NameList;

        public OtherList(string name)
        {
            Name = name;
            NameList = new ObservableCollection<StringWrap>();
        }
    }

    /// <summary>
    /// A simple wrapper function for string
    /// </summary>
    [DataContract]
    public class StringWrap
    {
        [DataMember] public int Index;
        [DataMember] public string Str;

        public StringWrap(int i)
        {
            Index = i;
            Str = "";
        }
    }
}