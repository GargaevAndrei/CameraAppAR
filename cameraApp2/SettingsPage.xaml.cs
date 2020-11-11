﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Data.Json;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
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

        MediaCapture mediaCaptureTemp;

        public SettingsPage()
        {
            this.InitializeComponent();

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

        private void EndoPreviewSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EndoPhotoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EndoVideoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void getMainPage_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        //JsonCamerasSettings jsonCamerasSettings;
        private async void saveSettings_Click(object sender, RoutedEventArgs e)
        {
            JsonCamerasSettings jsonCamerasSettings = new JsonCamerasSettings();

            var tempSelectedItem                   =  (ComboBoxItem)MainCamera.SelectedItem;            
            jsonCamerasSettings.MainCameraName     =  (string)tempSelectedItem.Content;
            jsonCamerasSettings.MainCameraId       =  (string)tempSelectedItem.Tag;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)MainVideoSettings.SelectedItem;
            jsonCamerasSettings.MainCameraVideo    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)MainPhotoSettings.SelectedItem;
            jsonCamerasSettings.MainCameraPhoto    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)MainPreviewSettings.SelectedItem;
            jsonCamerasSettings.MainCameraPreview  =  (string)tempSelectedItem.Content;
                                                   
                                                   
            tempSelectedItem                       =  (ComboBoxItem)EndoCamera.SelectedItem;            
            jsonCamerasSettings.EndoCameraName     =  (string)tempSelectedItem.Content;
            jsonCamerasSettings.EndoCameraId       =  (string)tempSelectedItem.Tag;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoVideoSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraVideo    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoPhotoSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraPhoto    =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)EndoPreviewSettings.SelectedItem;
            jsonCamerasSettings.EndoCameraPreview  =  (string)tempSelectedItem.Content;
                                                   
                                                   
            tempSelectedItem                       =  (ComboBoxItem)TermoCamera.SelectedItem;            
            jsonCamerasSettings.TermoCameraName    =  (string)tempSelectedItem.Content;
            jsonCamerasSettings.TermoCameraId      =  (string)tempSelectedItem.Tag;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoVideoSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraVideo   =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoPhotoSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraPhoto   =  (string)tempSelectedItem.Content;
                                                     
            tempSelectedItem                       =  (ComboBoxItem)TermoPreviewSettings.SelectedItem;
            jsonCamerasSettings.TermoCameraPreview =  (string)tempSelectedItem.Content;



            // serialize JSON to a string
            string json = JsonConvert.SerializeObject(jsonCamerasSettings);

            // write string to a file
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("cameraConfig4.json");
            await FileIO.WriteTextAsync(file, json);



            //var selectedItem = (ComboBoxItem)EndoVideoSettings.SelectedItem;
            //var encodingProperties = (selectedItem.Tag as StreamResolution).EncodingProperties;
            //jsonCamerasSettings.MediaEncodingProperties = (IMediaEncodingProperties)encodingProperties;


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


    class JsonCamerasSettings
    {    

        public string MainCameraName     { get; set; }
        public string MainCameraId       { get; set; }
        public string MainCameraPreview  { get; set; }
        public string MainCameraPhoto    { get; set; }
        public string MainCameraVideo    { get; set; }

        public string EndoCameraName     { get; set; }
        public string EndoCameraId       { get; set; }
        public string EndoCameraPreview  { get; set; }
        public string EndoCameraPhoto    { get; set; }
        public string EndoCameraVideo    { get; set; }

        public string TermoCameraName    { get; set; }
        public string TermoCameraId      { get; set; }
        public string TermoCameraPreview { get; set; }
        public string TermoCameraPhoto   { get; set; }
        public string TermoCameraVideo   { get; set; }

        public IMediaEncodingProperties MediaEncodingProperties { get; set; }

        public JsonCamerasSettings()
        {           

        }

        /*public JsonCamerasSettings(string jsonString) : this()
        {
            JsonObject jsonObject = JsonObject.Parse(jsonString);       

            //StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            //// получаем файл
           // StorageFile configFile = await localFolder.GetFileAsync("cameraConfig2.json");
            //string text =  FileIO.ReadTextAsync(configFile);
            ////this = JsonConvert.DeserializeObject<JsonCamerasSettings>(text);

            //return text;
        }*/


        public static  async Task<JsonCamerasSettings> readFileSettings() 
        {

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            // получаем файл
            StorageFile configFile = await localFolder.GetFileAsync("cameraConfig4.json");
            string text = await FileIO.ReadTextAsync(configFile);
            

            return JsonConvert.DeserializeObject<JsonCamerasSettings>(text); ;
        }

    }


    class StreamResolution
    {
        private IMediaEncodingProperties _properties;

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
