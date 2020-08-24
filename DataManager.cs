using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Security.Cryptography;
using Windows.Security.Cryptography.Core;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using BangumiArchiveType =
    System.Collections.ObjectModel.ObservableCollection<BangumiArchive.BangumiNotebook>;

namespace BangumiArchive
{
    /// <summary>
    /// A class that wraps Series and index
    /// </summary>
    public class SeriesIndex
    {
        public int Index;
        public bool Watched;

        public SeriesIndex(int index, bool watched) 
        { 
            Index = index;
            Watched = watched;
        }

        public string IndexString => 
            Watched ? (Index + 1).ToString() : "";
        public Series Series => 
            Watched ? DataManager.Watched[Index] : DataManager.ToWatch[Index];
        public Series S => Series;

        public override string ToString() { return Series.Title; }
    }

    /// <summary>
    /// A class for all the global variable
    /// </summary>
    public static class DataManager 
    {
        private static string ArchiveFileName = "BangumiArchive.xml";

        private static int ArcIndex = 0;
        private static BangumiArchiveType Archive;
        private static ObservableCollection<SeriesIndex> WatchedIndices;
        private static ObservableCollection<SeriesIndex> ToWatchIndices;

        private static string SeriesListHash = "";
        public static HashSet<string> CompanyHashSet;

        public static ApplicationDataContainer LocalSettings =
            ApplicationData.Current.LocalSettings;
        public static StorageFolder LocalFolder =
            ApplicationData.Current.LocalFolder;

        internal static ObservableCollection<Series> Watched
        {
            get { return Archive[ArcIndex].Watched; }
            set { Archive[ArcIndex].Watched = value; }
        }
        public static ObservableCollection<SeriesIndex> WatchedIdx => WatchedIndices;
        internal static ObservableCollection<Series> ToWatch
        {
            get { return Archive[ArcIndex].ToWatch; }
            set { Archive[ArcIndex].ToWatch = value; }
        }
        public static ObservableCollection<SeriesIndex> ToWatchIdx => ToWatchIndices;

        private static HashAlgorithmProvider md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        /// <summary>
        /// Add a new series to the watched list
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SeriesIndex AddWatchedSeries(string name)
        {
            int index = Watched.Count;
            Watched.Add(new Series(name));
            WatchedIndices.Add(new SeriesIndex(index, true));

            return WatchedIndices[index];
        }

        /// <summary>
        /// Add a new series to the to-watch list
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static SeriesIndex AddToWatchSeries(string name)
        {
            int index = ToWatch.Count;
            ToWatch.Add(new Series(name));
            ToWatchIndices.Add(new SeriesIndex(index, false));

            return WatchedIndices[index];
        }

        /// <summary>
        /// Move a to-watch series to the watched list
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public static SeriesIndex MoveToWatchedSeries(int index)
        {
            int watchedIndex = Watched.Count;

            Watched.Add(ToWatch[index]);
            WatchedIndices.Add(new SeriesIndex(watchedIndex, true));

            RemoveToWatchSeries(index);
            
            return WatchedIndices[watchedIndex];
        }

        /// <summary>
        /// Delete a to-watch series
        /// </summary>
        /// <param name="index"></param>
        public static void RemoveToWatchSeries(int index)
        {
            ToWatch.RemoveAt(index);
            ToWatchIdx.RemoveAt(index);
            foreach (var si in ToWatchIdx)
            {
                if (si.Index > index) { si.Index -= 1; }
            }
        }

        /// <summary>
        /// Initialize all the required data
        /// </summary>
        public static async Task InitializeData()
        {
            await ReadDataFromLocal();
            ResetSeriesIndices();
            SetCompanyList();
        }

        /// <summary>
        /// Save all data to file and update company list
        /// </summary>
        public static void SaveData()
        {
            WriteDataToLocal();
            SetCompanyList();
        }

        /// <summary>
        /// Write data to local storage
        /// </summary>
        public static async void WriteDataToLocal()
        {
            using MemoryStream memoryStream = new MemoryStream();
            using StreamReader reader = new StreamReader(memoryStream);

            DataContractSerializer serializer =
                new DataContractSerializer(typeof(BangumiArchiveType));
            serializer.WriteObject(memoryStream, Archive);
            memoryStream.Position = 0;

            StorageFile file = await LocalFolder.CreateFileAsync(ArchiveFileName,
                CreationCollisionOption.ReplaceExisting);
            await FileIO.WriteTextAsync(file, reader.ReadToEnd());

            SeriesListHash = ComputeStreamHash(memoryStream);
        }

