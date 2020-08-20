using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace BangumiArchive
{
    /// <summary>
    /// The data structure that holds each Bangumi item 
    /// </summary>
    [DataContract]
    public class BangumiNotebook
    {
        [DataMember]
        internal string Title;
        [DataMember]
        internal ObservableCollection<Series> Watched;
        [DataMember]
        internal ObservableCollection<Series> ToWatch;

        public BangumiNotebook(string t)
        {
            Title = t;
            Watched = new ObservableCollection<Series>();
            ToWatch = new ObservableCollection<Series>();
        }
    }

    /// <summary>
    /// Anime class that contains the index, name and other information of an anime 
    /// </summary>
    [DataContract]
    public class Series : INotifyPropertyChanged
    {
        [DataMember]
        public string Title;
        [DataMember]
        internal string SubTitle;
        [DataMember]
        private int Rank;
        internal Review Review { 
            get { return (Review)Rank; } 
            set 
            { 
                Rank = (int)value;
                OnPropertyChanged("Review");
            } 
        }
        [DataMember]
        internal ObservableCollection<Season> Seasons;
        [DataMember]
        internal ObservableCollection<string> CVs;
        [DataMember]
        internal bool IsWatching;
        public bool? IsWatchingNullable
        {
            get { return IsWatching; }
            set { IsWatching = (bool)value; }
        }
        [DataMember]
        internal bool IsLive;
        public bool? IsLiveNullable
        {
            get { return IsLive; }
            set { IsLive = (bool)value; }
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

        public Series(string t = "")
        {
            Title = t;
            SubTitle = "";
            Review = Review.NoRank;
            Seasons = new ObservableCollection<Season>();
            CVs = new ObservableCollection<string>();
            IsWatchingNullable = false;
            IsLiveNullable = false;

            FlagByte = null;
        }

        public bool HasSeason() { return Seasons.Count > 0; }

        public override string ToString() { return Title; }

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

        internal void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// Enum for series review
    /// </summary>
    public enum Review
    {
        Rank5 = 0,
        Rank4 = 1,
        Rank3 = 2,
        Rank2 = 3,
        Rank1 = 4,
        NoRank = 5,
    }

    public static class ReviewHelper
    {
        public readonly static List<Review> All = Enum.GetValues(typeof(Review)).Cast<Review>().ToList();
        public static int Count => All.Count;

        public static string ToString(Review r)
        {
            return r switch
            {
                Review.Rank5 => "Rank 5",
                Review.Rank4 => "Rank 4",
                Review.Rank3 => "Rank 3",
                Review.Rank2 => "Rank 2",
                Review.Rank1 => "Rank 1",
                _ => "No Rank",
            };
        }

        public static Review FromString(string s)
        { 
            return s switch
            {
                "Rank 5" => Review.Rank5,
                "Rank 4" => Review.Rank4,
                "Rank 3" => Review.Rank3,
                "Rank 2" => Review.Rank2,
                "Rank 1" => Review.Rank1,
                _ => Review.NoRank,
            };
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
}
