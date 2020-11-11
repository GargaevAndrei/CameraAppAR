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




// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace CameraCOT
{


    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
            
        private MediaCapture _mediaCapture;        
        bool isInitialized = false;
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private StorageFolder storageFolder = null;
        LowLagMediaRecording _mediaRecording;
        string Lenght;
        DispatcherTimer dispatcherTimer;
        SerialPort serialPortEndo;
        double Xf;
        double Yf;
        double Zf;

        private bool _isInitialized;
        private bool _isPreviewing;
        private bool _isActivePage;
        private bool _isSuspending;
        private bool _isRecording;
        private readonly DisplayRequest _displayRequest = new DisplayRequest();
        private Task _setupTask = Task.CompletedTask;
        private bool _isUIActive;
        public int _cntCamera;  //was private
        private bool _changeCamera;
        private bool _isSwitched;
        private StorageFolder _captureFolder = null;        


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

            public void setCameraSettings(string _Name, string _Id)
            {
                this.Name = _Name;
                this.Id = _Id;                
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
        int cameraType;// = (int)camera.mainCamera;


        //private MediaFrameReader mediaFrameReader;
        //StorageFile fileVideo;
        //private SoftwareBitmap backBuffer;
        //private bool taskRunning = false;
        //CanvasBitmap canvasBitmap;
        //MediaComposition mediaComposition;
        //private MediaCapture mediaCaptureNote;

        private async Task<JsonCamerasSettings> readFileSettings()
        {
            JsonCamerasSettings jsonCamerasSettings = await JsonCamerasSettings.readFileSettings();
            return jsonCamerasSettings;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {            

            jsonCamerasSettings = await readFileSettings();
            var d = jsonCamerasSettings.MainCameraName;

            cameras = new Camera[3];
            cameras[(int)camera.mainCamera].setCameraSettings(jsonCamerasSettings.MainCameraName,   jsonCamerasSettings.MainCameraId);
            cameras[(int)camera.endoCamera].setCameraSettings(jsonCamerasSettings.EndoCameraName,   jsonCamerasSettings.EndoCameraId);
            cameras[(int)camera.termoCamera].setCameraSettings(jsonCamerasSettings.TermoCameraName, jsonCamerasSettings.TermoCameraId);

            UpdateCaptureControls();
        }

        public MainPage()
        {

            this.InitializeComponent();
            EnumerateHidDevices();

            /*dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            serialPortEndo = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
            
            if(serialPortEndo != null)
            {
                serialPortEndo.Open();
                serialPortEndo.DataReceived += SerialPortEndo_DataReceived;
            }*/

            
            
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

        private void dispatcherTimer_Tick(object sender, object e)
        {
            getDistance();
            serialPortEndo.Write("BAAZ");
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
                textBoxInfo.Text = "HID devices found: " + devices.Count;

                // Open the target HID device.
                device = await HidDevice.FromIdAsync(devices.ElementAt(0).Id, FileAccessMode.ReadWrite);

                readHid();

            }
            else
            {
                // There were no HID devices that met the selector criteria.
                textBoxInfo.Text = "HID device not found";
            }
        }

        private async void sendOutReport(DataWriter dataWriter)
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

                    if(ByteArray[5] == 0x1c)
                    {
                        byte[] bytes = new byte[4];

                        bytes[0] = (byte)(ByteArray[6]);
                        bytes[1] = (byte)(ByteArray[7]);
                        bytes[2] = (byte)(ByteArray[8]);
                        bytes[3] = (byte)(ByteArray[9]);
                        //Array.Reverse(bytes);
                        double value = BitConverter.ToSingle(bytes, 0);

                        //textBoxInfo.Text += "\n Lenght = " + value.ToString();
                        videoEffectSettings.lenght = "\n Lenght = " + value.ToString("0.000");
                        videoEffectSettings.coordinate = "\n x = " + Xf.ToString() + "\n y = " + Yf.ToString() + "\n z = " + Zf.ToString();
                         
                    }


                }));
            };
        }
        
        private async void readHid()
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

        private async void startMeasure()
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

        private async void stopMeasure()
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

        private async void getDistance()
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

        private void inReportRead_Click(object sender, RoutedEventArgs e)
        {
            readHid();
        }

        private void runMeasure_Click(object sender, RoutedEventArgs e)
        {
            startMeasure();
        }

        private void stopMeasure_Click(object sender, RoutedEventArgs e)
        {
            stopMeasure();
        }

        private void getDistanse_Click(object sender, RoutedEventArgs e)
        {
            //getDistance();
            dispatcherTimer.Start();
        }

        public static async Task<DeviceInformationCollection> FindCameraDeviceAsync()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return allVideoDevices;
        }


        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            Application.Current.Suspending += Application_Suspending;
            Application.Current.Resuming += Application_Resuming;
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
            Application.Current.Suspending -= Application_Suspending;
            Application.Current.Resuming -= Application_Resuming;
            Window.Current.VisibilityChanged -= Window_VisibilityChanged;

            _isActivePage = false;

            await SetUpBasedOnStateAsync();
        }

        private async void Window_VisibilityChanged(object sender, VisibilityChangedEventArgs args)
        {
            await SetUpBasedOnStateAsync();
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

                //var cameraDevice = cameraDeviceList.FirstOrDefault();

                /*if (_changeCamera)
                {
                    _changeCamera = false;
                    _cntCamera++;
                }

                if (cameraDeviceList.Count < (_cntCamera + 1))
                    _cntCamera = 0;
                */

                //------------ можно закоментировать и обращаться прямо к cameras[cameraType].Id !!!
                //------------------------ есть подводные камни: идет привязка к конкретному usb !!!
                string cameraName = cameras[cameraType].Name;
                string cameraId = "";

                //var cameraDevice = cameraDeviceList[_cntCamera];

                foreach (DeviceInformation camera in cameraDeviceList)
                {
                    if (camera.Name == cameraName)
                        cameraId = camera.Id;
                }    
                //-----------------------------------------------------------------------------------

                _mediaCapture = new MediaCapture();
                _mediaCapture.Failed += MediaCaptureFiled;

                
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraId };           //cameraDevice.Id

                try
                {
                    await _mediaCapture.InitializeAsync(settings);
                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }

                if (_isInitialized)
                {
                    await StartPreviewAsync();
                }

            }
        }

        private async Task StartPreviewAsync()
        {
            displayRequest.RequestActive();
            PreviewControl.Source = _mediaCapture;

           // var encodingProperties = StreamResolution.EncodingProperties;
           // await _previewer.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);


            //------------ add video effect -----------------
            if (_cntCamera != (int)camera.termoCamera)
            { 
                var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");

                IMediaExtension videoEffect =
                   await _mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

                //videoEffect.SetProperties(new PropertySet() { { "FadeValue", 0.1 } });
                //videoEffect.SetProperties(new PropertySet() { { "LenghtValue", Lenght } });
            }
            //-----------------------------------------------
            //var encodingProperties = (jsonCamerasSettings.MainCameraVideo as StreamResolution).EncodingProperties;
           // IMediaEncodingProperties mediaEncodingProperties;
           // mediaEncodingProperties.
           // await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, jsonCamerasSettings.MediaEncodingProperties);
            await _mediaCapture.StartPreviewAsync();
            _isPreviewing = true;
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


        private async void SwitchCam_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SwitchCameraClick");
            _isUIActive = false;
            _changeCamera = true;

            await CleanupCameraAsync();
            await SetUpBasedOnStateAsync();

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

           
            //mainCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.BlueViolet);

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

            UpdateCaptureControls();
        }

        private async void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isRecording)
            {
                await StartRecordingAsync(); 
            }
            else
            {
                await StopRecordingAsync();
            }

            // After starting or stopping video recording, update the UI to reflect the MediaCapture state
            UpdateCaptureControls();
        }

        private async Task StartRecordingAsync()
        {
            Debug.WriteLine("StartRecord");

            var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            try
            {
                _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), file);
                await _mediaRecording.StartAsync();
                _isRecording = true;
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
            // The buttons should only be enabled if the preview started sucessfully
            PhotoButton.IsEnabled = _isPreviewing;
            VideoButton.IsEnabled = _isPreviewing;

            // Update recording button to show "Stop" icon instead of red "Record" icon
            StartRecordingIcon.Visibility = _isRecording ? Visibility.Collapsed : Visibility.Visible;
            StopRecordingIcon.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;

            // If the camera doesn't support simultaneosly taking pictures and recording video, disable the photo button on record
            if (_isInitialized && !_mediaCapture.MediaCaptureSettings.ConcurrentRecordAndPhotoSupported)
            {
                PhotoButton.IsEnabled = !_isRecording;

                // Make the button invisible if it's disabled, so it's obvious it cannot be interacted with
                PhotoButton.Opacity = PhotoButton.IsEnabled ? 1 : 0;
            }


            switch(_cntCamera)
            {
                case 0: 
                    mainCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.Thistle);
                    endoCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                    break;
                case 1:
                    mainCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.Transparent);
                    endoCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.Thistle);
                    break;
                case 2:
                    mainCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
                    endoCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
                    break;
            }


            videoEffectSettings.X = cameras[_cntCamera].X;
            videoEffectSettings.Y = cameras[_cntCamera].Y;
            videoEffectSettings.FontSize = cameras[_cntCamera].FontSize;


        }

        private void getScenarioSettings_Click(object sender, RoutedEventArgs e)
        {            

            this.Frame.Navigate(typeof(SettingsPage));

        }
       
    }


}
