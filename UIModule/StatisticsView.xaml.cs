using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The statistics view
    /// </summary>
    public sealed partial class StatisticsView : Page
    {
        private int seriesNum;
        private TimeSpan totalTime;
        private int watchingNum;

        private int reviewNum;
        private float reviewAvg;
        private ObservableCollection<ReviewIntPair> reviewRank;

        private int seasonNum;
        private int companyNum;
        private ObservableCollection<NamePair> companyRank;
        private ObservableCollection<NamePair> companyReviewRank;

        private int songNum;
        public static ObservableCollection<string> SongList;

        private string versionStr = string.Format("BangumiArchive {0}.{1}.{2}",
            Package.Current.Id.Version.Major,
            Package.Current.Id.Version.Minor,
            Package.Current.Id.Version.Build);

        public StatisticsView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();

            Refresh();
        }

        /// <summary>
        /// Refresh the statistics info
        /// </summary>
        public void Refresh()
        {
            seriesNum = DataManager.WatchedIdx.Count;

            {
                int totalMin = DataManager.WatchedIdx.Sum(cur =>
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

            watchingNum = DataManager.WatchedIdx.Count(cur => cur.Series.IsWatching);

            reviewNum = DataManager.WatchedIdx.Count(cur => cur.Series.Review != Review.NoRank);

            {
                int reviewedTotal = DataManager.WatchedIdx.Sum(cur => 5 - (int)cur.Series.Review);
                reviewAvg = (float)reviewedTotal / reviewNum;
            }

            reviewRank = DataManager.WatchedIdx.Aggregate(
                new ObservableCollection<ReviewIntPair>(ReviewHelper.All.Select(cur => new ReviewIntPair(cur, 0)).ToList()),
                (acc, cur) =>
            {
                acc[(int)cur.Series.Review].Second += 1;
                return acc;
            });

            seasonNum = DataManager.WatchedIdx.Aggregate(0, (acc, cur) => acc + cur.Series.Seasons.Count);

            // Count company related statistics
            {
                Dictionary<string, int> companyDict = DataManager.WatchedIdx.Aggregate(new Dictionary<string, int>(),
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
                companyRank = companys.Aggregate(new ObservableCollection<NamePair>(), (acc, cur) =>
                {
                    if (companyDict[cur] >= 4)
                    {
                        acc.Add(new NamePair(cur, companyDict[cur]));
                    }
                    return acc;
                });
                companyRank = new ObservableCollection<NamePair>(companyRank.OrderByDescending(i => i.Second));

                companyNum = companys.Count;

                Dictionary<string, int> companyReviewDict = DataManager.WatchedIdx.Aggregate(new Dictionary<string, int>(),
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
                companyReviewRank = companys.Aggregate(new ObservableCollection<NamePair>(), (acc, cur) =>
                {
                    if (companyDict[cur] >= 4)
                    {
                        float avg = (float)(int)((float)companyReviewDict[cur] / companyDict[cur] * 100) / 100;
                        acc.Add(new NamePair(cur, avg));
                    }
                    return acc;
                });
                companyReviewRank = new ObservableCollection<NamePair>(companyReviewRank.OrderByDescending(i => i.Second));
            }

            // Count song related statistics
            SongList = DataManager.WatchedIdx.Aggregate(new ObservableCollection<string>(),
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
            MainView.ReviewFilter = ((ReviewIntPair)e.ClickedItem).First;
            MainPage.NavigateMainView();
        }

        /// <summary>
        /// Navigate to anime grid view and show animes with clicked company
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CompanyItemClick(object sender, ItemClickEventArgs e)
        {
            MainView.CompanyFilter = ((NamePair)e.ClickedItem).First;
            MainPage.NavigateMainView();
        }
    }

    public class NamePair : Pair<string, float>
    {
        public string PairString => String.Format("{0}   {1}", First, Second);
        public NamePair(string f, float s) : base(f, s) { }
    }

    public class ReviewIntPair : Pair<Review, int>
    {
        public string PairString => String.Format("{0}   {1}",
            ReviewHelper.ToString(First), Second);
        public ReviewIntPair(Review f, int s) : base(f, s) { }
    }
}
