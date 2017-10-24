using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace AnimeArchive.UIModule
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class StatisticsView : Page
    {
        private int animeNum => Global.Animes.Count;
        private TimeSpan totalTime;
        private int watchingNum;

        private int reviewNum;
        private float reviewAvg;
        private ObservableCollection<Pair> reviewRank;

        private int seasonNum;
        private int companyNum => Global.CompanyHashSet.Count;
        private ObservableCollection<Pair> companyRank;
        private ObservableCollection<Pair> companyReviewRank;
        private int songNum => SongList.Count;

        public static ObservableCollection<string> SongList;

        public StatisticsView()
        {
            RefreshStatistic();
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }

        /// <summary>
        /// Refresh all needed statistics 
        /// </summary>
        private void RefreshStatistic()
        {
            watchingNum = 0;
            int totalMin = 0;

            reviewNum = 0;
            int reviewedTotal = 0;
            reviewRank = new ObservableCollection<Pair>
            {
                new Pair("Rank 5", 0),
                new Pair("Rank 4", 0),
                new Pair("Rank 3", 0),
                new Pair("Rank 2", 0),
                new Pair("Rank 1", 0),
                new Pair("No Rank", 0),
            };
            seasonNum = 0;

            Dictionary<string, int> companyDict = new Dictionary<string, int>();
            Dictionary<string, int> companyReviewDict = new Dictionary<string, int>();

            SongList = new ObservableCollection<string>();

            foreach (Anime a in Global.Animes)
            {
                if (UIDictionary.NullBToBool(a.IsWatching))
                    watchingNum += 1;

                reviewRank[a.Rank].Num += 1;
                if (a.Rank != 5)
                {
                    reviewNum += 1;
                    reviewedTotal += 5 - a.Rank;
                }

                // Count info for each season
                foreach (Season s in a.Seasons)
                {
                    totalMin += (s.Episode + s.Extra) * s.Length * s.Time;

                    seasonNum += 1;
                    if (!companyDict.ContainsKey(s.Company))
                        companyDict[s.Company] = 0;

                    companyDict[s.Company] += 1;


                    if (!companyReviewDict.ContainsKey(s.Company))
                        companyReviewDict[s.Company] = 0;

                    companyReviewDict[s.Company] += 5 - a.Rank;

                    foreach (Song ss in s.Songs)
                        SongList.Add(string.Format("{0}    -    {1}", a.Title, ss.Name));
                }
            }

            totalTime = new TimeSpan(0, 0, totalMin, 0);

            reviewAvg = (float) reviewedTotal / reviewNum;

            var companys = companyDict.Keys;

            companyRank = new ObservableCollection<Pair>();
            companyReviewRank = new ObservableCollection<Pair>();
            foreach (string c in companys)
            {
                if (companyDict[c] >= 4)
                {
                    companyRank.Add(new Pair(c, companyDict[c]));
                    float avg = (float) (int) ((float) companyReviewDict[c] / companyDict[c] * 100) / 100;
                    companyReviewRank.Add(new Pair(c, avg));
                }
            }
            companyRank = new ObservableCollection<Pair>(companyRank.OrderByDescending(i => i.Num));
            companyReviewRank = new ObservableCollection<Pair>(companyReviewRank.OrderByDescending(i => i.Num));
        }

        /// <summary>
        /// Refresh the view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshStatistic();
            Bindings.Update();
        }

        /// <summary>
        /// Navigate to song page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongClick(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(SongView));
        }

        private void CompanyBySeason(object sender, RoutedEventArgs e)
        {
            CompanyRank.ItemsSource = companyRank;
        }

        private void CompanyByReview(object sender, RoutedEventArgs e)
        {
            CompanyRank.ItemsSource = companyReviewRank;
        }
    }

    /// <summary>
    /// A simple pair class
    /// </summary>
    public class Pair
    {
        public float Num;
        public string Name;

        public Pair(string name, float num)
        {
            Name = name;
            Num = num;
        }
    }
}