        /// <summary>
        /// Read data from local storage
        /// </summary>
        public static async Task<bool> ReadDataFromLocal()
        {
            try
            {
                StorageFile file = await LocalFolder.GetFileAsync(ArchiveFileName);
                string xml = await FileIO.ReadTextAsync(file);

                using Stream stream = new MemoryStream();

                byte[] data = Encoding.UTF8.GetBytes(xml);
                stream.Write(data, 0, data.Length);
                stream.Position = 0;
                DataContractSerializer deserializer = new DataContractSerializer(typeof(BangumiArchiveType));
                Archive = (BangumiArchiveType)deserializer.ReadObject(stream);

                SeriesListHash = ComputeStreamHash((MemoryStream)stream);
            }
            catch (Exception)
            {
                Archive = new BangumiArchiveType();
                Archive.Add(new BangumiNotebook("First Notebook"));
                return false;
            }
            return true;
        }

        /// <summary>
        /// Export data to user selected location
        /// </summary>
        public static async void ExportArchive()
        {
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = ArchiveFileName,
                FileTypeChoices = { { "XML", new List<string>() { ".xml" } } }
            };

            var file = await savePicker.PickSaveFileAsync();

            if (file == null) return;

            using MemoryStream memoryStream = new MemoryStream();
            using StreamReader reader = new StreamReader(memoryStream);

            DataContractSerializer serializer = new DataContractSerializer(typeof(BangumiArchiveType));
            serializer.WriteObject(memoryStream, Archive);
            memoryStream.Position = 0;

            await FileIO.WriteTextAsync(file, reader.ReadToEnd());

            var dialog = new MessageDialog(string.Format("{0} Bangumi Notebook successfully exported",
                Archive.Count), "Export succeeded");
            await dialog.ShowAsync();
        }

        /// <summary>
        /// Import data from user selected file
        /// </summary>
        public static async Task<bool> ImportArchive()
        {
            // Load the flag of the data through file picker
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".xml" }
            };

            try
            {
                StorageFile file = await openPicker.PickSingleFileAsync();

                if (file == null) return false;

                string xml = await FileIO.ReadTextAsync(file);

                using (Stream stream = new MemoryStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(xml);
                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(BangumiArchiveType));
                    Archive = (BangumiArchiveType)deserializer.ReadObject(stream);

                    var dialog = new MessageDialog(string.Format("{0} Bangumi Notebook successfully imported", 
                        Archive.Count), "Import succeeded");
                    await dialog.ShowAsync();
                }
                ResetSeriesIndices();
            }
            catch (Exception e)
            {
                var dialog = new MessageDialog(e.ToString(), "Import Failed");
                await dialog.ShowAsync();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create a set of all the company
        /// </summary>
        public static void SetCompanyList()
        {
            DataManager.CompanyHashSet = new HashSet<string>();
            foreach (Series a in Watched)
            {
                foreach (Season s in a.Seasons)
                {
                    CompanyHashSet.Add(s.Company);
                }
            }
        }

        /// <summary>
        /// Check whether data have changed since last saving
        /// </summary>
        /// <returns></returns>
        public static bool IsDataChanged()
        {
            string curHash = ComputeHash();
            if (!SeriesListHash.Equals(curHash))
                return true;

            return false;
        }

        /// <summary>
        /// Reset the index list
        /// </summary>
        private static void ResetSeriesIndices()
        {
            WatchedIndices = new ObservableCollection<SeriesIndex>(
                Enumerable.Range(0, Watched.Count).Select(i => new SeriesIndex(i, true)));
            ToWatchIndices = new ObservableCollection<SeriesIndex>(
                Enumerable.Range(0, ToWatch.Count).Select(i => new SeriesIndex(i, false)));
        }

        /// <summary>
        /// Compute the hash of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        // Reference: https://alexmg.com/posts/compute-any-hash-for-any-object-in-c
        private static string ComputeHash()
        {
            DataContractSerializer serializer = new DataContractSerializer(typeof(BangumiArchiveType));
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, Archive);
                return ComputeStreamHash(memoryStream);
            }
        }

        /// <summary>
        /// Compute the hash of a memory stream
        /// </summary>
        /// <param name="memoryStream"></param>
        /// <returns></returns>
        private static string ComputeStreamHash(MemoryStream memoryStream)
        {
            memoryStream.Position = 0;
            IBuffer buffHash = md5.HashData(memoryStream.GetWindowsRuntimeBuffer());
            return CryptographicBuffer.EncodeToBase64String(buffHash);
        }

    }
}
