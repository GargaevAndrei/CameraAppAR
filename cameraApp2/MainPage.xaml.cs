using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Media.Effects;
using Windows.Media;
using Windows.Devices.HumanInterfaceDevice;
using System.Linq;
using Windows.UI.Core;
using Windows.Security.Cryptography;
using VideoEffectComponent;
using System.IO.Ports;
using Windows.ApplicationModel;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using System.Collections.Generic;
using Windows.Foundation.Collections;




// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace CameraCOT
{


    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        public static MainPage Current;
        SettingsPage SettingsPage = SettingsPage.CurrentSettings;
        // Object to manage access to camera devices
        //private MediaCapturePreviewer _previewer = null;



        private MediaCapture _mediaCapture;        
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private StorageFolder storageFolder = null;
        LowLagMediaRecording _mediaRecording;
        string Lenght;
        double initialLenght = 0;
        double lenght = 0;
        DispatcherTimer lenghtMeterTimer;
        DispatcherTimer flashDelayTimer;
        SerialPort serialPortFlash;
        double Xf;
        double Yf;
        double Zf;
        bool _isFlash = true;
        bool firstDistanceFlag = true;

        private bool _isInitialized;
        private bool _isPreviewing;
        private bool _isActivePage;
        private bool _isSuspending;
        private bool _isRecording;
        private bool _findLenghtZero = false;
        private readonly DisplayRequest _displayRequest = new DisplayRequest();
        private Task _setupTask = Task.CompletedTask;
        private bool _isUIActive;
        public int _cntCamera;  //was private
        private StorageFolder _captureFolder = null;

        public enum NotifyType
        {
            StatusMessage,
            ErrorMessage
        };
        enum camera 
        {
            mainCamera, 
            endoCamera, 
            termoCamera, 
            doubleCamera
        }

        public struct Camera
        {
            public string Name;
            public string Id;
            int width;
            int height;
            int x;
            int y;
            int fontSize;
            IMediaEncodingProperties _properties;

            public void setCameraSettings(string _Name)
            {
                this.Name = _Name;
            }

            public void setCameraSettings(string _Name, IMediaEncodingProperties EncodingProperties)
            {
                this.Name = _Name;
                this._properties = EncodingProperties;                
            }

            public IMediaEncodingProperties EncodingProperties
            {
                get { return _properties; }
            }

            public void setCameraSettings(int _width, int _height, int _x, int _y, int _fontSize)
            {
                this.width = _width;
                this.height = _height;
                this.x = _x;
                this.y = _y;
                this.fontSize = _fontSize;
            }

            public int Width {  get { return width; }   set { width = value; }  }
            public int Height { get { return height; }  set { height = value; } }
            public int X { get { return x; } set { x = value; } }
            public int Y { get { return y; } set { y = value; } }

            public int FontSize { get { return fontSize; } set { fontSize = value; } }

        }

        public static Camera[] cameras;
        JsonCamerasSettings jsonCamerasSettings;
        public int cameraType = (int)camera.mainCamera;                            



        private async Task<JsonCamerasSettings> readFileSettings()
        {
            JsonCamerasSettings jsonCamerasSettings = await JsonCamerasSettings.readFileSettings();
            return jsonCamerasSettings;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            cameras = new Camera[3];


            jsonCamerasSettings = await readFileSettings();
            var d = jsonCamerasSettings.MainCameraName;
            cameras[(int)camera.mainCamera].setCameraSettings(jsonCamerasSettings.MainCameraName);
            cameras[(int)camera.endoCamera].setCameraSettings(jsonCamerasSettings.EndoCameraName);
            cameras[(int)camera.termoCamera].setCameraSettings(jsonCamerasSettings.TermoCameraName);

            //cameras[(int)camera.mainCamera].setCameraSettings("USB Camera");  //RecordexUSA       //rmoncam 8M  //USB Camera
            //cameras[(int)camera.endoCamera].setCameraSettings("HD WEBCAM");
            //cameras[(int)camera.termoCamera].setCameraSettings("PureThermal (fw:v1.0.0)");


            UpdateCaptureControls();
        }

        public MainPage()
        {
           
            this.InitializeComponent();
            Current = this;
            // _previewer = new MediaCapturePreviewer(PreviewControl, Dispatcher);            

            EnumerateHidDevices();

            lenghtMeterTimer = new DispatcherTimer();
            lenghtMeterTimer.Tick += LenghtMeterTimer_Tick;
            lenghtMeterTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            flashDelayTimer = new DispatcherTimer();
            flashDelayTimer.Tick += FlashDelayTimer_Tick;
            flashDelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 400);

            /*serialPortEndo = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
            
            if(serialPortEndo != null)
            {
                serialPortEndo.Open();
                serialPortEndo.DataReceived += SerialPortEndo_DataReceived;
            }*/

            serialPortFlash = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);

            if (serialPortFlash != null)
            {
                serialPortFlash.Open();
                serialPortFlash.ReadTimeout = 500;
                serialPortFlash.WriteTimeout = 500;
                serialPortFlash.DataReceived += SerialPortFlash_DataReceived;
                textBoxInfo.Text += "serialPortFlash open" + Environment.NewLine;
            }


        }

        private void SerialPortFlash_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
        }

        private async void FlashDelayTimer_Tick(object sender, object e)
        {
            Debug.WriteLine("Stop flashDelayTimer");
            flashDelayTimer.Stop();
            await MakePhotoAsync();
        }

        


        private void SerialPortEndo_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            byte[] data = new byte[8];
            serialPort.Read(data, 0, 8);

            short Xaccel = (short)  (  (  (  (short)(data[1]) << 2 | (short)(data[0]) >> 6)) << 6);
            short Yaccel = (short)  (  (  (  (short)(data[3]) << 2 | (short)(data[2]) >> 6)) << 6);
            short Zaccel = (short)  (  (  (  (short)(data[5]) << 2 | (short)(data[4]) >> 6)) << 6);

            double Xf1 = (double)Xaccel / 4096;
            double Yf1 = (double)Yaccel / 4096;
            double Zf1 = (double)Zaccel / 4096;

            double k = 0.2;

            //Xf = Xf1 * k - (1 - k) * Xf;
            //Yf = Yf1 * k - (1 - k) * Yf;
            //Zf = Zf1 * k - (1 - k) * Zf;

            Xf = Xf1;
            Yf = Yf1;
            Zf = Zf1;

        }

        private void LenghtMeterTimer_Tick(object sender, object e)
        {
            getDistance();
            //serialPortEndo.Write("BAAZ");
        }

        HidDevice device;
        int count = 0;

        private async void EnumerateHidDevices()
        {
            // Microsoft Input Configuration Device.
            ushort vendorId  = 0x0483;      // 0x045E;
            ushort productId = 0x5750;      // 0x07CD;
            ushort usagePage = 0xff00;      // 0x000D;
            ushort usageId   = 0x0001;

            // Create the selector.
            string selector = HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);

            // Enumerate devices using the selector.
            var devices = await DeviceInformation.FindAllAsync(selector);

            if (devices.Any())
            {
                // At this point the device is available to communicate with
                // So we can send/receive HID reports from it or 
                // query it for control descriptions.
                
                textBoxInfo.Text += "HID devices found: " + devices.Count + Environment.NewLine;

                // Open the target HID device.
                device = await HidDevice.FromIdAsync(devices.ElementAt(0).Id, FileAccessMode.ReadWrite);

                readHid();

            }
            else
            {
                // There were no HID devices that met the selector criteria.
                textBoxInfo.Text += "HID device not found" + Environment.NewLine;
            }
        }

        private  void sendOutReport(DataWriter dataWriter)
        {
            HidOutputReport outReport = device.CreateOutputReport(0x00);
            outReport.Data = dataWriter.DetachBuffer();

            // Send the output report asynchronously
            device.SendOutputReportAsync(outReport);

            device.InputReportReceived += async (sender, args) =>
            {
                HidInputReport inputReport = args.Report;
                IBuffer buffer = inputReport.Data;
              
                byte[] ByteArray;
                CryptographicBuffer.CopyToByteArray(buffer, out ByteArray);

                await this.Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
                {
                    //textBoxInfo.Text += "\nCount = " + count.ToString() + "\nHID Input Report: " + inputReport.ToString() +
                    //"\nTotal number of bytes received: " + buffer.Length.ToString() +
                    //"\n Data = " + CryptographicBuffer.EncodeToHexString(buffer);

                    count++;

                    if(ByteArray[5] == 28)    // GET_DISTANCE
                    {
                        byte[] bytes = new byte[4];

                        bytes[0] = (byte)(ByteArray[6]);
                        bytes[1] = (byte)(ByteArray[7]);
                        bytes[2] = (byte)(ByteArray[8]);
                        bytes[3] = (byte)(ByteArray[9]);

                        double value = BitConverter.ToSingle(bytes, 0);

                        if (firstDistanceFlag)
                        {
                            firstDistanceFlag = false;
                            initialLenght = value;
                        }
                        lenght = value - initialLenght;

                        videoEffectSettings.lenght = "L = " + lenght.ToString("0.0") + " м";
                        videoEffectSettings.coordinate = "\n x = " + Xf.ToString() + "\n y = " + Yf.ToString() + "\n z = " + Zf.ToString();

                        
                    }

                    if (ByteArray[5] == 25) //AMP_FIND_SUCCESS
                    {
                        lenghtMeterTimer.Start();

                        videoEffectSettings.getLenghtFlag = true;

                        _findLenghtZero = false;


                        UpdateCaptureControls();
                        runMeasure.Visibility = Visibility.Collapsed;
                        progressRing.Visibility = Visibility.Collapsed;
                        progressRing.IsActive = false;
                        textInfo.Visibility = Visibility.Collapsed;

                    }
                    if (ByteArray[5] == 35) //AMP_FIND_NOT_SUCCESS
                    {
                        stopMeasure();
                        lenghtMeterTimer.Stop();

                        firstDistanceFlag = false;
                        _findLenghtZero = false;

                        UpdateCaptureControls();

                        textInfo.Visibility = Visibility.Visible;
                        textInfo.Text = "Калибровка не выполнена";
                    }

                }));
            };
        }
        
        private  void readHid()
        {
            if (device != null)
            {
                // construct a HID output report to send to the device
                

                /// Initialize the data buffer and fill it in
                byte[] bufferTx = new byte[65];
                bufferTx[0] = 0x00;
                bufferTx[1] = 0xaa;
                bufferTx[2] = 0xbb;
                bufferTx[3] = 0x00;
                bufferTx[4] = 0x06;
                bufferTx[5] = 13;

                DataWriter dataWriter = new DataWriter();
                dataWriter.WriteBytes(bufferTx);

                sendOutReport(dataWriter);

            }
        }

        private  void startMeasure()
        {
            if (device != null)
            {

                // construct a HID output report to send to the device
                HidOutputReport outReport = device.CreateOutputReport(0x00);

                /// Initialize the data buffer and fill it in
                byte[] bufferTx = new byte[65];
                bufferTx[0] = 0x00;
                bufferTx[1] = 0xaa;
                bufferTx[2] = 0xbb;
                bufferTx[3] = 0x00;
                bufferTx[4] = 0x06;
                bufferTx[5] = 23;

                DataWriter dataWriter = new DataWriter();
                dataWriter.WriteBytes(bufferTx);

                sendOutReport(dataWriter);

            }
        }

        private  void stopMeasure()
        {
            if (device != null)
            {

                // construct a HID output report to send to the device
                HidOutputReport outReport = device.CreateOutputReport(0x00);

                /// Initialize the data buffer and fill it in
                byte[] bufferTx = new byte[65];
                bufferTx[0] = 0x00;
                bufferTx[1] = 0xaa;
                bufferTx[2] = 0xbb;
                bufferTx[3] = 0x00;
                bufferTx[4] = 0x06;
                bufferTx[5] = 33;

                DataWriter dataWriter = new DataWriter();
                dataWriter.WriteBytes(bufferTx);

                sendOutReport(dataWriter);

            }
        }

        private  void getDistance()
        {
            if (device != null)
            {

                // construct a HID output report to send to the device
                HidOutputReport outReport = device.CreateOutputReport(0x00);

                /// Initialize the data buffer and fill it in
                byte[] bufferTx = new byte[65];
                bufferTx[0] = 0x00;
                bufferTx[1] = 0xaa;
                bufferTx[2] = 0xbb;
                bufferTx[3] = 0x00;
                bufferTx[4] = 0x06;
                bufferTx[5] = 28;

                DataWriter dataWriter = new DataWriter();
                dataWriter.WriteBytes(bufferTx);

                sendOutReport(dataWriter);

            }
        }

        //private void inReportRead_Click(object sender, RoutedEventArgs e)
        //{
        //    readHid();
        //}

        private void runMeasure_Click(object sender, RoutedEventArgs e)
        {
            firstDistanceFlag = true;

            _findLenghtZero = true;
            UpdateCaptureControls();

            startMeasure();
        }

        private void stopMeasure_Click(object sender, RoutedEventArgs e)
        {
            stopMeasure();
            firstDistanceFlag = false;
            //if (cameraType == (int)camera.endoCamera)
                videoEffectSettings.getLenghtFlag = false;
        }


        private void getDistanse_Click(object sender, RoutedEventArgs e)
        {
            //getDistance();
            lenghtMeterTimer.Start();
            if(cameraType == (int)camera.endoCamera)
                videoEffectSettings.getLenghtFlag = true;          
        }

        public static async Task<DeviceInformationCollection> FindCameraDeviceAsync()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return allVideoDevices;
        }




        public void NotifyUser(string strMessage, NotifyType type)
        {
            // If called from the UI thread, then update immediately.
            // Otherwise, schedule a task on the UI thread to perform the update.
            //if (Dispatcher.HasThreadAccess)
            //{
            //    UpdateStatus(strMessage, type);
            //}
            //else
            //{
            //    var task = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () => UpdateStatus(strMessage, type));
            //}
        }

        private void UpdateStatus(string strMessage, NotifyType type)
        {
            //switch (type)
            //{
            //    case NotifyType.StatusMessage:
            //        StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Green);
            //        break;
            //    case NotifyType.ErrorMessage:
            //        StatusBorder.Background = new SolidColorBrush(Windows.UI.Colors.Red);
            //        break;
            //}

            //StatusBlock.Text = strMessage;

        }

        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            //Application.Current.Suspending += Application_Suspending;
            //Application.Current.Resuming += Application_Resuming;
            Window.Current.VisibilityChanged += Window_VisibilityChanged;

            _isActivePage = true;

            await SetUpBasedOnStateAsync();
        }

        private void Application_Resuming(object sender, object e)
        {
            _isSuspending = false;

            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await SetUpBasedOnStateAsync();
            });
        }

        private void Application_Suspending(object sender, SuspendingEventArgs e)
        {
            _isSuspending = true;

            var defferal = e.SuspendingOperation.GetDeferral();
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                await SetUpBasedOnStateAsync();
                defferal.Complete();
            });
        }

        protected override async void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            //Application.Current.Suspending -= Application_Suspending;
            //Application.Current.Resuming -= Application_Resuming;
            Window.Current.VisibilityChanged -= Window_VisibilityChanged;

            _isActivePage = false;

            await SetUpBasedOnStateAsync();
        }

        private async void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs args)
        {
            await SetUpBasedOnStateAsync();
            UpdateCaptureControls();
        }




        public static DeviceInformationCollection cameraDeviceList;
        
        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {
                cameraDeviceList = await FindCameraDeviceAsync();

                if (cameraDeviceList.Count == 0)
                {
                    Debug.WriteLine("No camera device found!");
                    return;
                }                


                string cameraName = cameras[cameraType].Name;
                string cameraId = "";

                foreach (DeviceInformation camera in cameraDeviceList)
                {
                    if (camera.Name == cameraName)
                        cameraId = camera.Id;
                }    

                _mediaCapture = new MediaCapture();
                _mediaCapture.Failed += MediaCaptureFiled;             
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraId };


                try
                {
                    await _mediaCapture.InitializeAsync(settings);

                    //var encodingProperties = (jsonCamerasSettings.MainEncodingProperties as StreamResolution).EncodingProperties;

                    //await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, jsonCamerasSettings.MainEncodingProperties);

                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }

                if (_isInitialized)
                {
                   // await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, jsonCamerasSettings.MainEncodingProperties);
                    await StartPreviewAsync();

                }
               

            }
        }

        private async Task StartPreviewAsync()
        {
            displayRequest.RequestActive();



            //await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, jsonCamerasSettings.MainEncodingProperties);
            PreviewControl.Source = _mediaCapture;


            //------------ add video effect -----------------
            if (_cntCamera == (int)camera.endoCamera)
            {
                var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");

                IMediaExtension videoEffect = await _mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

                videoEffect.SetProperties(new PropertySet() { { "LenghtValue", Lenght } });
            }
            //-----------------------------------------------

            await _mediaCapture.StartPreviewAsync();
            _isPreviewing = true;
           // await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, jsonCamerasSettings.MainEncodingProperties);
        }

        private async Task StopPreviewAsync()
        {
            await _mediaCapture.StopPreviewAsync();
            _isPreviewing = false;
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PreviewControl.Source = null;
                //_displayRequest.RequestRelease();
            });
        }

        private async Task CleanupCameraAsync()
        {
            Debug.WriteLine("CleanupCameraAsync");

            if (_isInitialized)
            {
                if (_isPreviewing)
                {
                    await StopPreviewAsync();
                }

                _isInitialized = false;
            }

            if (_mediaCapture != null)
            {
                _mediaCapture.Failed -= MediaCaptureFiled;
                _mediaCapture.Dispose();
                _mediaCapture = null;
            }
        }

        private async void MediaCaptureFiled(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture_Failed: (0x{0:X}) {1}", errorEventArgs.Code, errorEventArgs.Message);

            await CleanupCameraAsync();
        }


        private async Task SetUpBasedOnStateAsync()
        {

            while (!_setupTask.IsCompleted)
            {
                await _setupTask;
            }

            bool wantUIActive = _isActivePage && Window.Current.Visible && !_isSuspending;

            if (_isUIActive != wantUIActive)
            {
                _isUIActive = wantUIActive;

                Func<Task> setupAsync = async () =>
                {
                    if (wantUIActive)
                    {
                        await SetupUiAsync();
                        await InitializeCameraAsync();
                    }
                    else
                    {
                        await CleanupCameraAsync();
                    }
                };
                _setupTask = setupAsync();
            }

            await _setupTask;
        }


        private async Task SetupUiAsync()
        {
            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            _captureFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;
        }


        private async void PhotoButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("Start flashDelayTimer");
            if (_isFlash)
            {
                flashDelayTimer.Start();
                durationFlashDivider = 1;
                FlashCMD();
            }
            else
                await MakePhotoAsync();
        }



       
        private async Task MakePhotoAsync()
        {
            Debug.WriteLine("Make photo");
            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            storageFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;

            var stream = new InMemoryRandomAccessStream();
            await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            try
            {
                var photofile = await storageFolder.CreateFileAsync("CameraPhoto.jpg", CreationCollisionOption.GenerateUniqueName);
                await SavePhotoAsync(stream, photofile);
                Debug.WriteLine("Photo saved in" + photofile.Path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while making photo " + ex.Message.ToString());
            }
        }

        private async Task SavePhotoAsync(InMemoryRandomAccessStream stream, StorageFile photofile)
        {

            using (var photoStream = stream)
            {
                var decoder = await BitmapDecoder.CreateAsync(photoStream);
                using (var photoFileStream = await photofile.OpenAsync(FileAccessMode.ReadWrite))
                {
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(photoFileStream, decoder);


                    if ((decoder.OrientedPixelWidth == 80) && (decoder.OrientedPixelHeight == 60))
                    {
                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Linear;
                        encoder.BitmapTransform.ScaledHeight = 360;
                        encoder.BitmapTransform.ScaledWidth = 480;

                    }


                    await encoder.FlushAsync();
                }
            }


        }
               
        private async void mainCameraButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SwitchCamera on main");
            _isUIActive = false;
            //_cntCamera = 0;
            cameraType = (int)camera.mainCamera;
            await CleanupCameraAsync();
           
            await SetUpBasedOnStateAsync();

            stopMeasure();
            firstDistanceFlag = false;
            _findLenghtZero = false;
            videoEffectSettings.getLenghtFlag = false;


            UpdateCaptureControls();
        }

        private async void endoCameraButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SwitchCamera on endo");
            _isUIActive = false;
            _cntCamera = 1;
            cameraType = (int)camera.endoCamera;

            await CleanupCameraAsync();
            await SetUpBasedOnStateAsync();

            videoEffectSettings.getLenghtFlag = true;

            runMeasure.Visibility = Visibility.Visible;

            UpdateCaptureControls();
        }

        private async void termoCameraButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SwitchCamera on termo");
            _isUIActive = false;
            _cntCamera = 2;
            cameraType = (int)camera.termoCamera;
            await CleanupCameraAsync();
            await SetUpBasedOnStateAsync();

            stopMeasure();
            firstDistanceFlag = false;
            _findLenghtZero = false;
            videoEffectSettings.getLenghtFlag = false;

            UpdateCaptureControls();
        }

        private async void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                _isRecording = true;
                UpdateCaptureControls();

                await StartRecordingAsync(); 
            }
            else
            {
                _isRecording = false;
                UpdateCaptureControls();

                await StopRecordingAsync();
            }
            
        }

        private async Task StartRecordingAsync()
        {
            Debug.WriteLine("StartRecord");

            var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            try
            {
                if (cameraType == (int)camera.termoCamera)
                    _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Wvga), file);
                else
                    _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), file);
                await _mediaRecording.StartAsync();
                //_isRecording = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("err3 = " + e.ToString());
            }
        }

        private async Task StopRecordingAsync()
        {
            Debug.WriteLine("Stopping recording...");

            _isRecording = false;
            await _mediaCapture.StopRecordAsync();

            Debug.WriteLine("Stopped recording!");
        }


        private void UpdateCaptureControls()
        {
            // diagnostic information
            textBoxInfo.Visibility = Visibility.Collapsed;

            //_getDistanse.Visibility = Visibility.Collapsed;
            //_stopMeasure.Visibility = Visibility.Collapsed;


            // The buttons should only be enabled if the preview started sucessfully
            PhotoButton.IsEnabled = _isPreviewing;
            VideoButton.IsEnabled = _isPreviewing;

            // Update recording button to show "Stop" icon instead of red "Record" icon
            StartRecordingIcon.Visibility = _isRecording ? Visibility.Collapsed : Visibility.Visible;
            StopRecordingIcon.Visibility  = _isRecording ? Visibility.Visible   : Visibility.Collapsed;
            Rec.Visibility                = _isRecording ? Visibility.Visible   : Visibility.Collapsed;

            // Update flash button
            NotFlashIcon.Visibility     = _isFlash ? Visibility.Collapsed : Visibility.Visible;
            FlashIcon.Visibility        = _isFlash ? Visibility.Visible   : Visibility.Collapsed;
            plusFlashButton.Visibility  = _isFlash ? Visibility.Visible   : Visibility.Collapsed;
            minusFlashButton.Visibility = _isFlash ? Visibility.Visible   : Visibility.Collapsed;
            pbFlashPower.Visibility     = _isFlash ? Visibility.Visible   : Visibility.Collapsed;

            // Update lenght meter controls            
            runMeasure.Visibility = (cameraType == (int)camera.endoCamera) ? Visibility.Visible : Visibility.Collapsed;
            
            if(_findLenghtZero)
            {
                progressRing.Visibility = Visibility.Visible;
                progressRing.IsActive = true;
                textInfo.Text = "Подождите, идет калибровка";
                textInfo.Visibility = Visibility.Visible;                
            }
            else
            {
                progressRing.Visibility = Visibility.Collapsed;
                progressRing.IsActive = false;
                textInfo.Visibility = Visibility.Collapsed;
            }


            // If the camera doesn't support simultaneosly taking pictures and recording video, disable the photo button on record
            if (_isInitialized && !_mediaCapture.MediaCaptureSettings.ConcurrentRecordAndPhotoSupported)
            {
                PhotoButton.IsEnabled = !_isRecording;

                // Make the button invisible if it's disabled, so it's obvious it cannot be interacted with
                PhotoButton.Opacity = PhotoButton.IsEnabled ? 1 : 0;
            }


            Windows.UI.Color color;
            color.A = 51;
            color.B = 0;
            color.G = 0;
            color.R = 0;
            

            switch (cameraType)
            {
                case (int)camera.mainCamera:
                    mainCameraButton.Background.Opacity = 10;
                    mainCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
                    
                    endoCameraButton.Background = new SolidColorBrush(color);
                    termoCameraButton.Background = new SolidColorBrush(color);
                    break;
                case (int)camera.endoCamera:
                    mainCameraButton.Background = new SolidColorBrush(color);
                    endoCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
                    termoCameraButton.Background = new SolidColorBrush(color);
                    break;
                case (int)camera.termoCamera:
                    mainCameraButton.Background = new SolidColorBrush(color);
                    endoCameraButton.Background = new SolidColorBrush(color);
                    termoCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
                    break;
            }


            //videoEffectSettings.X = cameras[_cntCamera].X;
            //videoEffectSettings.Y = cameras[_cntCamera].Y;
            //videoEffectSettings.FontSize = cameras[_cntCamera].FontSize;

            videoEffectSettings.X = 750;
            videoEffectSettings.Y = 600;
            videoEffectSettings.FontSize = 90;

           

        }

        private void getScenarioSettings_Click(object sender, RoutedEventArgs e)
        {            

            this.Frame.Navigate(typeof(SettingsPage));

        }


        private void buttonFlash_Click(object sender, RoutedEventArgs e)
        {
            _isFlash = !_isFlash;
            UpdateCaptureControls();
        }


        private void FlashCMD()
        {
            Debug.WriteLine("Send flash");
            textBoxInfo.Text += "Send flash" + Environment.NewLine;

            byte[] data = new byte[1];
            byte[] flashByte = new byte[2];

            flashByte[0] = (byte)(  (int)(1199 - pbFlashPower.Value) & 0x00FF);
            flashByte[1] = (byte)(  (int)(1199 - pbFlashPower.Value) >> 8);
            try
            {
                data[0] = 0x55;
                serialPortFlash.Write(data, 0, 1);
                data[0] = 0x53;
                serialPortFlash.Write(data, 0, 1);
                data[0] = flashByte[1];
                serialPortFlash.Write(data, 0, 1);
                data[0] = flashByte[0];
                serialPortFlash.Write(data, 0, 1);
                data[0] = (byte)(FlashDuration / durationFlashDivider);
                serialPortFlash.Write(data, 0, 1);
            }
            catch (Exception ex)
            {
                textBoxInfo.Text += "er1 " + ex.Message.ToString() + Environment.NewLine;
            }
        }


        //short flashValue = 600;
        short FlashDuration = 0x12;
        short durationFlashDivider = 1;
        short StepFlashPower = 100;


        //private void flashSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
        //{
        //    Slider slider = sender as Slider;
        //    if (slider != null)
        //    {
        //        flashValue = (short)slider.Value;
        //    }         
        //}

        private void minusFlashButton_Click(object sender, RoutedEventArgs e)
        {
            pbFlashPower.Value = pbFlashPower.Value - StepFlashPower;
            durationFlashDivider = 3;
            FlashCMD();
        }

        private void plusFlashButton_Click(object sender, RoutedEventArgs e)
        {
            pbFlashPower.Value = pbFlashPower.Value + StepFlashPower;
            durationFlashDivider = 3;
            FlashCMD();
        }
    }


}
