using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace CameraCOT
{
    [Serializable]
    class JsonCamerasSettings
    {

        //private VideoEncodingProperties _properties;

        //private StreamResolution streamResolution;// = new StreamResolution();
        //public VideoEncodingProperties MainEncodingProperties { get { return _properties; } set { _properties = value; } }

        //public StreamResolution MainStreamResolution { get { return streamResolution; } set { streamResolution = value; } }

        public string MainCameraName     { get; set; }        
        public string MainCameraPhoto    { get; set; }
        public string MainCameraVideo    { get; set; }
       
        public string EndoCameraName     { get; set; }
        public string EndoCameraPhoto    { get; set; }
        public string EndoCameraVideo    { get; set; }

        public string TermoCameraName    { get; set; }
        public string TermoCameraPhoto   { get; set; }
        public string TermoCameraVideo   { get; set; }

        public List<int> BadPixelList    { get; set; }

        public JsonCamerasSettings()
        {
            BadPixelList = new List<int>();
        }

        public static  async Task<JsonCamerasSettings> readFileSettings() 
        {
            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            //var localFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;
            //StorageFile configFile = await localFolder.GetFileAsync(SettingsPage.fileJsonName);


            var storageFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;
            var naparnikFolder = (StorageFolder)await storageFolder.TryGetItemAsync("Напарник");
            var configFile = (StorageFile)await naparnikFolder.TryGetItemAsync("cameraConfig.json");


            string text = await FileIO.ReadTextAsync(configFile);

            return JsonConvert.DeserializeObject<JsonCamerasSettings>(text, new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.Auto
            });

        }

    }

}
