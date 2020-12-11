﻿using System;
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
        private MediaCapture _mediaCaptureDouble1;
        private MediaCapture _mediaCaptureDouble2;
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private StorageFolder storageFolder = null;
        LowLagMediaRecording _mediaRecording;
        string Lenght;
        double initialLenght = 0;
        double lenght = 0;
        double X;
        double Y;
        double Z;
        double Xf;
        double Yf;
        double Zf;
        DispatcherTimer lenghtMeterTimer;
        DispatcherTimer flashDelayTimer;
        DispatcherTimer refreshCameraTimer;
        SerialPort serialPortFlash;        
        bool _isFlash = true;
        bool _isPause = false;
        bool _isFlashEndo = true;
        bool firstDistanceFlag = true;
       
        bool _isMainCameraFlag;
        bool _isEndoCameraFlag;
        bool _isTermoCameraFlag;
        bool _isFail;

        private bool _isInitialized;
        private bool _isPreviewing;
        private bool _isActivePage;
        private bool _isSuspending;
        private bool _isRecording;
        private bool _findLenghtZero = false;
        private bool _isDouble;
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
        public enum cameraType 
        {
            mainCamera, 
            endoCamera, 
            termoCamera, 
            doubleCamera
        }

        public struct Camera
        {
            public StreamResolution PreviewResolution, PhotoResolution, VideoResolution;
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

            public int Width {  get { return width; }   set { width = value; }  }
            public int Height { get { return height; }  set { height = value; } }
            public int X { get { return x; } set { x = value; } }
            public int Y { get { return y; } set { y = value; } }

            public int FontSize { get { return fontSize; } set { fontSize = value; } }

        }

        public static Camera[] cameras;
        JsonCamerasSettings jsonCamerasSettings;
        public int currentCameraType; //= (int)cameraType.mainCamera;                            



        private async Task<JsonCamerasSettings> readFileSettings()
        {
            JsonCamerasSettings jsonCamerasSettings = await JsonCamerasSettings.readFileSettings();
            return jsonCamerasSettings;
        }

        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            cameras = new Camera[3];

           
            //cameras[(int)camera.mainCamera].setCameraSettings("RecordexUSA");  //RecordexUSA       //rmoncam 8M  //USB Camera
            //cameras[(int)camera.endoCamera].setCameraSettings("HD WEBCAM");
            //cameras[(int)camera.termoCamera].setCameraSettings("PureThermal (fw:v1.0.0)");

            jsonCamerasSettings = await readFileSettings();
            cameras[(int)cameraType.mainCamera].setCameraSettings(jsonCamerasSettings.MainCameraName);
            cameras[(int)cameraType.endoCamera].setCameraSettings(jsonCamerasSettings.EndoCameraName);
            cameras[(int)cameraType.termoCamera].setCameraSettings(jsonCamerasSettings.TermoCameraName);

            cameraDeviceListOld = await FindCameraDeviceAsync();
            _isEndoCameraFlag = cameraDeviceListOld.Count == 2 ? false : true;
            

            if (cameraDeviceListOld.Count == 0)
            {
                Debug.WriteLine("No camera device found!");
                return;
            }         

            foreach (DeviceInformation camera in cameraDeviceListOld)
            {
                if (camera.Name == cameras[(int)cameraType.mainCamera].Name)
                {
                    cameras[(int)cameraType.mainCamera].Id = camera.Id;
                    _isMainCameraFlag = true;
                }
                if (camera.Name == cameras[(int)cameraType.endoCamera].Name)
                {
                    cameras[(int)cameraType.endoCamera].Id = camera.Id;
                    _isEndoCameraFlag = true;
                }
                if (camera.Name == cameras[(int)cameraType.termoCamera].Name)
                {
                    cameras[(int)cameraType.termoCamera].Id = camera.Id;
                    _isTermoCameraFlag = true;
                }
            }

            

            if(_isMainCameraFlag)
                currentCameraType = (int)cameraType.mainCamera;
            else if(_isEndoCameraFlag)
                currentCameraType = (int)cameraType.endoCamera;
            else
                currentCameraType = (int)cameraType.termoCamera;

            await SetUpBasedOnStateAsync();
            UpdateUIControls();
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

            refreshCameraTimer = new DispatcherTimer();
            refreshCameraTimer.Tick += refreshCameraTimer_Tick;
            refreshCameraTimer.Interval = new TimeSpan(0, 0, 0, 0, 2000);
            refreshCameraTimer.Start();

            /*serialPortEndo = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
            
            if(serialPortEndo != null)
            {
                serialPortEndo.Open();
                serialPortEndo.DataReceived += SerialPortEndo_DataReceived;
            }*/

            serialPortFlash = new SerialPort("COM3", 9600, Parity.None, 8, StopBits.One);

            if (serialPortFlash != null)
            {
                try
                {
                    serialPortFlash.Open();
                    serialPortFlash.ReadTimeout = 500;
                    serialPortFlash.WriteTimeout = 500;
                    serialPortFlash.DataReceived += SerialPortFlash_DataReceived;
                    textBoxInfo.Text += "serialPortFlash open" + Environment.NewLine;
                }
                catch(Exception e)
                {
                    Debug.WriteLine(e.Message);
                    textBoxInfo.Text = e.Message;
                }
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

        private async void refreshCameraTimer_Tick(object sender, object e)
        {
            cameraDeviceList = await FindCameraDeviceAsync();

            if (cameraDeviceList.Count != cameraDeviceListOld.Count)
            {
                //_isEndoCameraFlag = cameraDeviceList.Count > cameraDeviceListOld.Count ? true : false;

                cameraDeviceListOld = cameraDeviceList;
                _isMainCameraFlag  = false;
                _isEndoCameraFlag  = false;
                _isTermoCameraFlag = false;

                foreach (DeviceInformation camera in cameraDeviceList)
                {
                    if (camera.Name == cameras[(int)cameraType.mainCamera].Name)
                    {
                        cameras[(int)cameraType.mainCamera].Id = camera.Id;
                        _isMainCameraFlag = true;
                    }
                    if (camera.Name == cameras[(int)cameraType.endoCamera].Name)
                    {
                        cameras[(int)cameraType.endoCamera].Id = camera.Id;
                        _isEndoCameraFlag = true;
                    }
                    if (camera.Name == cameras[(int)cameraType.termoCamera].Name)
                    {
                        cameras[(int)cameraType.termoCamera].Id = camera.Id;
                        _isTermoCameraFlag = true;
                    }
                }
                
                UpdateUIControls();
            }

            if (_isFail)
            {
                _isFail = false;
                _isUIActive = false;

                currentCameraType = (++currentCameraType) % 3;

                _isDouble = false;

                await SetUpBasedOnStateAsync();

                stopMeasure();
                firstDistanceFlag = false;
                _findLenghtZero = false;
                videoEffectSettings.getLenghtFlag = false;


                UpdateUIControls();
            }

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
            getAccel();
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
                
                textBoxInfo.Text += "\nHID devices found: " + devices.Count + Environment.NewLine;

                // Open the target HID device.
                device = await HidDevice.FromIdAsync(devices.ElementAt(0).Id, FileAccessMode.ReadWrite);

                readHid();

            }
            else
            {
                // There were no HID devices that met the selector criteria.
                textBoxInfo.Text += "\nHID device not found" + Environment.NewLine;
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

                    if (ByteArray[5] == 25)     // AMP_FIND_SUCCESS
                    {
                        lenghtMeterTimer.Start();

                        videoEffectSettings.getLenghtFlag = true;

                        _findLenghtZero = false;


                        UpdateUIControls();
                        runMeasure.Visibility = Visibility.Collapsed;
                        progressRing.Visibility = Visibility.Collapsed;
                        progressRing.IsActive = false;
                        textInfo.Visibility = Visibility.Collapsed;

                    }
                    if (ByteArray[5] == 35)  // AMP_FIND_NOT_SUCCESS
                    {
                        stopMeasure();
                        lenghtMeterTimer.Stop();

                        firstDistanceFlag = false;
                        _findLenghtZero = false;

                        UpdateUIControls();

                        textInfo.Visibility = Visibility.Visible;
                        textInfo.Text = "Калибровка не выполнена";
                    }

                    if (ByteArray[5] == 32)     // MSG_GET_ACCEL_DATA
                    {
                        //byte[] data = new byte[8];
                        

                        short Xaccel = (short)((((short)(ByteArray[7]) << 2  | (short)(ByteArray[6])  >> 6)) << 6);
                        short Yaccel = (short)((((short)(ByteArray[9]) << 2  | (short)(ByteArray[8])  >> 6)) << 6);
                        short Zaccel = (short)((((short)(ByteArray[11]) << 2 | (short)(ByteArray[10]) >> 6)) << 6);

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

                        textInfo.Visibility = Visibility.Visible;
                        textBoxInfo.Text = "Данные акселерометра\n";
                        textBoxInfo.Text = Xf.ToString() + "  " + Yf.ToString() + "  " + Zf.ToString() + "\n";
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
                bufferTx[5] = 23;       // start measure

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
                bufferTx[5] = 33;    // stop measure

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
                bufferTx[5] = 28;       // get distance

                DataWriter dataWriter = new DataWriter();
                dataWriter.WriteBytes(bufferTx);

                sendOutReport(dataWriter);

            }
        }

        private void getAccel()
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
                bufferTx[5] = 32;       // get accel data

                DataWriter dataWriter = new DataWriter();
                dataWriter.WriteBytes(bufferTx);

                sendOutReport(dataWriter);

            }
        }

        int light = 250;
        private void setFlashingLight(int light)
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
                bufferTx[4] = 0x08;
                bufferTx[5] = 31;       // set flash light

                bufferTx[6] = (byte)(light >> 8);
                bufferTx[7] = (byte)(light & 0xff);

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
            UpdateUIControls();

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
            if(currentCameraType == (int)cameraType.endoCamera)
                videoEffectSettings.getLenghtFlag = true;          
        }

        public static async Task<DeviceInformationCollection> FindCameraDeviceAsync()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return allVideoDevices;
        }



        //-----------------------------------------------------------------------
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
                //await SetUpBasedOnStateAsync();
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
            //jsonCamerasSettings = await readFileSettings();
            //cameras[(int)cameraType.mainCamera].setCameraSettings(jsonCamerasSettings.MainCameraName);
            //cameras[(int)cameraType.endoCamera].setCameraSettings(jsonCamerasSettings.EndoCameraName);
            //cameras[(int)cameraType.termoCamera].setCameraSettings(jsonCamerasSettings.TermoCameraName);

            //cameraDeviceList = await FindCameraDeviceAsync();

            //if (cameraDeviceList.Count == 0)
            //{
            //    Debug.WriteLine("No camera device found!");
            //    return;
            //}

            //int cameraCount = 3;
            //for (int i = 0; i < cameraCount; i++)
            //{
            //    foreach (DeviceInformation camera in cameraDeviceList)
            //    {
            //        if (camera.Name == cameras[i].Name)
            //            cameras[i].Id = camera.Id;
            //    }
            //}


            //await SetUpBasedOnStateAsync();
            //UpdateCaptureControls();
        }
        //-----------------------------------------------------------------------



        public static DeviceInformationCollection cameraDeviceList;
        public static DeviceInformationCollection cameraDeviceListOld;
        //StreamResolution MainPreview, MainPhoto, MainVideo;


        private async Task DefineCameraResolutionAsync()
        {
            Debug.WriteLine("CameraResolutionAsync");
            IEnumerable<StreamResolution> allStreamProperties;

            if (_mediaCapture == null)
            {
                cameraDeviceList = await FindCameraDeviceAsync();

                if (cameraDeviceList.Count == 0)
                {
                    Debug.WriteLine("No camera device found!");
                    return;
                }


                foreach (DeviceInformation camera in cameraDeviceList)
                {

                    _mediaCapture = new MediaCapture();
                    _mediaCapture.Failed += MediaCaptureFiled;
                    var settings = new MediaCaptureInitializationSettings { VideoDeviceId = camera.Id };

                    try
                    {
                        await _mediaCapture.InitializeAsync(settings);

                        allStreamProperties = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
                       // allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

                        foreach (var property in allStreamProperties)
                        {
                            if (jsonCamerasSettings.MainCameraPreview == property.GetFriendlyName(true))
                                cameras[(int)cameraType.mainCamera].PreviewResolution = new StreamResolution(property.EncodingProperties);
                            if (jsonCamerasSettings.MainCameraPhoto == property.GetFriendlyName(true))
                                cameras[(int)cameraType.mainCamera].PhotoResolution = new StreamResolution(property.EncodingProperties);
                            if (jsonCamerasSettings.MainCameraVideo == property.GetFriendlyName(true))
                                cameras[(int)cameraType.mainCamera].VideoResolution = new StreamResolution(property.EncodingProperties);

                        }
                        //_isInitialized = true;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        Debug.WriteLine("The app was denied access to the camera");
                    }

                }
                            
            }
        }


        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {
                //cameraDeviceList = await FindCameraDeviceAsync();

                //if (cameraDeviceList.Count == 0)
                //{
                //    Debug.WriteLine("No camera device found!");
                //    return;
                //}                


                //string cameraName = cameras[currentCameraType].Name;
                //string cameraId = "";

                //// Нужно единожды найти и задать id камер
                //foreach (DeviceInformation camera in cameraDeviceList)
                //{
                //    if (camera.Name == cameraName)
                //        cameraId = camera.Id;
                //}    

                _mediaCapture = new MediaCapture();
                _mediaCapture.Failed += MediaCaptureFiled;             
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameras[currentCameraType].Id };


                try
                {
                    await _mediaCapture.InitializeAsync(settings);

                    IEnumerable<StreamResolution>  allStreamProperties = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
                    allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

                    foreach (var property in allStreamProperties)
                    {
                        if (jsonCamerasSettings.MainCameraPreview == property.GetFriendlyName(true))
                            cameras[(int)cameraType.mainCamera].PreviewResolution = new StreamResolution(property.EncodingProperties);
                        if (jsonCamerasSettings.MainCameraPhoto == property.GetFriendlyName(true))
                            cameras[(int)cameraType.mainCamera].PhotoResolution = new StreamResolution(property.EncodingProperties);
                        if (jsonCamerasSettings.MainCameraVideo == property.GetFriendlyName(true))
                            cameras[(int)cameraType.mainCamera].VideoResolution = new StreamResolution(property.EncodingProperties);

                    }

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

        private async Task InitializeDoubleCameraAsync()
        {
            Debug.WriteLine("InitializeDoubleCameraAsync");

            if (_mediaCaptureDouble1 == null && _mediaCaptureDouble2 == null)
            {
                cameraDeviceList = await FindCameraDeviceAsync();

                if (cameraDeviceList.Count == 0)
                {
                    Debug.WriteLine("No camera device found!");
                    return;
                }


                string cameraName1 = cameras[0].Name;
                string cameraName2 = cameras[2].Name;
                string cameraId1 = "";
                string cameraId2 = "";

                foreach (DeviceInformation camera in cameraDeviceList)
                {
                    if (camera.Name == cameraName1)
                        cameraId1 = camera.Id;
                    if (camera.Name == cameraName2)
                        cameraId2 = camera.Id;
                }

                _mediaCaptureDouble1 = new MediaCapture();
                _mediaCaptureDouble1.Failed += MediaCaptureFiled;
                var settings1 = new MediaCaptureInitializationSettings { VideoDeviceId = cameraId1 };
                _mediaCaptureDouble2 = new MediaCapture();
                _mediaCaptureDouble2.Failed += MediaCaptureFiled;
                var settings2 = new MediaCaptureInitializationSettings { VideoDeviceId = cameraId2 };


                try
                {
                    await _mediaCaptureDouble1.InitializeAsync(settings1);
                    await _mediaCaptureDouble2.InitializeAsync(settings2);


                    _isInitialized = true;
                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("The app was denied access to the camera");
                }

                if (_isInitialized)
                {
                    
                    await StartPreviewDoubleAsync();

                }


            }
        }

        private async Task StartPreviewAsync()
        {
            displayRequest.RequestActive();


            PreviewControl.Source = _mediaCapture;


            //------------ add video effect -----------------
            if (currentCameraType == (int)cameraType.endoCamera || currentCameraType == (int)cameraType.mainCamera)
            {
                var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");

                IMediaExtension videoEffect = await _mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

                videoEffect.SetProperties(new PropertySet() { { "LenghtValue", Lenght } });
            }
            //-----------------------------------------------

            try
            {
                await _mediaCapture.StartPreviewAsync();
                _isPreviewing = true;
            }
            catch(Exception e)
            {
                Debug.WriteLine("error preview " + e.Message);
            }

        }

        private async Task StartPreviewDoubleAsync()
        {
            displayRequest.RequestActive();
          
            PreviewControlDouble1.Source = _mediaCaptureDouble1;
            PreviewControlDouble2.Source = _mediaCaptureDouble2;

            await _mediaCaptureDouble1.StartPreviewAsync();
            await _mediaCaptureDouble2.StartPreviewAsync();
            _isPreviewing = true;
            
        }
        
        private async Task StopPreviewAsync()
        {
            try
            {
                await _mediaCapture.StopPreviewAsync();
            }
            catch(Exception e)
            {
                Debug.WriteLine("error stop preview" + e.Message);
                _isFail = true;
            }
            _isPreviewing = false;
            
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                PreviewControl.Source = null;
                //_displayRequest.RequestRelease();
            });
        }

        private async Task StopPreviewDoubleAsync()
        {
            await _mediaCaptureDouble1.StopPreviewAsync();
            await _mediaCaptureDouble2.StopPreviewAsync();
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
                    if (_isDouble)
                        await StopPreviewDoubleAsync();
                    else
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
            if (_mediaCaptureDouble1 != null)
            {
                _mediaCaptureDouble1.Failed -= MediaCaptureFiled;
                _mediaCaptureDouble1.Dispose();
                _mediaCaptureDouble1 = null;
            }
            if (_mediaCaptureDouble2 != null)
            {
                _mediaCaptureDouble2.Failed -= MediaCaptureFiled;
                _mediaCaptureDouble2.Dispose();
                _mediaCaptureDouble2 = null;
            }            
            
        }

        private async void MediaCaptureFiled(MediaCapture sender, MediaCaptureFailedEventArgs errorEventArgs)
        {
            Debug.WriteLine("MediaCapture_Failed: (0x{0:X}) {1}", errorEventArgs.Code, errorEventArgs.Message);

            try
            {
                await CleanupCameraAsync();
            }
            catch(Exception e)
            {
                Debug.WriteLine("error cleanupCamera" + e.Message);
            }
        }


        private async Task SetUpBasedOnStateAsync()
        {

            while (!_setupTask.IsCompleted)
            {
                await _setupTask;
            }

            bool wantUIActive = _isActivePage && Window.Current.Visible && !_isSuspending;    // 

            if (_isUIActive != wantUIActive)
            {
                _isUIActive = wantUIActive;

                Func<Task> setupAsync = async () =>
                {
                    if (wantUIActive)
                    {
                        await SetupUiAsync();

                        if (_isDouble)
                            await InitializeDoubleCameraAsync();
                        else
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
            stopMeasure();
            firstDistanceFlag = false;
            _findLenghtZero = false;
            videoEffectSettings.getLenghtFlag = false;

            Debug.WriteLine("SwitchCamera on main");
            _isUIActive = false;

            currentCameraType = (int)cameraType.mainCamera;
            try
            {
                await CleanupCameraAsync();

                _isDouble = false;

                await SetUpBasedOnStateAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error main camera button" + ex.Message);
            }

            UpdateUIControls();
        }

        private async void endoCameraButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SwitchCamera on endo");
            _isUIActive = false;

            currentCameraType = (int)cameraType.endoCamera;

            try
            {               
                await CleanupCameraAsync();

                _isDouble = false;

                await SetUpBasedOnStateAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error endo camera button" + ex.Message);
            }
            videoEffectSettings.getLenghtFlag = true;

            runMeasure.Visibility = Visibility.Visible;

            UpdateUIControls();
        }

        private async void termoCameraButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SwitchCamera on termo");
            _isUIActive = false;

            currentCameraType = (int)cameraType.termoCamera;

            try
            {
                await CleanupCameraAsync();

                _isDouble = false;

                await SetUpBasedOnStateAsync();
            }
            catch(Exception ex)
            {
                Debug.WriteLine("error termo camera button" + ex.Message);
            }

            stopMeasure();
            firstDistanceFlag = false;
            _findLenghtZero = false;
            videoEffectSettings.getLenghtFlag = false;

            UpdateUIControls();
        }

        private async void doubleCameraButton_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine("SwitchCamera on double");
            _isUIActive = false;

            currentCameraType = (int)cameraType.doubleCamera;
            try
            {
                await CleanupCameraAsync();

                _isDouble = true;

                await SetUpBasedOnStateAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine("error termo camera button" + ex.Message);
            }
            stopMeasure();
            firstDistanceFlag = false;
            _findLenghtZero = false;
            videoEffectSettings.getLenghtFlag = false;


            UpdateUIControls();           

        }

        private async void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            _isPause = false;
            if (!_isRecording)
            {
                _isRecording = true;
                UpdateUIControls();

                await StartRecordingAsync(); 
            }
            else
            {
                _isRecording = false;
                UpdateUIControls();

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
                if (currentCameraType == (int)cameraType.termoCamera)
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


        private void UpdateUIControls()
        {
            PauseVideoButton.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;
            PauseIcon.Visibility  = _isPause ? Visibility.Collapsed : Visibility.Visible;
            ResumeIcon.Visibility = _isPause ? Visibility.Visible : Visibility.Collapsed;

            mainCameraButton.Visibility = (_isMainCameraFlag) ? Visibility.Visible : Visibility.Collapsed;
            endoCameraButton.Visibility = (_isEndoCameraFlag) ? Visibility.Visible : Visibility.Collapsed;
            termoCameraButton.Visibility = (_isTermoCameraFlag) ? Visibility.Visible : Visibility.Collapsed;
            doubleCameraButton.Visibility = (_isMainCameraFlag && _isTermoCameraFlag) ? Visibility.Visible : Visibility.Collapsed;

            // diagnostic information
            textBoxInfo.Visibility = Visibility.Visible;

            //_getDistanse.Visibility = Visibility.Collapsed;
            //_stopMeasure.Visibility = Visibility.Collapsed;


            // The buttons should only be enabled if the preview started sucessfully
            PhotoButton.IsEnabled = _isPreviewing;
            VideoButton.IsEnabled = _isPreviewing;

            // Update recording button to show "Stop" icon instead of red "Record" icon
            StartRecordingIcon.Visibility = _isRecording ? Visibility.Collapsed : Visibility.Visible;
            StopRecordingIcon.Visibility  = _isRecording ? Visibility.Visible   : Visibility.Collapsed;
            Rec.Visibility                = _isRecording && !_isPause ? Visibility.Visible   : Visibility.Collapsed;

            // Update flash button
            NotFlashIcon.Visibility     = _isFlash ? Visibility.Collapsed : Visibility.Visible;
            FlashIcon.Visibility        = _isFlash ? Visibility.Visible   : Visibility.Collapsed;
            plusFlashButton.Visibility  = _isFlash && (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible   : Visibility.Collapsed;
            minusFlashButton.Visibility = _isFlash && (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible   : Visibility.Collapsed;
            pbFlashPower.Visibility     = _isFlash && (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible   : Visibility.Collapsed;
            buttonFlash.Visibility      = (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible   : Visibility.Collapsed;

            // Update flash button Endo
            NotFlashIconEndo.Visibility      = _isFlashEndo ? Visibility.Collapsed : Visibility.Visible;
            FlashIconEndo.Visibility         = _isFlashEndo ? Visibility.Visible   : Visibility.Collapsed;
            plusFlashButtonEndo.Visibility   = _isFlashEndo && (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Visible : Visibility.Collapsed;
            minusFlashButtonEndo.Visibility  = _isFlashEndo && (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Visible : Visibility.Collapsed;
            pbFlashPowerEndo.Visibility      = _isFlashEndo && (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Visible : Visibility.Collapsed;
            buttonFlashEndo.Visibility       = (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Visible : Visibility.Collapsed;





            // Update lenght meter controls            
            runMeasure.Visibility = (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Visible : Visibility.Collapsed;
            
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
            //if (_isInitialized && !_mediaCapture.MediaCaptureSettings.ConcurrentRecordAndPhotoSupported)
            //{
            //    PhotoButton.IsEnabled = !_isRecording;

            //    // Make the button invisible if it's disabled, so it's obvious it cannot be interacted with
            //    PhotoButton.Opacity = PhotoButton.IsEnabled ? 1 : 0;
            //}


            Windows.UI.Color color;
            color.A = 51;
            color.B = 0;
            color.G = 0;
            color.R = 0;
            

            switch (currentCameraType)
            {
                case (int)cameraType.mainCamera:
                    //mainCameraButton.Background.Opacity = 10;
                    mainCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);                   
                    endoCameraButton.Background = new SolidColorBrush(color);
                    termoCameraButton.Background = new SolidColorBrush(color);
                    doubleCameraButton.Background = new SolidColorBrush(color);
                    break;
                case (int)cameraType.endoCamera:
                    mainCameraButton.Background = new SolidColorBrush(color);
                    endoCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
                    termoCameraButton.Background = new SolidColorBrush(color);
                    doubleCameraButton.Background = new SolidColorBrush(color);
                    break;
                case (int)cameraType.termoCamera:
                    mainCameraButton.Background = new SolidColorBrush(color);
                    endoCameraButton.Background = new SolidColorBrush(color);
                    termoCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
                    doubleCameraButton.Background = new SolidColorBrush(color);
                    break;
                case (int)cameraType.doubleCamera:
                    mainCameraButton.Background = new SolidColorBrush(color);
                    endoCameraButton.Background = new SolidColorBrush(color);
                    termoCameraButton.Background = new SolidColorBrush(color);
                    doubleCameraButton.Background = new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);
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
            UpdateUIControls();
        }
        private void buttonFlashEndo_Click(object sender, RoutedEventArgs e)
        {
            _isFlashEndo = !_isFlashEndo;
            UpdateUIControls();
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

        private void FlashCMDEndo()
        {
            Debug.WriteLine("Send flash Endo");
            textBoxInfo.Text += "Send flash Endo" + Environment.NewLine;            

            int litgh = (int)(pbFlashPowerEndo.Value);
            
            try
            {
                setFlashingLight(litgh);
            }
            catch (Exception ex)
            {
                textBoxInfo.Text += "error flashCMDEndo " + ex.Message.ToString() + Environment.NewLine;
            }
        }

        //short flashValue = 600;
        short FlashDuration = 0x12;
        short durationFlashDivider = 1;
        short StepFlashPower = 100;
        

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

        private void plusFlashButtonEndo_Click(object sender, RoutedEventArgs e)
        {
            pbFlashPowerEndo.Value = pbFlashPowerEndo.Value + StepFlashPower;
            FlashCMDEndo();
        }

        private void minusFlashButtonEndo_Click(object sender, RoutedEventArgs e)
        {
            pbFlashPowerEndo.Value = pbFlashPowerEndo.Value - StepFlashPower;
            FlashCMDEndo();
        }        

        //private async void PreviewSettings_Changed(object sender, SelectionChangedEventArgs e)
        //{
        //    if (_isPreviewing)
        //    {
        //        var selectedItem = (sender as ComboBox).SelectedItem as ComboBoxItem;
        //        var encodingProperties = (selectedItem.Tag as StreamResolution).EncodingProperties;
        //        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);
        //    }
        //}

        public async Task SetMediaStreamPropertiesAsync(MediaStreamType streamType, IMediaEncodingProperties encodingProperties)
        {
            // Stop preview and unlink the CaptureElement from the MediaCapture object
            await _mediaCapture.StopPreviewAsync();
            PreviewControl.Source = null;

            // Apply desired stream properties
            await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, encodingProperties);

            // Recreate the CaptureElement pipeline and restart the preview
            PreviewControl.Source = _mediaCapture;
            await _mediaCapture.StartPreviewAsync();
        }

        private async void ReadSettings_Click(object sender, RoutedEventArgs e)
        {
            //await SetMediaStreamPropertiesAsync(MediaStreamType.VideoPreview, cameras[(int)cameraType.mainCamera].PreviewResolution.EncodingProperties);
            //await SetMediaStreamPropertiesAsync(MediaStreamType.Photo, cameras[(int)cameraType.mainCamera].PhotoResolution.EncodingProperties);
            //await SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, cameras[(int)cameraType.mainCamera].VideoResolution.EncodingProperties);

            getAccel();

            setFlashingLight(light);
        }

        private async void PauseVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _isPause = !_isPause;
            UpdateUIControls();

            if(_isPause)
                await _mediaRecording.PauseAsync(Windows.Media.Devices.MediaCapturePauseBehavior.ReleaseHardwareResources);
            else
                await _mediaRecording.ResumeAsync();
        }

        static int temp1 = 0;
        private void NotesButton_Click(object sender, RoutedEventArgs e)
        {
            videoEffectSettings.commet = "Заметка" + ++temp1;
        }


    }


}
