using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace BangumiArchive
{
    /// <summary>
    /// The data structure that holds each Bangumi item 
    /// </summary>
    [DataContract]
    public class Bangumi
    {
        [DataMember]
        internal string Title;
        [DataMember]
        internal ObservableCollection<Series> Series;
        [DataMember]
        internal ObservableCollection<Series> WatchList;

        public Bangumi(string t) {
            Title = t;
        }
    }

    /// <summary>
    /// Anime class that contains the index, name and other information of an anime 
    /// </summary>
    [DataContract]
    public class Series
    {
        [DataMember]
        internal int Index;
        [DataMember]
        public string Title;
        [DataMember]
        internal string SubTitle;
        [DataMember]
        internal int Rank;
        [DataMember]
        internal ObservableCollection<Season> Seasons;
        [DataMember]
        internal ObservableCollection<string> CVs;
        [DataMember]
        internal bool IsWatching;
        public bool? IsWatchingNullable {
            get { return IsWatching; }
            set { IsWatching = value != null && (bool)value; }
        }
        [DataMember]
        internal bool IsLive;
        public bool? IsLiveNullable
        {
            get { return IsLive; }
            set { IsLive = value != null && (bool)value; }
        }

        [DataMember]
        internal byte[] FlagByte;
        public BitmapImage Flag
        {
            get
            {
                if (FlagByte == null)
                    return null;
                ByteToBitmap();
                return _flag;
            }
        }
        private BitmapImage _flag;

        public Series(int n, string t="")
        {
            Index = n;
            Title = t;
            SubTitle = "";
            Rank = 5;
            Seasons = new ObservableCollection<Season>();
            CVs = new ObservableCollection<string>();
            IsWatchingNullable = false;
            IsLiveNullable = false;

            FlagByte = null;
        }

        public bool HasSeason() { return Seasons.Count() > 0; }

        public override string ToString() { return Title; }

        public static Brush GetRankColorBrush(int value)
        {
            switch (value)
            {
                case 0:
                    return new SolidColorBrush(Colors.White);
                case 1:
                    return new SolidColorBrush(Color.FromArgb(255, 91, 235, 226));
                case 2:
                    return new SolidColorBrush(Color.FromArgb(255, 224, 231, 16));
                case 3:
                    return new SolidColorBrush(Color.FromArgb(255, 191, 191, 191));
                case 4:
                    return new SolidColorBrush(Color.FromArgb(255, 240, 133, 61));
                default:
                    return new SolidColorBrush(Colors.Black);
            }
        }

        /// <summary>
        /// Convert byte[] to bitmap image
        /// </summary>
        private async void ByteToBitmap()
        {

            if (_flag == null)
                _flag = new BitmapImage();
            using (InMemoryRandomAccessStream stream = new InMemoryRandomAccessStream())
            {
                await stream.WriteAsync(FlagByte.AsBuffer());
                stream.Seek(0);
                await _flag.SetSourceAsync(stream);
            }
        }

    }

    /// <summary>
    /// Season class that contains the information of a season
    /// </summary>
    [DataContract]
    internal class Season
    {
        [DataMember] internal string SeasonTitle;
        [DataMember] internal DateTime Date;
        [DataMember] internal string Company;
        [DataMember] internal ObservableCollection<Song> Songs;
        [DataMember] internal int Episode;
        [DataMember] internal int Extra;
        [DataMember] internal int Time;
        [DataMember] internal int Length;

        public Season()
        {
            SeasonTitle = "";
            Date = DateTime.Today;
            Company = "";
            Songs = new ObservableCollection<Song>();
            Extra = 0;
            Time = 1;
            Episode = 12;
            Length = 24;
        }
    }

    [DataContract]
    internal class Song
    {
        [DataMember] public string Name;

        public Song()
        {
            Name = "";
        }
    }

    /// <summary>
    /// Convert DateTime to DateTimeOffset
    /// </summary>
    public class TimeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return new DateTimeOffset(((DateTime)value).ToUniversalTime());

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return ((DateTimeOffset)value).DateTime;
        }
    }

    /// <summary>
    /// Convert rank index to rank color
    /// </summary>
    public class RankColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return Series.GetRankColorBrush((int) value);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Convert song list to string summary
    /// </summary>
    public class SongListConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ObservableCollection<Song> s = (ObservableCollection<Song>)value;
            if (s == null || !s.Any())
                return "Anime Song";
            else if (s.Count == 1)
                return "1 Song";
            else return string.Concat(s.Count.ToString(), " Songs");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
