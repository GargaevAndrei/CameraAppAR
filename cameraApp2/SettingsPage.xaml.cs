using Microsoft.Win32;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Data.Json;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

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

            var cameraDevice = (ComboBoxItem) EndoCamera.SelectedItem;
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = (string)cameraDevice.Tag };
            IReadOnlyList<MediaCaptureVideoProfile> profiles = MediaCapture.FindAllVideoProfiles((string)cameraDevice.Tag);            

            mediaCaptureTemp = new MediaCapture();
            await mediaCaptureTemp.InitializeAsync(settings);
            var videoProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview);




        }

        private void MainPreviewSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MainPhotoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MainVideoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoPreviewSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoPhotoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoVideoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private async void EndoCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {
            var cameraDevice = (ComboBoxItem)EndoCamera.SelectedItem;
            var settings = new MediaCaptureInitializationSettings { VideoDeviceId = (string)cameraDevice.Tag };

            mediaCaptureTemp = new MediaCapture();
            await mediaCaptureTemp.InitializeAsync(settings);

            IEnumerable<StreamResolution> allStreamProperties = mediaCaptureTemp.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
            // Order them by resolution then frame rate
            allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

            foreach (var property in allStreamProperties)
            {
                ComboBoxItem comboBoxItem = new ComboBoxItem();
                comboBoxItem.Content = property.GetFriendlyName(true);
                comboBoxItem.Tag = property;
                EndoVideoSettings.Items.Add(comboBoxItem);
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

        private void getMain_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }

        private async void saveSettings_Click(object sender, RoutedEventArgs e)
        {
            JsonCamerasSettings jsonCamerasSettings = new JsonCamerasSettings();

            var cameraDevice = (ComboBoxItem)EndoCamera.SelectedItem;            
            jsonCamerasSettings.Name = (string)cameraDevice.Content;

            cameraDevice = (ComboBoxItem)EndoVideoSettings.SelectedItem;
            jsonCamerasSettings.Id = (string)cameraDevice.Content; ;
            jsonCamerasSettings.Phone = "852";

            // serialize JSON to a string
            string json = JsonConvert.SerializeObject(jsonCamerasSettings);

            // write string to a file
            var file = await ApplicationData.Current.LocalFolder.CreateFileAsync("cameraConfig.json");
            await FileIO.WriteTextAsync(file, json);

        }

        private async void readSettings_Click(object sender, RoutedEventArgs e)
        {
            JsonCamerasSettings jsonCamerasSettings = new JsonCamerasSettings();

            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            // получаем файл
            StorageFile helloFile = await localFolder.GetFileAsync("cameraConfig.json");
            string text = await FileIO.ReadTextAsync(helloFile);
            jsonCamerasSettings = JsonConvert.DeserializeObject<JsonCamerasSettings>(text);
        }
    }


    class JsonCamerasSettings
    {
        private string id;
        private string phone;
        private string name;

        public JsonCamerasSettings()
        {
            Id = "";
            Phone = null;
            Name = "";

        }

        //public JsonCamerasSettings(string jsonString) : this()
        //{
        //    JsonObject jsonObject = JsonObject.Parse(jsonString);
        //    Id = jsonObject.GetNamedString(idKey, "");

        //    IJsonValue phoneJsonValue = jsonObject.GetNamedValue(phoneKey);
        //    if (phoneJsonValue.ValueType == JsonValueType.Null)
        //    {
        //        Phone = null;
        //    }
        //    else
        //    {
        //        Phone = phoneJsonValue.GetString();
        //    }

        //    Name = jsonObject.GetNamedString(nameKey, "");
        //    Timezone = jsonObject.GetNamedNumber(timezoneKey, 0);
        //    Verified = jsonObject.GetNamedBoolean(verifiedKey, false);

        //    foreach (IJsonValue jsonValue in jsonObject.GetNamedArray(educationKey, new JsonArray()))
        //    {
        //        if (jsonValue.ValueType == JsonValueType.Object)
        //        {
        //            Education.Add(new School(jsonValue.GetObject()));
        //        }
        //    }
        //}

        public string Id
        {
            get
            {
                return id;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                id = value;
            }
        }

        public string Phone
        {
            get
            {
                return phone;
            }
            set
            {
                phone = value;
            }
        }

        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                name = value;
            }
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
