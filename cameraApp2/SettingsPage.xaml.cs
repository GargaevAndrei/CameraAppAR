using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace CameraCOT
{    

    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        private static StreamResolution myStreamResolution = null;
        MediaCapture mediaCaptureTemp;
        public static SettingsPage CurrentSettings;


        public SettingsPage()
        {
            this.InitializeComponent();
            CurrentSettings = this;

            foreach (var camera in MainPage.cameraDeviceList)
            {
                ComboBoxItem comboBoxItem1 = new ComboBoxItem();
                comboBoxItem1.Content = camera.Name;
                comboBoxItem1.Tag = camera.Id;
                MainCamera.Items.Add(comboBoxItem1);


                ComboBoxItem comboBoxItem2 = new ComboBoxItem();
                comboBoxItem2.Content = camera.Name;
                comboBoxItem2.Tag = camera.Id;
                TermoCamera.Items.Add(comboBoxItem2);

                ComboBoxItem comboBoxItem3 = new ComboBoxItem();
                comboBoxItem3.Content = camera.Name;
                comboBoxItem3.Tag = camera.Id;
                EndoCamera.Items.Add(comboBoxItem3);

            }

        }

        private async void MainCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {
            var cameraDevice = (ComboBoxItem)MainCamera.SelectedItem;
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = (string)cameraDevice.Tag };

            mediaCaptureTemp = new MediaCapture();
            await mediaCaptureTemp.InitializeAsync(settings);

            //Find video resolution settings
            IEnumerable<StreamResolution> allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                MainVideoSettings.Items.Add(comboBoxItem);
            }

            //Find preview resolution settings
            allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                MainPreviewSettings.Items.Add(comboBoxItem);
            }

            //Find photo resolution settings
            allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                MainPhotoSettings.Items.Add(comboBoxItem);
            }

        }        

        private async void TermoCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {
            var cameraDevice = (ComboBoxItem)TermoCamera.SelectedItem;
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = (string)cameraDevice.Tag };

            mediaCaptureTemp = new MediaCapture();
            await mediaCaptureTemp.InitializeAsync(settings);

            //Find video resolution settings
            IEnumerable<StreamResolution> allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                TermoVideoSettings.Items.Add(comboBoxItem);
            }

            //Find preview resolution settings
            allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                TermoPreviewSettings.Items.Add(comboBoxItem);
            }

            //Find photo resolution settings
            allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                TermoPhotoSettings.Items.Add(comboBoxItem);
            }
        }      

        private async void EndoCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {
            var cameraDevice = (ComboBoxItem)EndoCamera.SelectedItem;
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = (string)cameraDevice.Tag };

            mediaCaptureTemp = new MediaCapture();
            await mediaCaptureTemp.InitializeAsync(settings);

            //Find video resolution settings
            IEnumerable<StreamResolution> allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoRecord).Select(x => new StreamResolution(x));            
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                EndoVideoSettings.Items.Add(comboBoxItem);
            }

            //Find preview resolution settings
            allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                EndoPreviewSettings.Items.Add(comboBoxItem);
            }

            //Find photo resolution settings
            allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.Photo).Select(x => new StreamResolution(x));
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                EndoPhotoSettings.Items.Add(comboBoxItem);
            }

        }
       

        private void getMainPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }


  
        public static string fileJsonName = "cameraConfig.json";

        private async void saveSettings_Click(object sender, RoutedEventArgs e)
        {
            

            JsonCamerasSettings jsonCamerasSettings = new JsonCamerasSettings();

            var tempSelectedItem                   =  (ComboBoxItem)MainCamera.SelectedItem;            
            jsonCamerasSettings.MainCameraName     =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)MainVideoSettings.SelectedItem;
            jsonCamerasSettings.MainCameraVideo    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)MainPhotoSettings.SelectedItem;
            jsonCamerasSettings.MainCameraPhoto    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)MainPreviewSettings.SelectedItem;
            jsonCamerasSettings.MainCameraPreview  = (string)tempSelectedItem.Content;                      //(tempSelectedItem.Tag as StreamResolution).EncodingProperties;

            //jsonCamerasSettings.MainEncodingProperties = (VideoEncodingProperties)(tempSelectedItem.Tag as StreamResolution).EncodingProperties;
            //jsonCamerasSettings.MainStreamResolution = new StreamResolution(jsonCamerasSettings.MainEncodingProperties);


            tempSelectedItem                       =  (ComboBoxItem)EndoCamera.SelectedItem;            
            jsonCamerasSettings.EndoCameraName     =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoVideoSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraVideo    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoPhotoSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraPhoto    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoPreviewSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraPreview  =  (string)tempSelectedItem.Content;
                                                   
                                                   
            tempSelectedItem                       =  (ComboBoxItem)TermoCamera.SelectedItem;            
            jsonCamerasSettings.TermoCameraName    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoVideoSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraVideo   =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoPhotoSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraPhoto   =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoPreviewSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraPreview =  (string)tempSelectedItem.Content;




            JsonSerializer serializer = new JsonSerializer();
            //serializer.Converters.Add(new Newtonsoft.Json.Converters.JavaScriptDateTimeConverter());
            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            serializer.Formatting = Formatting.Indented;

            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync(fileJsonName);
            using (StreamWriter sw = new StreamWriter(file.Path))
            using (JsonWriter writer = new Newtonsoft.Json.JsonTextWriter(sw))
            {
                serializer.Serialize(writer, jsonCamerasSettings, typeof(JsonCamerasSettings));
            }

        }


        //JsonCamerasSettings jsonCamerasSettings;

        private async void readSettings_Click(object sender, RoutedEventArgs e)
        {
            //JsonCamerasSettings jsonCamerasSettings = await JsonCamerasSettings.readFileSettings();

            // jsonCamerasSettings.readSettings();

            //StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            // получаем файл
            //StorageFile configFile = await localFolder.GetFileAsync("cameraConfig2.json");
            //string text = await FileIO.ReadTextAsync(configFile);
            //string text = await jsonCamerasSettings.readFileSettings();

            //jsonCamerasSettings = JsonConvert.DeserializeObject<JsonCamerasSettings>(text);

            

            //EndoCameraName.SelectedValue = jsonCamerasSettings.MainCameraName;
            //ComboBoxItem comboBoxItem = new ComboBoxItem();
            //comboBoxItem.Content = jsonCamerasSettings.MainCameraName;
            //comboBoxItem.Tag = jsonCamerasSettings.MainCameraName;
            //EndoCameraName.Tag = jsonCamerasSettings.MainCameraName;

        }
    }


    [Serializable]
    class JsonCamerasSettings
    {

        //private VideoEncodingProperties _properties;

        //private StreamResolution streamResolution;// = new StreamResolution();
        //public VideoEncodingProperties MainEncodingProperties { get { return _properties; } set { _properties = value; } }

        //public StreamResolution MainStreamResolution { get { return streamResolution; } set { streamResolution = value; } }

        public string MainCameraName     { get; set; }
        public string MainCameraPreview  { get; set; }                //IMediaEncodingProperties
        public string MainCameraPhoto    { get; set; }
        public string MainCameraVideo    { get; set; }
       
        public string EndoCameraName     { get; set; }
        public string EndoCameraPreview  { get; set; }
        public string EndoCameraPhoto    { get; set; }
        public string EndoCameraVideo    { get; set; }

        public string TermoCameraName    { get; set; }
        public string TermoCameraPreview { get; set; }
        public string TermoCameraPhoto   { get; set; }
        public string TermoCameraVideo   { get; set; }

        public JsonCamerasSettings()
        {           

        }

        public static  async Task<JsonCamerasSettings> readFileSettings() 
        {

            //StorageFolder installedLocation = Windows.ApplicationModel.Package.Current.InstalledLocation;
            //StorageFolder folder = await installedLocation.GetFolderAsync("Assets");
            //IReadOnlyList<StorageFile> files = await folder.GetFilesAsync();

            //StorageFile configFile = await folder.GetFileAsync(SettingsPage.fileJsonName);


            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            var localFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;
            StorageFile configFile = await localFolder.GetFileAsync(SettingsPage.fileJsonName);


            string text = await FileIO.ReadTextAsync(configFile);

            return JsonConvert.DeserializeObject<JsonCamerasSettings>(text, new Newtonsoft.Json.JsonSerializerSettings
            {
                Formatting = Formatting.Indented,
                NullValueHandling = NullValueHandling.Include,
                TypeNameHandling = TypeNameHandling.Auto
            });

        }

    }


    public class StreamResolution
    {
        private IMediaEncodingProperties _properties;

        public StreamResolution()
        {
        }

        public StreamResolution(IMediaEncodingProperties properties)
        {
            if (properties == null)
            {
                throw new ArgumentNullException(nameof(properties));
            }

            // Only handle ImageEncodingProperties and VideoEncodingProperties, which are the two types that GetAvailableMediaStreamProperties can return
            if (!(properties is ImageEncodingProperties) && !(properties is VideoEncodingProperties))
            {
                throw new ArgumentException("Argument is of the wrong type. Required: " + typeof(ImageEncodingProperties).Name
                    + " or " + typeof(VideoEncodingProperties).Name + ".", nameof(properties));
            }

            // Store the actual instance of the IMediaEncodingProperties for setting them later
            _properties = properties;
        }

        public uint Width
        {
            get
            {
                if (_properties is ImageEncodingProperties)
                {
                    return (_properties as ImageEncodingProperties).Width;
                }
                else if (_properties is VideoEncodingProperties)
                {
                    return (_properties as VideoEncodingProperties).Width;
                }

                return 0;
            }
        }

        public uint Height
        {
            get
            {
                if (_properties is ImageEncodingProperties)
                {
                    return (_properties as ImageEncodingProperties).Height;
                }
                else if (_properties is VideoEncodingProperties)
                {
                    return (_properties as VideoEncodingProperties).Height;
                }

                return 0;
            }
        }

        public uint FrameRate
        {
            get
            {
                if (_properties is VideoEncodingProperties)
                {
                    if ((_properties as VideoEncodingProperties).FrameRate.Denominator != 0)
                    {
                        return (_properties as VideoEncodingProperties).FrameRate.Numerator / (_properties as VideoEncodingProperties).FrameRate.Denominator;
                    }
                }

                return 0;
            }
        }

        public double AspectRatio
        {
            get { return Math.Round((Height != 0) ? (Width / (double)Height) : double.NaN, 2); }
        }

        public IMediaEncodingProperties EncodingProperties
        {
            get { return _properties; }
            //get; set;
        }

        /// <summary>
        /// Output properties to a readable format for UI purposes
        /// eg. 1920x1080 [1.78] 30fps MPEG
        /// </summary>
        /// <returns>Readable string</returns>
        public string GetFriendlyName(bool showFrameRate = true)
        {
            if (_properties is ImageEncodingProperties ||
                !showFrameRate)
            {
                return Width + "x" + Height + " [" + AspectRatio + "] " + _properties.Subtype;
            }
            else if (_properties is VideoEncodingProperties)
            {
                return Width + "x" + Height + " [" + AspectRatio + "] " + FrameRate + "FPS " + _properties.Subtype;
            }

            return String.Empty;
        }
    }

}
