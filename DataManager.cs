using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;
using BangumiArchive.UIModule;
using Windows.Security.Cryptography.Core;
using Windows.Storage.Streams;
using Windows.Security.Cryptography;
using System.Runtime.InteropServices.WindowsRuntime;

namespace BangumiArchive
{
    /// <summary>
    /// A class for all the global variable
    /// </summary>
    internal class Global
    {
        public static ObservableCollection<Series> Animes = 
            new ObservableCollection<Series>();

        public static ObservableCollection<Series> FilteredAnimes;
        public static bool IsFiltered;
        public static HashSet<string> CompanyHashSet;

        public static ObservableCollection<OtherList> OtherLists = 
            new ObservableCollection<OtherList>();

        public static ApplicationDataContainer LocalSettings = 
            ApplicationData.Current.LocalSettings;
        public static StorageFolder LocalFolder =
            ApplicationData.Current.LocalFolder;
    }

    /// <summary>
    /// A class for read and write anime data
    /// </summary>
    internal class DataManager
    {
        private static string _animeHash = "";
        private static string _otherHash = "";
        private static string _animeDataName = "animes.dat";
        private static string _otherDataName = "other.dat";


        private static HashAlgorithmProvider md5 = HashAlgorithmProvider.OpenAlgorithm(HashAlgorithmNames.Md5);

        /// <summary>
        /// Initialize all the required data
        /// </summary>
        public static async Task InitializeData()
        {
            await ReadAnime();
            await ReadOtherList();
            SetCompanyList();
        }

        /// <summary>
        /// Save all data to file
        /// </summary>
        public static void SaveData()
        {
            WriteAnime();
            WriteOtherList();
            SetCompanyList();
        }

        /// <summary>
        /// Write anime list to local storage
        /// </summary>
        public static async void WriteAnime()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<Series>));
                serializer.WriteObject(memoryStream, Global.Animes);
                memoryStream.Position = 0;

                StorageFile file = await Global.LocalFolder.CreateFileAsync(_animeDataName,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, reader.ReadToEnd());

                _animeHash = ComputeStreamHash(memoryStream);
            }
        }

        /// <summary>
        /// Read anime list from local storage
        /// </summary>
        public static async Task<bool> ReadAnime()
        {
            try
            {
                StorageFile file = await Global.LocalFolder.GetFileAsync(_animeDataName);
                string xml = await FileIO.ReadTextAsync(file);

                using (Stream stream = new MemoryStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(xml);
                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(ObservableCollection<Series>));
                    Global.Animes = (ObservableCollection<Series>)deserializer.ReadObject(stream);

                    _animeHash = ComputeStreamHash((MemoryStream) stream);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Write other list to local storage
        /// </summary>
        public static async void WriteOtherList()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<OtherList>));
                serializer.WriteObject(memoryStream, Global.OtherLists);
                memoryStream.Position = 0;

                StorageFile file = await Global.LocalFolder.CreateFileAsync(_otherDataName,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, reader.ReadToEnd());

                _otherHash = ComputeStreamHash(memoryStream);
            }
        }

        /// <summary>
        /// Read other list from local storage
        /// </summary>
        public static async Task<bool> ReadOtherList()
        {
            try
            {
                StorageFile file = await Global.LocalFolder.GetFileAsync(_otherDataName);
                string xml = await FileIO.ReadTextAsync(file);

                using (Stream stream = new MemoryStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(xml);
                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(ObservableCollection<OtherList>));
                    Global.OtherLists = (ObservableCollection<OtherList>)deserializer.ReadObject(stream);

                    _otherHash = ComputeStreamHash((MemoryStream)stream);
                }
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Export anime to user selected location
        /// </summary>
        public static async void ExportAnime()
        {
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = _animeDataName,
                FileTypeChoices = { {"Data", new List<string>() { ".dat" }} }
            };

            var file = await savePicker.PickSaveFileAsync();

            if (file == null) return;

            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<Series>));
                serializer.WriteObject(memoryStream, Global.Animes);
                memoryStream.Position = 0;

                await FileIO.WriteTextAsync(file, reader.ReadToEnd());

                var dialog = new MessageDialog(string.Format("{0} animes successfully exported", Global.Animes.Count), "Export succeeded");
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Import anime from user selected file
        /// </summary>
        public static async Task<bool> ImportAnime()
        {
            // Load the flag of the anime through file picker
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".dat" }
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
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(ObservableCollection<Series>));
                    Global.Animes = (ObservableCollection<Series>)deserializer.ReadObject(stream);

                    var dialog = new MessageDialog(string.Format("{0} animes successfully imported", Global.Animes.Count), "Import succeeded");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("", "Import Failed");
                await dialog.ShowAsync();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Export other list to user selected location
        /// </summary>
        public static async void ExportOtherList()
        {
            FileSavePicker savePicker = new FileSavePicker
            {
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                SuggestedFileName = _otherDataName,
                FileTypeChoices = { { "Data", new List<string>() { ".dat" } } }
            };

            var file = await savePicker.PickSaveFileAsync();

            if (file == null) return;

            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<OtherList>));
                serializer.WriteObject(memoryStream, Global.OtherLists);
                memoryStream.Position = 0;

                await FileIO.WriteTextAsync(file, reader.ReadToEnd());

                var dialog = new MessageDialog(string.Format("{0} lists successfully exported", Global.OtherLists.Count), "Export succeed");
                await dialog.ShowAsync();
            }
        }

        /// <summary>
        /// Import other list from user selected file
        /// </summary>
        public static async Task<bool> ImportOtherList()
        {
            // Load the flag of the anime through file picker
            FileOpenPicker openPicker = new FileOpenPicker
            {
                ViewMode = PickerViewMode.Thumbnail,
                SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                FileTypeFilter = { ".dat" }
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
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(ObservableCollection<OtherList>));
                    Global.OtherLists = (ObservableCollection<OtherList>)deserializer.ReadObject(stream);

                    var dialog = new MessageDialog(string.Format("{0} lists successfully imported", Global.OtherLists.Count), "Import succeed");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("", "Import Failed");
                await dialog.ShowAsync();
                return false;
            }
            return true;
        }

        /// <summary>
        /// Create a set of all the anime company
        /// </summary>
        public static void SetCompanyList()
        {
            Global.CompanyHashSet = new HashSet<string>();
            foreach (Series a in Global.Animes)
            {
                foreach (Season s in a.Seasons)
                {
                    Global.CompanyHashSet.Add(s.Company);
                }
            }
        }
        
        /// <summary>
        /// Check whether data have changed since last saving
        /// </summary>
        /// <returns></returns>
        public static bool IsDataChanged()
        {
            string curAnimeHash = ComputeHash(Global.Animes);
            if (!_animeHash.Equals(curAnimeHash))
                return true;
            string curOtherHash = ComputeHash(Global.OtherLists);
            if (!_otherHash.Equals(curOtherHash))
                return true;

            return false;
        }

        /// <summary>
        /// Compute the hash of an object
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns> 
        // Reference: https://alexmg.com/posts/compute-any-hash-for-any-object-in-c
        private static string ComputeHash(object obj)
        {
            DataContractSerializer serializer = new DataContractSerializer(obj.GetType());
            using (MemoryStream memoryStream = new MemoryStream())
            {
                serializer.WriteObject(memoryStream, obj);
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
            //Debug.WriteLine(CryptographicBuffer.EncodeToBase64String(buffHash));
            return CryptographicBuffer.EncodeToBase64String(buffHash);
        }

    }
}
