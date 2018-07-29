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
using AnimeArchive.UIModule;

namespace AnimeArchive
{
    /// <summary>
    /// A class for all the global variable
    /// </summary>
    internal class Global
    {
        public static ObservableCollection<Anime> Animes = 
            new ObservableCollection<Anime>();

        public static ObservableCollection<Anime> FilteredAnimes;
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
        private static string _animeDataName = "animes.dat";
        private static string _otherDataName = "other.dat";

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
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<Anime>));
                serializer.WriteObject(memoryStream, Global.Animes);
                memoryStream.Position = 0;

                StorageFile file = await Global.LocalFolder.CreateFileAsync(_animeDataName,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, reader.ReadToEnd());
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
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(ObservableCollection<Anime>));
                    Global.Animes = (ObservableCollection<Anime>)deserializer.ReadObject(stream);
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
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<Anime>));
                serializer.WriteObject(memoryStream, Global.Animes);
                memoryStream.Position = 0;

                await FileIO.WriteTextAsync(file, reader.ReadToEnd());

                var dialog = new MessageDialog("", "Export Successful");
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
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(ObservableCollection<Anime>));
                    Global.Animes = (ObservableCollection<Anime>)deserializer.ReadObject(stream);

                    var dialog = new MessageDialog("", string.Format("Import {0} anime Successful", Global.Animes.Count));
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

                var dialog = new MessageDialog("", "Export Successful");
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

                    var dialog = new MessageDialog("", string.Format("Import {0} list Successful", Global.OtherLists.Count));
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
            foreach (Anime a in Global.Animes)
            {
                foreach (Season s in a.Seasons)
                {
                    Global.CompanyHashSet.Add(s.Company);
                }
            }
        }
    }
}
