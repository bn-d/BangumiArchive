using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The statistics view
    /// </summary>
    public sealed partial class StatisticsView : Page
    {
        private int animeNum;
        private TimeSpan totalTime;
        private int watchingNum;

        private int reviewNum;
        private float reviewAvg;
        private ObservableCollection<Pair> reviewRank;

        private int seasonNum;
        private int companyNum;
        private ObservableCollection<Pair> companyRank;
        private ObservableCollection<Pair> companyReviewRank;

        private int songNum;
        public static ObservableCollection<string> SongList;

        public StatisticsView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();

            RefreshStatistic();
        }

        /// <summary>
        /// Refresh the statistics view info
        /// </summary>
        private void RefreshStatistic()
        {
            animeNum = DataManager.SIs.Count;

            {
                int totalMin = DataManager.SIs.Sum(cur =>
                {
                    int temp = 0;
                    foreach (Season s in cur.Series.Seasons)
                    {
                        temp += (s.Episode + s.Extra) * s.Length * s.Time;
                    }
                    return temp;
                });
                totalTime = new TimeSpan(0, 0, totalMin, 0);
            }

            watchingNum = DataManager.SIs.Count(cur => cur.Series.IsWatching);

            reviewNum = DataManager.SIs.Count(cur => cur.Series.Review != Review.NoRank);

            {
                int reviewedTotal = DataManager.SIs.Sum(cur => 5 - (int)cur.Series.Review);
                reviewAvg = (float)reviewedTotal / reviewNum;
            }

            reviewRank = DataManager.SIs.Aggregate(new ObservableCollection<Pair>
            {
                new Pair("Rank 5", 0),
                new Pair("Rank 4", 0),
                new Pair("Rank 3", 0),
                new Pair("Rank 2", 0),
                new Pair("Rank 1", 0),
                new Pair("No Rank", 0),
            }, (acc, cur) =>
            {
                acc[(int)cur.Series.Review].Num += 1;
                return acc;
            });

            seasonNum = DataManager.SIs.Aggregate(0, (acc, cur) => acc + cur.Series.Seasons.Count);

            // Count company related statistics
            {
                Dictionary<string, int> companyDict = DataManager.SIs.Aggregate(new Dictionary<string, int>(),
                    (acc, cur) =>
                    {
                        foreach (Season s in cur.Series.Seasons)
                        {
                            if (!acc.ContainsKey(s.Company))
                            { acc[s.Company] = 0; }

                            acc[s.Company] += 1;
                        }
                        return acc;
                    });
                var companys = companyDict.Keys;
                companyRank = companys.Aggregate(new ObservableCollection<Pair>(), (acc, cur) =>
                {
                    if (companyDict[cur] >= 4)
                    {
                        acc.Add(new Pair(cur, companyDict[cur]));
                    }
                    return acc;
                });
                companyRank = new ObservableCollection<Pair>(companyRank.OrderByDescending(i => i.Num));

                companyNum = companys.Count;

                Dictionary<string, int> companyReviewDict = DataManager.SIs.Aggregate(new Dictionary<string, int>(),
                    (acc, cur) =>
                    {
                        foreach (Season s in cur.Series.Seasons)
                        {
                            if (!acc.ContainsKey(s.Company))
                                acc[s.Company] = 0;

                            acc[s.Company] += 5 - (int)cur.Series.Review;
                        }
                        return acc;
                    });
                companyReviewRank = companys.Aggregate(new ObservableCollection<Pair>(), (acc, cur) =>
                {
                    if (companyDict[cur] >= 4)
                    {
                        float avg = (float)(int)((float)companyReviewDict[cur] / companyDict[cur] * 100) / 100;
                        acc.Add(new Pair(cur, avg));
                    }
                    return acc;
                });
                companyReviewRank = new ObservableCollection<Pair>(companyReviewRank.OrderByDescending(i => i.Num));
            }

            // Count song related statistics
            SongList = DataManager.SIs.Aggregate(new ObservableCollection<string>(),
                (acc, cur) =>
                {
                    foreach (Season s in cur.Series.Seasons)
                    {
                        foreach (Song ss in s.Songs)
                            acc.Add(string.Format("{0}    -    {1}", cur.Series.Title, ss.Name));
                    }
                    return acc;
                });

            songNum = SongList.Count;

            Bindings.Update();
        }

        /// <summary>
        /// Refresh the statistics view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshClick(object sender, RoutedEventArgs e) => RefreshStatistic();

        /// <summary>
        /// Navigate to the song page
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SongClick(object sender, RoutedEventArgs e) => Frame.Navigate(typeof(SongView));

        /// <summary>
        /// Sort companies by number of seasons watched
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompanyBySeasonClick(object sender, RoutedEventArgs e)
        {
            CompanyRank.ItemsSource = companyRank;
        }

        /// <summary>
        /// Sort companies by average review score
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompanyByReviewClick(object sender, RoutedEventArgs e)
        {
            CompanyRank.ItemsSource = companyReviewRank;
        }

        /// <summary>
        /// Navigate to anime grid view and show animes with clicked review
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ReviewItemClick(object sender, ItemClickEventArgs e)
        {
            MainView.ReviewFilter = ((Pair)e.ClickedItem).Name switch
            {
                "Rank 5" => Review.Rank5,
                "Rank 4" => Review.Rank4,
                "Rank 3" => Review.Rank3,
                "Rank 2" => Review.Rank2,
                "Rank 1" => Review.Rank1,
                _ => Review.NoRank,
            };
            MainPage.NavigateMainView();
        }

        /// <summary>
        /// Navigate to anime grid view and show animes with clicked company
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompanyItemClick(object sender, ItemClickEventArgs e)
        {
            MainView.CompanyFilter = ((Pair)e.ClickedItem).Name;
            MainPage.NavigateMainView();
        }
    }

    /// <summary>
    /// A simple string, float pair
    /// </summary>
    public class Pair
    {
        public string Name;
        public float Num;

        public Pair(string name, float num)
        {
            Name = name;
            Num = num;
        }
    }
}
