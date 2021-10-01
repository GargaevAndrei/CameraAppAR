using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Windows.Data.Json;
using Windows.Media.Capture;
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


            tempSelectedItem                       =  (ComboBoxItem)EndoCamera.SelectedItem;            
            jsonCamerasSettings.EndoCameraName     =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoVideoSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraVideo    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoPhotoSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraPhoto    =  (string)tempSelectedItem.Content;                                                   
                                                   
                                                   
            tempSelectedItem                       =  (ComboBoxItem)TermoCamera.SelectedItem;            
            jsonCamerasSettings.TermoCameraName    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoVideoSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraVideo   =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoPhotoSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraPhoto   =  (string)tempSelectedItem.Content;                                                     




            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            serializer.Formatting = Formatting.Indented;

            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            var storageFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;
            var naparnikFolder = (StorageFolder)await storageFolder.TryGetItemAsync("Напарник");
            var file = (StorageFile)await naparnikFolder.TryGetItemAsync("cameraConfig.json");


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

}
