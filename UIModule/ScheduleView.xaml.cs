using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.Appointments;
using Windows.ApplicationModel.VoiceCommands;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Toolkit.Uwp;

namespace BangumiArchive.UIModule
{
    /// <summary>
    /// Show the schedule of currently watching series
    /// </summary>
    public sealed partial class ScheduleView : Page
    {
        private readonly WeekList weekList;

        public ScheduleView()
        {
            weekList = new WeekList();
            RefreshWeeklyView();
            InitializeComponent();
            DataContextChanged += (s, e) => Bindings.Update();
        }

        private void RefreshWeeklyView()
        {
            weekList.Clear();

            int index = 0;
            foreach (Series s in Global.Animes)
            {
                if (s.IsWatching && s.HasSeason())
                {
                    weekList.AddSeries(s.Seasons.Last().Date.DayOfWeek, index);
                }
                ++index;
            }
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            RefreshWeeklyView();
            Bindings.Update();
        }

        private void SeriesItemClick(object sender, ItemClickEventArgs e)
        {
            MainPage.ShowAnimeDetail((int)e.ClickedItem);
            MainPage.NavigateAnimeGridView();
        }

    }

    public class DayList
    {
        public DayOfWeek DayOfWeek { get; }
        public ObservableCollection<int> Series { get; }

        public DayList(DayOfWeek dow)
        {
            DayOfWeek = dow;
            Series = new ObservableCollection<int>();
        }

        public void AddSeries(int i) { Series.Add(i); }

        public void Clear() { Series.Clear(); }
    }

    public class WeekList
    {
        public ObservableCollection<DayList> Days { get; }
        public DayList Monday;

        public WeekList()
        {
            Days = new ObservableCollection<DayList>();
            for (int i = 0; i < 7; ++i)
            {
                Days.Add(new DayList((DayOfWeek)i));
            }
            Monday = new DayList(0);
        }

        public void AddSeries(DayOfWeek dow, int index)
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
