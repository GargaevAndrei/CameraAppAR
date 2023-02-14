using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Storage;


namespace CameraCOT
{
    [Serializable]
    class JsonCamerasSettings
    {
        public string MainCameraName     { get; set; }        
        public string MainCameraPhoto    { get; set; }
        public string MainCameraVideo    { get; set; }
       
        public string EndoCameraName     { get; set; }
        public string EndoCameraPhoto    { get; set; }
        public string EndoCameraVideo    { get; set; }

        public string TermoCameraName    { get; set; }
        public string TermoCameraPhoto   { get; set; }
        public string TermoCameraVideo   { get; set; }

        public string SerialPortEndo     { get; set; }
        public string SerialPortFlash    { get; set; }
        public string SerialPortLepton   { get; set; }

        public List<int> BadPixelList    { get; set; }

        public string EndoHeadType { get; set; }

        public JsonCamerasSettings()
        {
            BadPixelList = new List<int>();
        }

        public static  async Task<JsonCamerasSettings> readFileSettings() 
        {
            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
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
