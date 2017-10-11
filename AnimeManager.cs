using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Popups;

namespace AnimeArchive
{
    /// <summary>
    /// A class for all the global variable
    /// </summary>
    internal class Global
    {
        public static ObservableCollection<Anime> Animes;
        public static HashSet<string> CompanyHashSet;

        public static ApplicationDataContainer LocalSettings = 
            ApplicationData.Current.LocalSettings;
        public static StorageFolder LocalFolder =
            ApplicationData.Current.LocalFolder;
    }

    /// <summary>
    /// A class for read and write anime data
    /// </summary>
    internal class AnimeManager
    {
        private static string _animeDataName = "animes.dat";

        /// <summary>
        /// Initialize all the required data
        /// </summary>
        public static void InitializeData()
        {
            ReadAnime();
            SetCompanyList();
        }

        /// <summary>
        /// Write the given anime list to local storage
        /// </summary>
        public static async void WriteAnime()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream))
            {
                DataContractSerializer serializer = new DataContractSerializer(typeof(ObservableCollection<Anime>));
                serializer.WriteObject(memoryStream, Global.Animes);
                memoryStream.Position = 0;

                //Global.localSettings.Values["animeData"] = reader.ReadToEnd();

                StorageFile file = await Global.LocalFolder.CreateFileAsync(_animeDataName,
                    CreationCollisionOption.ReplaceExisting);
                await FileIO.WriteTextAsync(file, reader.ReadToEnd());
            }
        }

        /// <summary>
        /// Read anime list from local storage
        /// </summary>
        public static async void ReadAnime()
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
                Global.Animes = new ObservableCollection<Anime>();
            }
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
        public static async void ImportAnime()
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

                if (file == null) return;

                string xml = await FileIO.ReadTextAsync(file);

                using (Stream stream = new MemoryStream())
                {
                    byte[] data = Encoding.UTF8.GetBytes(xml);
                    stream.Write(data, 0, data.Length);
                    stream.Position = 0;
                    DataContractSerializer deserializer = new DataContractSerializer(typeof(ObservableCollection<Anime>));
                    Global.Animes = (ObservableCollection<Anime>)deserializer.ReadObject(stream);

                    var dialog = new MessageDialog("", "Import Successful");
                    await dialog.ShowAsync();
                }
            }
            catch (Exception)
            {
                var dialog = new MessageDialog("", "Import Failed");
                await dialog.ShowAsync();
            }
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
