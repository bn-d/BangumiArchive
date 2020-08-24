using System;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// The weekly schedule view 
    /// </summary>
    public sealed partial class ScheduleView : Page
    {
        private readonly WeekList weekList;

        public ScheduleView()
        {
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();

            weekList = new WeekList();
            RefreshWeeklyView();
        }

        /// <summary>
        /// Update the schedule info
        /// </summary>
        private void RefreshWeeklyView()
        {
            weekList.Clear();

            foreach (SeriesIndex i in DataManager.WatchedIdx)
            {
                if (i.Series.IsWatching && i.Series.HasSeason())
                {
                    weekList.AddSeries(i.Series.Seasons.Last().Date.DayOfWeek, i);
                }
            }
        }

        /// <summary>
        /// Refresh the schedule view
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshWeeklyView();
            Bindings.Update();
        }

        /// <summary>
        /// Navigate to corresponding series detail page 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SeriesItemClick(object sender, ItemClickEventArgs e)
        {
            MainPage.NavigateDetailView((SeriesIndex)e.ClickedItem);
        }
    }

    public class DayList
    {
        public DayOfWeek DayOfWeek { get; }
        public ObservableCollection<SeriesIndex> Series { get; }

        public DayList(DayOfWeek dow)
        {
            DayOfWeek = dow;
            Series = new ObservableCollection<SeriesIndex>();
        }

        public void AddSeries(SeriesIndex i) { Series.Add(i); }

        public void Clear() { Series.Clear(); }
    }

    public class WeekList
    {
        public ObservableCollection<DayList> Days { get; }

        public WeekList()
        {
            Days = new ObservableCollection<DayList>();
            for (int i = 0; i < 7; ++i)
            {
                Days.Add(new DayList((DayOfWeek)i));
            }
        }

        public void AddSeries(DayOfWeek dow, SeriesIndex index)
        {
            Days[(int)dow].AddSeries(index);
        }

        public void Clear()
        {
            foreach (DayList day in Days)
            {
                day.Clear();
            }
        }
    }
}
