using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
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
        JsonCamerasSettings jsonCamerasSettings;
        //public static string fileJsonName = "cameraConfig.json";


        public SettingsPage()
        {
            this.InitializeComponent();
            CurrentSettings = this;

            updateUI();

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
        

        private async void saveSettings_Click(object sender, RoutedEventArgs e)
        {


            JsonCamerasSettings jsonCamerasSettings = new JsonCamerasSettings();
            jsonCamerasSettings = await JsonCamerasSettings.readFileSettings();

            var tempSelectedItem = (ComboBoxItem)MainCamera.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.MainCameraName = (string)tempSelectedItem.Content;

            tempSelectedItem = (ComboBoxItem)MainVideoSettings.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.MainCameraVideo = (string)tempSelectedItem.Content;

            tempSelectedItem = (ComboBoxItem)MainPhotoSettings.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.MainCameraPhoto = (string)tempSelectedItem.Content;


            tempSelectedItem = (ComboBoxItem)EndoCamera.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.EndoCameraName = (string)tempSelectedItem.Content;

            tempSelectedItem = (ComboBoxItem)EndoVideoSettings.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.EndoCameraVideo = (string)tempSelectedItem.Content;

            tempSelectedItem = (ComboBoxItem)EndoPhotoSettings.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.EndoCameraPhoto = (string)tempSelectedItem.Content;


            tempSelectedItem = (ComboBoxItem)TermoCamera.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.TermoCameraName = (string)tempSelectedItem.Content;

            tempSelectedItem = (ComboBoxItem)TermoVideoSettings.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.TermoCameraVideo = (string)tempSelectedItem.Content;

            tempSelectedItem = (ComboBoxItem)TermoPhotoSettings.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.TermoCameraPhoto = (string)tempSelectedItem.Content;

            tempSelectedItem = (ComboBoxItem)endoHeadType.SelectedItem;
            if (tempSelectedItem != null) jsonCamerasSettings.EndoHeadType = (string)tempSelectedItem.Content;


            JsonSerializer serializer = new JsonSerializer();
            serializer.NullValueHandling = NullValueHandling.Include;
            serializer.TypeNameHandling = TypeNameHandling.Auto;
            serializer.Formatting = Formatting.Indented;

            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            var storageFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;
            var naparnikFolder = (StorageFolder)await storageFolder.TryGetItemAsync("Напарник");
            var file = (StorageFile)await naparnikFolder.TryGetItemAsync("cameraConfig.json");

            string jsonData = JsonConvert.SerializeObject(jsonCamerasSettings, Formatting.Indented);
            await FileIO.WriteTextAsync(file, jsonData);

        }

        private async void readSettings_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            jsonCamerasSettings = await JsonCamerasSettings.readFileSettings();

            foreach (var item in MainCamera.Items)
            {
                if ( ((ComboBoxItem)item).Content.ToString() == jsonCamerasSettings.MainCameraName)
                    MainCamera.SelectedItem = item;
            }

            await Task.Delay(500);

            serialPortEndo.Text   = jsonCamerasSettings.SerialPortEndo;
            serialPortLepton.Text = jsonCamerasSettings.SerialPortLepton;
            serialPortFlash.Text  = jsonCamerasSettings.SerialPortFlash;

            foreach (var item in TermoCamera.Items)
            {
                if (((ComboBoxItem)item).Content.ToString() == jsonCamerasSettings.TermoCameraName)
                    TermoCamera.SelectedItem = item;
            }

            await Task.Delay(500);

            foreach (var item in EndoCamera.Items)
            {
                if (((ComboBoxItem)item).Content.ToString() == jsonCamerasSettings.EndoCameraName)
                    EndoCamera.SelectedItem = item;
            }

        }

        //bool _isMicrophone = false;
        //bool _isBadPixel = false;
        private void MicrophoneButton_Click(object sender, RoutedEventArgs e)
        {
            MainPage._isMicrophone = !MainPage._isMicrophone;
            updateUI();            
        }

        private void BadPixelPanel_Click(object sender, RoutedEventArgs e)
        {
            MainPage._isBadPixel = !MainPage._isBadPixel;

            updateUI();            
        }

        void updateUI()
        {
            BadPixelPanelOff.Visibility = MainPage._isBadPixel ? Visibility.Collapsed : Visibility.Visible;
            BadPixelPanelOn.Visibility = MainPage._isBadPixel ? Visibility.Visible : Visibility.Collapsed;

            MicOff.Visibility = MainPage._isMicrophone ? Visibility.Collapsed : Visibility.Visible;
            MicOn.Visibility = MainPage._isMicrophone ? Visibility.Visible : Visibility.Collapsed;
        }
    }

}
