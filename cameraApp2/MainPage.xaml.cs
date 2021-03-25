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
using System.IO.MemoryMappedFiles;
using System.IO;
using Windows.Networking.Sockets;
using Windows.Media.Capture.Frames;
using System.Threading;
using System.Net.WebSockets;
using System.Net;
using Windows.Networking;
using Windows.UI.Xaml.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Drawing;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Microsoft.Graphics.Canvas.Text;
using Windows.UI;




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


        MediaFrameReader mediaFrameReader;

        private MediaCapture _mediaCapture;
        private MediaCapture _mediaCaptureDouble1;
        private MediaCapture _mediaCaptureDouble2;
        private readonly DisplayRequest displayRequest = new DisplayRequest();

        private StorageFolder storageFolder = null;
        private StorageFolder naparnikFolder = null;
        private StorageFolder photoFolder = null;
        private StorageFolder videoFolder = null;
        private StorageFile configFile = null;
        private StorageFile notesFile = null;

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
        DispatcherTimer serialCommandDelayTimer;
        DispatcherTimer refreshCameraTimer;
        DispatcherTimer histogramStatisticTimer;
        DispatcherTimer opacityTimer;
        DispatcherTimer recordTimer;
        DispatcherTimer recordTimerPause;

        SerialPort serialPortFlash;
        SerialPort serialPortLepton;

        bool _isMicrophone = false;
        bool _isVoice = false;
        bool _isFlash = true;
        bool _isPause = false;
        bool _isFlashEndo = true;
        bool firstDistanceFlag = true;

        bool _isMainCameraFlag;
        bool _isEndoCameraFlag;
        bool _isTermoCameraFlag;
        bool _isFail;
        bool _isNotes;
        bool _isNotFirstStart;
        bool _isHelpCommands;

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

        string strMinT, strMaxT, strPointT, strTemp;

        double minT, maxT, pointT;

        List<string> stringNote = new List<string>(new string[] { "" });
        int indexNoteShow;


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
            public StreamResolution PhotoResolution, VideoResolution;
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

            public int Width { get { return width; } set { width = value; } }
            public int Height { get { return height; } set { height = value; } }
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
            //StartingTextBox.Visibility = Visibility.Collapsed;
            //progressRing.Visibility = Visibility.Collapsed;
            //progressRing.IsActive = false;


            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            storageFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;

            naparnikFolder = (StorageFolder)await storageFolder.TryGetItemAsync("Напарник");
            if (naparnikFolder == null)
            {
                try
                {
                    naparnikFolder = await storageFolder.CreateFolderAsync("Напарник", CreationCollisionOption.FailIfExists);
                    photoFolder = await naparnikFolder.CreateFolderAsync("Фото", CreationCollisionOption.FailIfExists);
                    videoFolder = await naparnikFolder.CreateFolderAsync("Видео", CreationCollisionOption.FailIfExists);
                    notesFile = await naparnikFolder.CreateFileAsync("Заметки.txt");
                    //configFile = await naparnikFolder.CreateFileAsync("cameraConfig.json");
                }
                catch (Exception ex)
                {
                    textBoxInfo.Text += ex.Message;
                }
            }
            else
            {
                photoFolder = (StorageFolder)await naparnikFolder.TryGetItemAsync("Фото");
                if (photoFolder == null)
                {
                    try
                    {
                        photoFolder = await naparnikFolder.CreateFolderAsync("Фото", CreationCollisionOption.FailIfExists);
                    }
                    catch (Exception ex)
                    {
                        textBoxInfo.Text += ex.Message;
                    }
                }
                videoFolder = (StorageFolder)await naparnikFolder.TryGetItemAsync("Видео");
                if (videoFolder == null)
                {
                    try
                    {
                        videoFolder = await naparnikFolder.CreateFolderAsync("Видео", CreationCollisionOption.FailIfExists);
                    }
                    catch (Exception ex)
                    {
                        textBoxInfo.Text += ex.Message;
                    }
                }

                notesFile = (StorageFile)await naparnikFolder.TryGetItemAsync("Заметки.txt");
                if (notesFile == null)
                    notesFile = await naparnikFolder.CreateFileAsync("Заметки.txt");
                else
                {
                    IList<string> data = await FileIO.ReadLinesAsync(notesFile);
                    stringNote = data.ToList();
                }
            }



            cameras = new Camera[3];

            //  RecordexUSA       //rmoncam 8M  //USB Camera //"USB Camera2


            //load settings
            configFile = (StorageFile)await naparnikFolder.TryGetItemAsync("cameraConfig.json");
            if (configFile == null)
            {
                configFile = await naparnikFolder.CreateFileAsync("cameraConfig.json");


                jsonCamerasSettings = new JsonCamerasSettings();

                jsonCamerasSettings.MainCameraName = "USB Camera2";
                jsonCamerasSettings.MainCameraPhoto = "3264x2448 [1,33] 15FPS NV12";
                jsonCamerasSettings.MainCameraVideo = "1600x1200 [1,33] 30FPS NV12";

                jsonCamerasSettings.EndoCameraName = "HD WEBCAM";
                jsonCamerasSettings.EndoCameraPhoto = "1600x1200 [1,33] 30FPS NV12";
                jsonCamerasSettings.EndoCameraVideo = "1600x1200 [1,33] 30FPS NV12";

                jsonCamerasSettings.TermoCameraName = "PureThermal (fw:v1.0.0)";
                jsonCamerasSettings.TermoCameraPhoto = "80x60 [1,33] 9FPS {59565955-0000-0010-8000-00AA00389B71}";
                jsonCamerasSettings.TermoCameraVideo = "80x60 [1,33] 9FPS {59565955-0000-0010-8000-00AA00389B71}";

                JsonSerializer serializer = new JsonSerializer();
                serializer.NullValueHandling = NullValueHandling.Include;
                serializer.TypeNameHandling = TypeNameHandling.Auto;
                serializer.Formatting = Formatting.Indented;

                string temp = "{\n" +
                          "\"MainCameraName\": \"USB Camera2\",\n" +
                          "\"MainCameraPhoto\": \"3264x2448 [1,33] 15FPS NV12\",\n" +
                          "\"MainCameraVideo\": \"1600x1200 [1,33] 30FPS NV12\",\n" +
                          "\"EndoCameraName\": \"HD WEBCAM\",\n" +
                          "\"EndoCameraPhoto\": \"1600x1200 [1,33] 30FPS NV12\",\n" +
                          "\"EndoCameraVideo\": \"1600x1200 [1,33] 30FPS NV12\",\n" +
                          "\"TermoCameraName\": \"PureThermal (fw:v1.0.0)\",\n" +
                          "\"TermoCameraPhoto\": \"80x60 [1,33] 9FPS {59565955-0000-0010-8000-00AA00389B71}\",\n" +
                          "\"TermoCameraVideo\": \"80x60 [1,33] 9FPS {59565955-0000-0010-8000-00AA00389B71}\"\n" +
                            "}\n";


                await FileIO.AppendTextAsync(configFile, temp);

                /*using (StreamWriter sw = new StreamWriter(naparnikFolder.Path))
                using (JsonWriter writer = new Newtonsoft.Json.JsonTextWriter(sw))
                {
                    serializer.Serialize(writer, jsonCamerasSettings, typeof(JsonCamerasSettings));
                }*/

            }
            else
            {
                jsonCamerasSettings = await readFileSettings();
            }


            cameras[(int)cameraType.mainCamera].setCameraSettings(jsonCamerasSettings.MainCameraName);
            cameras[(int)cameraType.endoCamera].setCameraSettings(jsonCamerasSettings.EndoCameraName);
            cameras[(int)cameraType.termoCamera].setCameraSettings(jsonCamerasSettings.TermoCameraName);

            cameraDeviceListOld = await FindCameraDeviceAsync();
            refreshCameraTimer.Start();

            if (cameraDeviceListOld.Count == 0)
            {
                Debug.WriteLine("No camera device found!");
                return;
            }

            CameraDefineLogic();


            await DefineCameraResolutionAsync();

            await SetUpBasedOnStateAsync();
            UpdateUIControls();
            _isNotFirstStart = true;


            SetCoordinateNotes(150, (int)cameras[(int)cameraType.mainCamera].VideoResolution.Height - 200, 34, 1250);

            progressRingStart.Visibility = Visibility.Collapsed;
            progressRingStart.IsActive = false;
        }

        public void SetCoordinateNotes(int _x, int _y, int _fontSize, int _widthRect)
        {
            videoEffectSettings.XRect = _x;
            videoEffectSettings.YRect = _y;
            videoEffectSettings.FontSize = _fontSize;
            videoEffectSettings.widthRect = _widthRect;

        }

        public MainPage()
        {
            
            this.InitializeComponent();
            Current = this;

            EnumerateHidDevices();

            lenghtMeterTimer = new DispatcherTimer();
            lenghtMeterTimer.Tick += LenghtMeterTimer_Tick;
            lenghtMeterTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            flashDelayTimer = new DispatcherTimer();
            flashDelayTimer.Tick += FlashDelayTimer_Tick;
            flashDelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 2000);

            serialCommandDelayTimer = new DispatcherTimer();
            serialCommandDelayTimer.Tick += serialCommandDelayTimer_Tick;
            serialCommandDelayTimer.Interval = new TimeSpan(0, 0, 0, 0, 200);

            refreshCameraTimer = new DispatcherTimer();
            refreshCameraTimer.Tick += refreshCameraTimer_Tick;
            refreshCameraTimer.Interval = new TimeSpan(0, 0, 0, 0, 2000);


            histogramStatisticTimer = new DispatcherTimer();
            histogramStatisticTimer.Tick += histogramStatisticTimer_Tick;
            histogramStatisticTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            //histogramStatisticTimer.Start();

            opacityTimer = new DispatcherTimer();
            opacityTimer.Tick += opacityTimer_Tick;
            opacityTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            recordTimer = new DispatcherTimer();
            recordTimer.Tick += recordTimer_Tick;
            recordTimer.Interval = new TimeSpan(0, 0, 0, 0, 100);

            recordTimerPause = new DispatcherTimer();
            recordTimerPause.Tick += recordTimerPause_Tick;
            recordTimerPause.Interval = new TimeSpan(0, 0, 0, 0, 100);


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
                    serialPortFlash.ReadTimeout = 3000;
                    serialPortFlash.WriteTimeout = 3000;
                    serialPortFlash.DataReceived += SerialPortFlash_DataReceived;
                    textBoxInfo.Text += "serialPortFlash open" + Environment.NewLine;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    //textBoxInfo.Text = e.Message;
                }
            }

            serialPortLepton = new SerialPort("COM7", 115200, Parity.None, 8, StopBits.One);    //COM2  // COM7

            if (serialPortLepton != null)
            {
                try
                {
                    serialPortLepton.Open();
                    serialPortLepton.ReadTimeout = 2000;
                    serialPortLepton.WriteTimeout = 2000;

                    if (serialPortLepton.IsOpen)
                        serialPortLepton.DataReceived += SerialPortLepton_DataReceived;

                    textBoxInfo.Text += "serialPortLepton open" + Environment.NewLine;
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                    textBoxInfo.Text += e.Message;
                }
            }


        }


        public void OutputsData()
        {
            this.textBoxTmax.Text = maxT.ToString("0.0");
            this.textBoxTmin.Text = minT.ToString("0.0");
            textBoxTpoint.Text = videoEffectSettings.temperature.ToString("0.0");
            //this.textBoxInfo.Text += strTemp + "\n";

            //serialPortLepton.DiscardInBuffer();
            //histogramStatisticTimer.Start();
        }

        public async Task ParseTemperature(string response)
        {
            try
            {
                var index1 = response.IndexOf(":");
                var index2 = response.LastIndexOf(":");
                strMinT = response.Substring(0, index1);
                strMaxT = response.Substring(index1 + 1, index2 - index1 - 1);

                strPointT = videoEffectSettings.temperature.ToString("0.0");

                minT = Convert.ToDouble(strMinT);
                maxT = Convert.ToDouble(strMaxT);

                videoEffectSettings.Tmax = maxT;
                videoEffectSettings.Tmin = minT;

                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => OutputsData());

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                textBoxInfo.Text += ex.Message;
            }

            serialPortLepton.DiscardInBuffer();
            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => histogramStatisticTimer.Start());
        }

        private async Task RunVoiceCommand(string response)
        {
            strTemp = response;

            try
            {
                string[] substr = strTemp.Split(' ');

                switch (substr[1])
                {
                    case "1": if (_isMainCameraFlag)
                                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => mainCameraButton_Click(null, null)); break;
                    case "2": if (_isEndoCameraFlag)
                                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => endoCameraButton_Click(null, null)); break;
                    case "3": if (_isTermoCameraFlag)
                                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => termoCameraButton_Click(null, null)); break;
                    case "4": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => PhotoButton_Click(null, null)); break;
                    case "5": if (!_isRecording && currentCameraType != (int)cameraType.doubleCamera) 
                                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => VideoButton_Click(null, null)); break;
                    case "6": if (_isRecording && currentCameraType != (int)cameraType.doubleCamera)
                                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => VideoButton_Click(null, null)); break;
                    case "7": if (_isRecording && currentCameraType != (int)cameraType.doubleCamera)
                                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => PauseVideoButton_Click(null, null)); break;
                    case "8":  await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => plusFlashButton_Click(null, null)); break;
                    case "9":  await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => minusFlashButton_Click(null, null)); break;
                    case "10": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => NotesButton_Click(null, null)); break;
                    case "11": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => imageControlPreview_Tapped(null, null)); break;
                    case "12": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => OutputHelpCommand()); break;
                    case "13": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => buttonFlash_Click(null, null)); break;
                    case "14": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => doubleCameraButton_Click(null, null)); break;
                    case "15": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => PhotoButton_Click(null, null)); break;
                    case "16": await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => doubleCameraButton_Click(null, null)); break;
                }

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                textBoxInfo.Text += ex.Message;
            }
        }

        private async void SerialPortLepton_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            try
            {
                var response = serialPort.ReadLine();

                if (response.IndexOf("on") >= 0)
                {
                    _isMicrophone = true;
                    return;
                }
                if (response.IndexOf("off") >= 0)
                {
                    _isMicrophone = false;
                    return;
                }
                if (response.IndexOf("voice") >= 0)
                {
                    await RunVoiceCommand(response);
                    return;
                }
                if (response.IndexOf("temperature") >= 0)
                {
                    await ParseTemperature(response);
                    return;
                }
                


            }
            catch (Exception ex)
            {
                textBoxInfo.Text += ex.Message;
            }

            //await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => histogramStatisticTimer.Start());


        }

        private void recordTimerPause_Tick(object sender, object e)
        {
            //DateTime currentTime = DateTime.Now;
            //deltaTimePause = currentTime - timePause;
        }

        //TimeSpan deltaTimePause;
        //DateTime timePause;
        //DateTime timeStartRecord;
        Stopwatch stopWatch = new Stopwatch();
        private void recordTimer_Tick(object sender, object e)
        {
            //DateTime currentTime = DateTime.Now;            
            //TimeSpan deltaTime = currentTime - timeStartRecord;
            //deltaTime -= deltaTimePause; 
            //recordTimeTextBox.Text = deltaTime.ToString(@"hh\:mm\:ss");

            recordTimeTextBox.Text = stopWatch.Elapsed.ToString(@"hh\:mm\:ss");
        }

        private void opacityTimer_Tick(object sender, object e)
        {
            if (_isDouble)
            {
                PreviewControlDouble1.Opacity = 1;
                PreviewControlDouble2.Opacity = 1;
            }
            else
                PreviewControl.Opacity = 1;

            opacityTimer.Stop();
        }


        static int index;

        private void histogramStatisticTimer_Tick(object sender, object e)
        {
            //textBoxInfo.Text += ++index + "\n";
            //StartClient();
            try
            {
                serialPortLepton.WriteLine("request");
            }
            catch (Exception ex)
            {
                textBoxInfo.Text += ex.Message;
            }

            if(currentCameraType != (int)cameraType.termoCamera && currentCameraType != (int)cameraType.doubleCamera)
                histogramStatisticTimer.Stop();
        }


        private async void serialCommandDelayTimer_Tick(object sender, object e)
        {
            serialCommandDelayTimer.Stop();
            if (serialCommand == 49)
            {
                if (currentCameraType == (int)cameraType.mainCamera)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => plusFlashButton_Click(null, null));
                if (currentCameraType == (int)cameraType.endoCamera)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => plusFlashButtonEndo_Click(null, null));
            }
            if (serialCommand == 50)
            {
                if (currentCameraType == (int)cameraType.mainCamera)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => minusFlashButton_Click(null, null));
                if (currentCameraType == (int)cameraType.endoCamera)
                    await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => minusFlashButtonEndo_Click(null, null));
            }
            if (serialCommand == 52)
            {
                await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => PhotoButton_Click(null, null));
            }
        }

        int serialCommand = 0;
        private void SerialPortFlash_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            //if(serialCommandDelayTimer.IsEnabled)
            //    serialCommandDelayTimer.Start();


            try
            {
                byte[] data = new byte[2];
                serialPortFlash.Read(data, 0, 2);

                if (data[0] == 65)
                {
                    serialCommand = data[1];
                    Dispatcher.RunAsync(CoreDispatcherPriority.High, () => serialCommandDelayTimer.Start());
                }


                /*if (data[1] == 49)
                {                                       
                    Dispatcher.RunAsync(CoreDispatcherPriority.High, () => plusFlashButton_Click(null, null));
                }
                if(data[1] == 50)
                {
                    Dispatcher.RunAsync(CoreDispatcherPriority.High, () => minusFlashButton_Click(null, null));
                }*/
            }
            catch (Exception ex)
            {
                //textBoxInfo.Text += ex.Message;
                Debug.WriteLine(ex.Message);
            }


        }

        private async void FlashDelayTimer_Tick(object sender, object e)
        {
            Debug.WriteLine("Stop flashDelayTimer");
            flashDelayTimer.Stop();

            if (currentCameraType == (int)cameraType.doubleCamera)
                await MakePhotoDoubleAsync();
            else
            {
                await MakePhotoAsync();
                await SwitchLowResolution();
            }
        }

        private async void refreshCameraTimer_Tick(object sender, object e)
        {
            cameraDeviceList = await FindCameraDeviceAsync();

            if (cameraDeviceList.Count != cameraDeviceListOld.Count)
            {

                await CleanupCameraAsync();

                cameraDeviceListOld = cameraDeviceList;
                _isMainCameraFlag = false;
                _isEndoCameraFlag = false;
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


                _isUIActive = false;
                CameraDefineLogic();

                await DefineCameraResolutionAsync();
                await SetUpBasedOnStateAsync();

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
                //videoEffectSettings.termo = true;

                UpdateUIControls();
            }

        }

        private void CameraDefineLogic()
        {
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



            if (_isEndoCameraFlag)
                currentCameraType = (int)cameraType.endoCamera;
            else if (_isMainCameraFlag)
                currentCameraType = (int)cameraType.mainCamera;
            else
                currentCameraType = (int)cameraType.termoCamera;
        }

        private void SerialPortEndo_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            byte[] data = new byte[8];
            serialPort.Read(data, 0, 8);

            short Xaccel = (short)((((short)(data[1]) << 2 | (short)(data[0]) >> 6)) << 6);
            short Yaccel = (short)((((short)(data[3]) << 2 | (short)(data[2]) >> 6)) << 6);
            short Zaccel = (short)((((short)(data[5]) << 2 | (short)(data[4]) >> 6)) << 6);

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
            ushort vendorId = 0x0483;      // 0x045E;
            ushort productId = 0x5750;      // 0x07CD;
            ushort usagePage = 0xff00;      // 0x000D;
            ushort usageId = 0x0001;

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

        private void sendOutReport(DataWriter dataWriter)
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

                    if (ByteArray[5] == 28)    // GET_DISTANCE
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


                        short Xaccel = (short)((((short)(ByteArray[7]) << 2 | (short)(ByteArray[6]) >> 6)) << 6);
                        short Yaccel = (short)((((short)(ByteArray[9]) << 2 | (short)(ByteArray[8]) >> 6)) << 6);
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

        private void readHid()
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

        private void startMeasure()
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

        private void stopMeasure()
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

        private void getDistance()
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
            if (currentCameraType == (int)cameraType.endoCamera)
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
            //Window.Current.Closed += Window_Closed;

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
            histogramStatisticTimer.Stop();

            _isSuspending = true;

            var defferal = e.SuspendingOperation.GetDeferral();
            var task = Dispatcher.RunAsync(CoreDispatcherPriority.High, async () =>
            {
                //await SetUpBasedOnStateAsync();
                defferal.Complete();
            });
            //defferal.Complete();

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

            if (_isNotFirstStart)
                await SetUpBasedOnStateAsync();
            //UpdateCaptureControls();
        }

        //private void Window_Closed(object sender, VisibilityChangedEventArgs args)
        //{
        //    streamSocket.Dispose();
        //}
        //-----------------------------------------------------------------------



        public static DeviceInformationCollection cameraDeviceList;
        public static DeviceInformationCollection cameraDeviceListOld;


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

                    if (camera.Name == cameras[(int)cameraType.mainCamera].Name)
                    {
                        var settings = new MediaCaptureInitializationSettings { VideoDeviceId = camera.Id };

                        try
                        {
                            await _mediaCapture.InitializeAsync(settings);

                            allStreamProperties = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
                            // allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

                            foreach (var property in allStreamProperties)
                            {
                                if (jsonCamerasSettings.MainCameraPhoto == property.GetFriendlyName(true))
                                    cameras[(int)cameraType.mainCamera].PhotoResolution = new StreamResolution(property.EncodingProperties);
                                if (jsonCamerasSettings.MainCameraVideo == property.GetFriendlyName(true))
                                    cameras[(int)cameraType.mainCamera].VideoResolution = new StreamResolution(property.EncodingProperties);

                            }
                            if (_mediaCapture != null)
                            {
                                _mediaCapture.Failed -= MediaCaptureFiled;
                                _mediaCapture.Dispose();
                                _mediaCapture = null;
                            }
                            //_isInitialized = true;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Debug.WriteLine("The app was denied access to the camera");
                        }
                    }

                    if (camera.Name == cameras[(int)cameraType.endoCamera].Name)
                    {
                        var settings = new MediaCaptureInitializationSettings { VideoDeviceId = camera.Id };

                        try
                        {
                            await _mediaCapture.InitializeAsync(settings);

                            allStreamProperties = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
                            // allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

                            foreach (var property in allStreamProperties)
                            {
                                if (jsonCamerasSettings.EndoCameraPhoto == property.GetFriendlyName(true))
                                    cameras[(int)cameraType.endoCamera].PhotoResolution = new StreamResolution(property.EncodingProperties);
                                if (jsonCamerasSettings.EndoCameraVideo == property.GetFriendlyName(true))
                                    cameras[(int)cameraType.endoCamera].VideoResolution = new StreamResolution(property.EncodingProperties);

                            }
                            if (_mediaCapture != null)
                            {
                                _mediaCapture.Failed -= MediaCaptureFiled;
                                _mediaCapture.Dispose();
                                _mediaCapture = null;
                            }
                            //_isInitialized = true;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Debug.WriteLine("The app was denied access to the camera");
                        }
                    }

                    if (camera.Name == cameras[(int)cameraType.termoCamera].Name)
                    {
                        var settings = new MediaCaptureInitializationSettings { VideoDeviceId = camera.Id };

                        try
                        {
                            await _mediaCapture.InitializeAsync(settings);

                            allStreamProperties = _mediaCapture.VideoDeviceController.GetAvailableMediaStreamProperties(MediaStreamType.VideoPreview).Select(x => new StreamResolution(x));
                            // allStreamProperties = allStreamProperties.OrderByDescending(x => x.Height * x.Width).ThenByDescending(x => x.FrameRate);

                            foreach (var property in allStreamProperties)
                            {
                                if (jsonCamerasSettings.TermoCameraPhoto == property.GetFriendlyName(true))
                                    cameras[(int)cameraType.termoCamera].PhotoResolution = new StreamResolution(property.EncodingProperties);
                                if (jsonCamerasSettings.TermoCameraVideo == property.GetFriendlyName(true))
                                    cameras[(int)cameraType.termoCamera].VideoResolution = new StreamResolution(property.EncodingProperties);

                            }
                            if (_mediaCapture != null)
                            {
                                _mediaCapture.Failed -= MediaCaptureFiled;
                                _mediaCapture.Dispose();
                                _mediaCapture = null;
                            }
                            //_isInitialized = true;
                        }
                        catch (UnauthorizedAccessException)
                        {
                            Debug.WriteLine("The app was denied access to the camera");
                        }
                    }


                }

                await CleanupCameraAsync();


            }
        }


        private async Task InitializeCameraAsync()
        {
            Debug.WriteLine("InitializeCameraAsync");

            if (_mediaCapture == null)
            {

                _mediaCapture = new MediaCapture();
                _mediaCapture.Failed += MediaCaptureFiled;
                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameras[currentCameraType].Id };

                try
                {
                    await _mediaCapture.InitializeAsync(settings);

                    if (currentCameraType == (int)cameraType.mainCamera)
                        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, cameras[(int)cameraType.mainCamera].VideoResolution.EncodingProperties);

                    if (currentCameraType == (int)cameraType.endoCamera)
                        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, cameras[(int)cameraType.endoCamera].VideoResolution.EncodingProperties);

                    if (currentCameraType == (int)cameraType.termoCamera)
                        await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, cameras[(int)cameraType.termoCamera].VideoResolution.EncodingProperties);

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


        #region frame reader --------------------------
        //----------------------------------------------------
        private SoftwareBitmap backBuffer;
        private byte[] imageBuffer;


        private async void GetFrame_Click()
        {

            var frameSourceGroups = await MediaFrameSourceGroup.FindAllAsync();

            MediaFrameSourceGroup selectedGroup = null;
            MediaFrameSourceInfo colorSourceInfo = null;

            foreach (var sourceGroup in frameSourceGroups)
            {
                foreach (var sourceInfo in sourceGroup.SourceInfos)
                {
                    if (sourceInfo.MediaStreamType == MediaStreamType.VideoRecord && sourceInfo.SourceKind == MediaFrameSourceKind.Color)
                    {
                        colorSourceInfo = sourceInfo;
                        break;
                    }
                }
                if (colorSourceInfo != null)
                {
                    selectedGroup = sourceGroup;
                    break;
                }
            }



            if (selectedGroup == null)
            {
                return;
            }

            _mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings()
            {
                SourceGroup = selectedGroup,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };
            try
            {
                await _mediaCapture.InitializeAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MediaCapture initialization failed: " + ex.Message);
                return;
            }
;

            var colorFrameSourse = _mediaCapture.FrameSources[colorSourceInfo.Id];
            var preferredFormat = colorFrameSourse.SupportedFormats.Where(format =>
            {
                return format.VideoFormat.Width <= 800;               //&& format.Subtype == MediaEncodingSubtypes.Argb32
            }).FirstOrDefault();
            if (preferredFormat == null)
            {
                Debug.WriteLine("designed format not supported");
                return;
            }

            await colorFrameSourse.SetFormatAsync(preferredFormat);

            mediaFrameReader = await _mediaCapture.CreateFrameReaderAsync(colorFrameSourse, MediaEncodingSubtypes.Argb32);
            mediaFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            await mediaFrameReader.StartAsync();

        }

        private void ColorFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var mediaFrameReference = sender.TryAcquireLatestFrame();
            var videoMediaFrame = mediaFrameReference?.VideoMediaFrame;
            var softwareBitmap = videoMediaFrame?.SoftwareBitmap;


            if (softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != Windows.Graphics.Imaging.BitmapPixelFormat.Bgra8 ||
                    softwareBitmap.BitmapAlphaMode != Windows.Graphics.Imaging.BitmapAlphaMode.Premultiplied)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }

                // Swap the processed frame to _backBuffer and dispose of the unused image.
                softwareBitmap = Interlocked.Exchange(ref backBuffer, softwareBitmap);

                imageBuffer = new byte[backBuffer.PixelWidth * backBuffer.PixelHeight * 4];
                backBuffer.CopyToBuffer(imageBuffer.AsBuffer());


                softwareBitmap?.Dispose();

            }

            //mediaFrameReference.Dispose();
        }


        //----------------------------------------------------
        #endregion


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

                    // await _mediaCaptureDouble1.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, cameras[(int)cameraType.mainCamera].VideoResolution.EncodingProperties);
                    await _mediaCaptureDouble2.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, cameras[(int)cameraType.termoCamera].VideoResolution.EncodingProperties);


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

            var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");
            IMediaExtension videoEffect = await _mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

            //-----------------------------------------------

            try
            {
                await _mediaCapture.StartPreviewAsync();
                _isPreviewing = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("error preview " + e.Message);
            }

        }

        private async Task StartPreviewDoubleAsync()
        {
            displayRequest.RequestActive();

            PreviewControlDouble1.Source = _mediaCaptureDouble1;
            PreviewControlDouble2.Source = _mediaCaptureDouble2;


            //------------ add video effect -----------------

            var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");
            //await _mediaCaptureDouble1.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);
            await _mediaCaptureDouble2.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);



            var temp = mainGrid.ActualSize;
            PreviewControlDouble1.Margin = new Thickness(0, 0, temp.X / 2, 0);
            PreviewControlDouble2.Margin = new Thickness(temp.X / 2, 0, 0, 0);

            try
            {
                await _mediaCaptureDouble2.StartPreviewAsync();
                await _mediaCaptureDouble1.StartPreviewAsync();
                _isPreviewing = true;
            }
            catch (Exception e)
            {
                Debug.WriteLine("error preview " + e.Message);
            }

        }

        private async Task StopPreviewAsync()
        {
            try
            {
                await _mediaCapture.StopPreviewAsync();
            }
            catch (Exception e)
            {
                Debug.WriteLine("error stop preview" + e.Message);
                _isFail = true;
            }
            _isPreviewing = false;

            await Dispatcher.RunAsync(CoreDispatcherPriority.High, () =>
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
            catch (Exception e)
            {
                Debug.WriteLine("error cleanupCamera" + e.Message);
            }
        }


        private async Task SetUpBasedOnStateAsync()
        {

            while (!_setupTask.IsCompleted)     //
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
            if (currentCameraType == (int)cameraType.termoCamera)
            {
                await MakePhotoAsync();
                return;
            }

            // switch camera resolution
            if (currentCameraType == (int)cameraType.mainCamera)
            {
                await SwitchHightResolution();
                await Task.Delay(200);
            }

            Debug.WriteLine("Start flashDelayTimer");
            if (_isFlash)
            {
                flashDelayTimer.Start();
                durationFlashDivider = 1;
                if (currentCameraType != (int)cameraType.termoCamera)
                    FlashCMD();
            }
            else
            {
                flashDelayTimer.Start();
                //await MakePhotoAsync();
                //await SwitchLowResolution();
            }
        }

        private async Task SwitchHightResolution()
        {
            SetCoordinateNotes(100, 2100, 82, 3250);

            if (currentCameraType == (int)cameraType.mainCamera)
            {
                await _mediaCapture.ClearEffectsAsync(MediaStreamType.VideoPreview);
                await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.Photo, cameras[(int)cameraType.mainCamera].PhotoResolution.EncodingProperties);

                var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");
                await _mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

                // задать координаты
            }
        }

        private async Task SwitchLowResolution()
        {

            SetCoordinateNotes(50, 1000, 40, 1250);
            if (currentCameraType == (int)cameraType.mainCamera)
                SetCoordinateNotes(150, (int)cameras[(int)cameraType.mainCamera].VideoResolution.Height - 200, 34, 1250);


            if (currentCameraType == (int)cameraType.mainCamera)
            {
                await _mediaCapture.ClearEffectsAsync(MediaStreamType.VideoPreview);
                await _mediaCapture.VideoDeviceController.SetMediaStreamPropertiesAsync(MediaStreamType.VideoRecord, cameras[(int)cameraType.mainCamera].VideoResolution.EncodingProperties);

                var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");
                await _mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

                // задать координаты
            }
        }

        StorageFile savedFile;
        private async Task MakePhotoAsync()
        {
            PreviewControl.Opacity = 0.5;
            opacityTimer.Start();

            Debug.WriteLine("Make photo");

            var stream = new InMemoryRandomAccessStream();

            await _mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

            try
            {
                var temp = " Tcenter=" + strPointT + " ";

                if ((currentCameraType == (int)cameraType.termoCamera))
                {
                    savedFile = await photoFolder.CreateFileAsync("Camera " + temp + DateTime.Now.ToString("d") + ".jpg", CreationCollisionOption.GenerateUniqueName);


                    using (InMemoryRandomAccessStream photoStream2 = stream,
                                                      photoStream2Transform = new InMemoryRandomAccessStream(),
                                                      fileStream = new InMemoryRandomAccessStream())
                    {
                        try
                        {
                            var decoder = await BitmapDecoder.CreateAsync(photoStream2);
                            var encoder = await BitmapEncoder.CreateForTranscodingAsync(photoStream2Transform, decoder);
                            if ((decoder.OrientedPixelWidth == 80) && (decoder.OrientedPixelHeight == 60))
                            {
                                encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Cubic;
                                encoder.BitmapTransform.ScaledHeight = 480;
                                encoder.BitmapTransform.ScaledWidth = 640;
                            }

                            await encoder.FlushAsync();

                            CanvasBitmap cbi;
                            CanvasDevice device = CanvasDevice.GetSharedDevice();
                            CanvasRenderTarget offscreen = new CanvasRenderTarget(device, 640, 480, 96);
                            cbi = await CanvasBitmap.LoadAsync(device, photoStream2Transform);

                            using (var ds = offscreen.CreateDrawingSession())
                            {
                                ds.DrawImage(cbi, 0, 0);

                                ds.DrawText("Tcenter = " + strPointT, 10, 10, Colors.PaleTurquoise, new CanvasTextFormat
                                {
                                    FontSize = 24,
                                    FontWeight = Windows.UI.Text.FontWeights.Bold
                                });
                                ds.DrawLine(320, 230, 320, 250, Colors.Black);
                                ds.DrawLine(310, 240, 330, 240, Colors.Black);

                            }

                            fileStream.Seek(0);
                            await offscreen.SaveAsync(fileStream, CanvasBitmapFileFormat.Png);
                            await SavePhotoAsync(fileStream, savedFile);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Exception while making preview " + ex.Message.ToString());
                        }
                    }



                }
                else
                {
                    savedFile = await photoFolder.CreateFileAsync("Camera " + DateTime.Now.ToString("d") + ".jpg", CreationCollisionOption.GenerateUniqueName);
                    await SavePhotoAsync(stream, savedFile);
                }


                Debug.WriteLine("Photo saved in" + savedFile.Path);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while making photo " + ex.Message.ToString());
            }

            try
            {
                BitmapImage bitmapPreviewImage = await StorageFileToBitmapImage(savedFile);
                imageControlPreview.Source = bitmapPreviewImage;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Exception while making preview " + ex.Message.ToString());
            }

        }


        private async Task MakePhotoDoubleAsync()
        {
            PreviewControlDouble1.Opacity = 0.5;
            PreviewControlDouble2.Opacity = 0.5;
            opacityTimer.Start();

            Debug.WriteLine("Make photo");

            var stream1 = new InMemoryRandomAccessStream();
            var stream2 = new InMemoryRandomAccessStream();

            await _mediaCaptureDouble1.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream1);
            await _mediaCaptureDouble2.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream2);

            await CombineTwoPhoto(stream1, stream2);

        }

        private async Task CombineTwoPhoto(InMemoryRandomAccessStream _photoStream1, InMemoryRandomAccessStream _photoStream2)
        {
            using (InMemoryRandomAccessStream photoStream1 = _photoStream1,
                                              photoStream2 = _photoStream2,
                                              photoStream2Transform = new InMemoryRandomAccessStream(),
                                              fileStream = new InMemoryRandomAccessStream())
            {
                try
                {
                    var decoder = await BitmapDecoder.CreateAsync(photoStream2);
                    var encoder = await BitmapEncoder.CreateForTranscodingAsync(photoStream2Transform, decoder);
                    if ((decoder.OrientedPixelWidth == 80) && (decoder.OrientedPixelHeight == 60))
                    {
                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Cubic;
                        encoder.BitmapTransform.ScaledHeight = 480;
                        encoder.BitmapTransform.ScaledWidth = 640;
                    }

                    await encoder.FlushAsync();

                    CanvasBitmap cbi1, cbi2;
                    CanvasDevice device = CanvasDevice.GetSharedDevice();
                    CanvasRenderTarget offscreen = new CanvasRenderTarget(device, 1280, 480, 96);
                    cbi1 = await CanvasBitmap.LoadAsync(device, photoStream1);
                    cbi2 = await CanvasBitmap.LoadAsync(device, photoStream2Transform);

                    using (var ds = offscreen.CreateDrawingSession())
                    {
                        ds.DrawImage(cbi1, 0, 0);
                        ds.DrawImage(cbi2, 640, 0);
                        ds.DrawText("Tcenter = " + strPointT, 650, 10, Colors.PaleTurquoise, new CanvasTextFormat
                        {
                            FontSize = 24,
                            FontWeight = Windows.UI.Text.FontWeights.Bold
                        });
                        ds.DrawLine(960, 230, 960, 250, Colors.Black);
                        ds.DrawLine(950, 240, 970, 240, Colors.Black);

                    }



                    fileStream.Seek(0);
                    await offscreen.SaveAsync(fileStream, CanvasBitmapFileFormat.Png);

                    BitmapImage image = new BitmapImage();
                    image.SetSource(fileStream);
                    imageControlPreview.Source = image;

                    var temp = " Tcenter=" + strPointT + " ";
                    savedFile = await photoFolder.CreateFileAsync("CameraDouble " + temp + DateTime.Now.ToString("d") + ".jpg", CreationCollisionOption.GenerateUniqueName);
                    await SavePhotoAsync(fileStream, savedFile);



                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Exception while making preview " + ex.Message.ToString());
                }
            }
        }

        public static async Task<BitmapImage> StorageFileToBitmapImage(StorageFile savedStorageFile)
        {
            using (IRandomAccessStream fileStream = await savedStorageFile.OpenAsync(Windows.Storage.FileAccessMode.Read))
            {
                BitmapImage bitmapImage = new BitmapImage();
                bitmapImage.DecodePixelHeight = 200;
                bitmapImage.DecodePixelWidth = 200;
                await bitmapImage.SetSourceAsync(fileStream);
                return bitmapImage;
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
                        encoder.BitmapTransform.InterpolationMode = BitmapInterpolationMode.Cubic;
                        encoder.BitmapTransform.ScaledHeight = 480;
                        encoder.BitmapTransform.ScaledWidth = 640;

                    }


                    await encoder.FlushAsync();
                }
            }


        }

        private async void mainCameraButton_Click(object sender, RoutedEventArgs e)
        {
            progressRingStart.Visibility = Visibility.Visible;
            progressRingStart.IsActive = true;

            _isUIActive = false;
            _isFlash = true;

            if (_isRecording)
            {
                await StopRecordingAsync();

                stopWatch.Reset();
                recordTimer.Stop();
            }

            stopMeasure();
            firstDistanceFlag = false;
            _findLenghtZero = false;
            videoEffectSettings.getLenghtFlag = false;
            videoEffectSettings.termo = false;

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

            _isDouble = false;

            while(histogramStatisticTimer.IsEnabled)
                histogramStatisticTimer.Stop();

            SetCoordinateNotes(150, (int)cameras[(int)cameraType.mainCamera].VideoResolution.Height - 200, 34, 1250);


            progressRingStart.Visibility = Visibility.Collapsed;
            progressRingStart.IsActive = false;

            UpdateUIControls();
        }

        private async void endoCameraButton_Click(object sender, RoutedEventArgs e)
        {
            progressRingStart.Visibility = Visibility.Visible;
            progressRingStart.IsActive = true;

            _isFlash = false;
            _isUIActive = false;

            if (_isRecording)
            {
                await StopRecordingAsync();

                stopWatch.Reset();
                recordTimer.Stop();
            }


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
            videoEffectSettings.termo = false;

            SetCoordinateNotes(50, 1000, 40, 1550);
            //runMeasure.Visibility = Visibility.Visible;

            while (histogramStatisticTimer.IsEnabled)
                histogramStatisticTimer.Stop();

            progressRingStart.Visibility = Visibility.Collapsed;
            progressRingStart.IsActive = false;

            UpdateUIControls();
        }

        private async void termoCameraButton_Click(object sender, RoutedEventArgs e)
        {
            progressRingStart.Visibility = Visibility.Visible;
            progressRingStart.IsActive = true;

            _isFlash = false;
            _isUIActive = false;

            if (_isRecording)
            {
                await StopRecordingAsync();

                stopWatch.Reset();
                recordTimer.Stop();
            }


            currentCameraType = (int)cameraType.termoCamera;

            try
            {
                await CleanupCameraAsync();

                _isDouble = false;

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
            videoEffectSettings.termo = true;
            
            UpdateUIControls();

            histogramStatisticTimer.Start();

            progressRingStart.Visibility = Visibility.Collapsed;
            progressRingStart.IsActive = false;
        }

        private async void doubleCameraButton_Click(object sender, RoutedEventArgs e)
        {
            progressRingStart.Visibility = Visibility.Visible;
            progressRingStart.IsActive = true;

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

            videoEffectSettings.termo = true;

            //SetCoordinateNotes(150, 300, 34, 1250);

            progressRingStart.Visibility = Visibility.Collapsed;
            progressRingStart.IsActive = false;

            UpdateUIControls();

            histogramStatisticTimer.Start();



        }

        private async void VideoButton_Click(object sender, RoutedEventArgs e)
        {
            _isPause = false;
            if (!_isRecording)
            {
                _isRecording = true;
                UpdateUIControls();

                if (currentCameraType != (int)cameraType.doubleCamera)  // can't record video in double mode
                {
                    await StartRecordingAsync();

                    recordTimer.Start();
                    stopWatch.Start();
                }
            }
            else
            {
                _isRecording = false;
                UpdateUIControls();

                if (currentCameraType != (int)cameraType.doubleCamera)
                {
                    await StopRecordingAsync();
                    recordTimeTextBox.Text = "";
                    stopWatch.Reset();
                    recordTimer.Stop();
                }

            }

        }

        private async Task StartRecordingAsync()
        {
            Debug.WriteLine("StartRecord");

            savedFile = await videoFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            try
            {
                if (currentCameraType == (int)cameraType.termoCamera)
                    _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Wvga), savedFile);
                else
                    _mediaRecording = await _mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), savedFile);
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

            var previewProperties = _mediaCapture.VideoDeviceController.GetMediaStreamProperties(MediaStreamType.VideoPreview) as VideoEncodingProperties;
            VideoFrame videoFrame = new VideoFrame(BitmapPixelFormat.Bgra8, (int)previewProperties.Width, (int)previewProperties.Height);

            var previewFrame = await _mediaCapture.GetPreviewFrameAsync(videoFrame);
            SoftwareBitmap previewBitmap = previewFrame.SoftwareBitmap;
            var source = new SoftwareBitmapSource();
            await source.SetBitmapAsync(previewBitmap);
            imageControlPreview.Source = source;

            previewFrame.Dispose();
            previewFrame = null;

            _isRecording = false;
            await _mediaCapture.StopRecordAsync();



            Debug.WriteLine("Stopped recording!");
        }


        private void UpdateUIControls()
        {
            // diagnostic information
            textBoxInfo.Visibility = Visibility.Collapsed;

            termoPanelDot.Margin = _isDouble ? new Thickness(0, 232, 245, 0) : new Thickness(0, 232, 490, 0);
            termoPanel1.Margin = _isDouble ? new Thickness(0, 0, 235, 0) : new Thickness(0, 0, 470, 0);

            buttonFlash.Visibility = _isRecording ? Visibility.Collapsed : Visibility.Visible;
            notesButton.Visibility = (currentCameraType != (int)cameraType.termoCamera) && !_isDouble ? Visibility.Visible : Visibility.Collapsed;

            panelNotes.Visibility = _isNotes && (currentCameraType != (int)cameraType.termoCamera) && !_isDouble ? Visibility.Visible : Visibility.Collapsed;  //


            PauseVideoButton.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;
            PauseIcon.Visibility = _isPause ? Visibility.Collapsed : Visibility.Visible;
            ResumeIcon.Visibility = _isPause ? Visibility.Visible : Visibility.Collapsed;

            mainCameraButton.Visibility = (_isMainCameraFlag) ? Visibility.Visible : Visibility.Collapsed;
            endoCameraButton.Visibility = (_isEndoCameraFlag) ? Visibility.Visible : Visibility.Collapsed;
            termoCameraButton.Visibility = (_isTermoCameraFlag) ? Visibility.Visible : Visibility.Collapsed;

            //termoPanel.Visibility = (_isTermoCameraFlag && (currentCameraType == (int)cameraType.termoCamera)) ? Visibility.Visible : Visibility.Collapsed;
            termoPanel1.Visibility = (_isTermoCameraFlag && (currentCameraType == (int)cameraType.termoCamera || currentCameraType == (int)cameraType.doubleCamera)) ? Visibility.Visible : Visibility.Collapsed;
            termoPanelDot.Visibility = (_isTermoCameraFlag && (currentCameraType == (int)cameraType.termoCamera || currentCameraType == (int)cameraType.doubleCamera)) ? Visibility.Visible : Visibility.Collapsed;

            //CenterIcon.Visibility = (_isTermoCameraFlag && (currentCameraType == (int)cameraType.termoCamera)) ? Visibility.Visible : Visibility.Collapsed;
            //doubleCameraButton.Visibility = (_isMainCameraFlag && _isTermoCameraFlag) ? Visibility.Visible : Visibility.Collapsed;            

            //_getDistanse.Visibility = Visibility.Collapsed;
            //_stopMeasure.Visibility = Visibility.Collapsed;


            // The buttons should only be enabled if the preview started sucessfully
            PhotoButton.IsEnabled = _isPreviewing;
            VideoButton.IsEnabled = _isPreviewing;
            VideoButton.Visibility = _isDouble ? Visibility.Collapsed : Visibility.Visible;

            // Update recording button to show "Stop" icon instead of red "Record" icon
            StartRecordingIcon.Visibility = _isRecording ? Visibility.Collapsed : Visibility.Visible;
            StopRecordingIcon.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;
            Rec.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;    //&& !_isPause
            recordTimeTextBox.Visibility = _isRecording ? Visibility.Visible : Visibility.Collapsed;

            // Update microphone button 
            MicOff.Visibility = _isMicrophone ? Visibility.Collapsed : Visibility.Visible;
            MicOn.Visibility = _isMicrophone ? Visibility.Visible : Visibility.Collapsed;

            // Update flash button
            NotFlashIcon.Visibility = _isFlash ? Visibility.Collapsed : Visibility.Visible;
            NotFlashIcon1.Visibility = _isFlash ? Visibility.Collapsed : Visibility.Visible;
            FlashIcon.Visibility = _isFlash ? Visibility.Visible : Visibility.Collapsed;
            plusFlashButton.Visibility = _isFlash && (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible : Visibility.Collapsed;
            minusFlashButton.Visibility = _isFlash && (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible : Visibility.Collapsed;
            pbFlashPower.Visibility = _isFlash && (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible : Visibility.Collapsed;
            buttonFlash.Visibility = (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Visible : Visibility.Collapsed;

            // Update flash button Endo
            //NotFlashIconEndo.Visibility      = _isFlashEndo ? Visibility.Collapsed : Visibility.Visible;
            //FlashIconEndo.Visibility         = _isFlashEndo ? Visibility.Visible   : Visibility.Collapsed;
            plusFlashButtonEndo.Visibility = _isFlashEndo && (currentCameraType == (int)cameraType.endoCamera) && !_isDouble ? Visibility.Visible : Visibility.Collapsed;
            minusFlashButtonEndo.Visibility = _isFlashEndo && (currentCameraType == (int)cameraType.endoCamera) && !_isDouble ? Visibility.Visible : Visibility.Collapsed;
            pbFlashPowerEndo.Visibility = _isFlashEndo && (currentCameraType == (int)cameraType.endoCamera) && !_isDouble ? Visibility.Visible : Visibility.Collapsed;
            //buttonFlashEndo.Visibility       = (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Visible : Visibility.Collapsed;

            // Update lenght meter controls            
            //runMeasure.Visibility = (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Visible : Visibility.Collapsed;

            if (_findLenghtZero)
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

            notesButton.Background = !_isNotes ? new SolidColorBrush(color) : new SolidColorBrush(Windows.UI.Colors.MediumSlateBlue);

            buttonFlash.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Collapsed : Visibility.Visible;
            plusFlashButton.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Collapsed : Visibility.Visible;
            minusFlashButton.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Collapsed : Visibility.Visible;
            pbFlashPower.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.endoCamera) ? Visibility.Collapsed : Visibility.Visible;

            //buttonFlashEndo.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.mainCamera) ? Visibility.Collapsed : Visibility.Visible;
            plusFlashButtonEndo.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.mainCamera) || _isDouble ? Visibility.Collapsed : Visibility.Visible;
            minusFlashButtonEndo.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.mainCamera) || _isDouble ? Visibility.Collapsed : Visibility.Visible;
            pbFlashPowerEndo.Visibility = _isRecording || (currentCameraType == (int)cameraType.termoCamera) || (currentCameraType == (int)cameraType.mainCamera) || _isDouble ? Visibility.Collapsed : Visibility.Visible;


            Rec.Text = _isPause ? "Pause" : "Rec";

            //videoEffectSettings.X = cameras[_cntCamera].X;
            //videoEffectSettings.Y = cameras[_cntCamera].Y;
            //videoEffectSettings.FontSize = cameras[_cntCamera].FontSize;

            //videoEffectSettings.X = 750;
            //videoEffectSettings.Y = 600;
            //videoEffectSettings.FontSize = 90;

        }

        private async void getScenarioSettings_Click(object sender, RoutedEventArgs e)
        {
            await CleanupCameraAsync();

            this.Frame.Navigate(typeof(SettingsPage));
        }

        private void buttonFlash_Click(object sender, RoutedEventArgs e)
        {
            _isFlash = !_isFlash;
            UpdateUIControls();
        }
        private void buttonFlashEndo_Click(object sender, RoutedEventArgs e)
        {
            //_isFlashEndo = !_isFlashEndo;
            UpdateUIControls();
        }
        private void FlashCMD()
        {
            Debug.WriteLine("Send flash");
            textBoxInfo.Text += "Send flash" + Environment.NewLine;

            byte[] data = new byte[1];
            byte[] flashByte = new byte[2];

            flashByte[0] = (byte)((int)(1199 - pbFlashPower.Value) & 0x00FF);
            flashByte[1] = (byte)((int)(1199 - pbFlashPower.Value) >> 8);
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
        short FlashDuration = 0x15;
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


        private void ButtonUpNotes_Click(object sender, RoutedEventArgs e)
        {
            if ((stringNote.Count > 0) && (stringNote.Count - (indexNoteShow + 1) > 0))
            {
                indexNoteShow = indexNoteShow + 1;
                videoEffectSettings.commet = stringNote[stringNote.Count - 1 - indexNoteShow];
                textBlockNotes.Text = stringNote[stringNote.Count - 1 - indexNoteShow];
            }

        }

        private void buttonDownNotes_Click(object sender, RoutedEventArgs e)
        {
            if (indexNoteShow > 0)
            {
                indexNoteShow = indexNoteShow - 1;
                videoEffectSettings.commet = stringNote[stringNote.Count - 1 - indexNoteShow];
                textBlockNotes.Text = stringNote[stringNote.Count - 1 - indexNoteShow];
            }

        }

        private void ButtonClearNotes_Click(object sender, RoutedEventArgs e)
        {

        }

        private async void ButtonSetNotes_Click(object sender, RoutedEventArgs e)
        {

            if (textBlockNotes.Text == "")
                indexNoteShow = 0;

            if (!stringNote.Contains(textBlockNotes.Text))
            {
                stringNote.Add(textBlockNotes.Text);

                await FileIO.AppendTextAsync(notesFile, "\n" + textBlockNotes.Text);

                //videoEffectSettings.commet = textBlockNotes.Text;

            }
            videoEffectSettings.commet = textBlockNotes.Text;
            //videoEffectSettings.commet = stringNote[indexNoteShow];

        }

        private async void imageControlPreview_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {

            try
            {
                StorageFolder dlFolder = await savedFile.GetParentAsync();
                await Windows.System.Launcher.LaunchFolderAsync(dlFolder);
            }
            catch(Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            //await Windows.System.Launcher.LaunchFileAsync(savedFile);
        }


        private async void PauseVideoButton_Click(object sender, RoutedEventArgs e)
        {
            _isPause = !_isPause;


            UpdateUIControls();

            if (_isPause)
            {
                await _mediaRecording.PauseAsync(Windows.Media.Devices.MediaCapturePauseBehavior.ReleaseHardwareResources);

                //timePause = DateTime.Now;
                //recordTimerPause.Start();
                stopWatch.Stop();
            }
            else
            {
                stopWatch.Start();
                await _mediaRecording.ResumeAsync();
            }
        }

        private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {

        }

        static int temp1 = 0;
        private void NotesButton_Click(object sender, RoutedEventArgs e)
        {
            // StartUdpClient();

            indexNoteShow = stringNote.Count - 1;
            _isNotes = !_isNotes;
            UpdateUIControls();


            //videoEffectSettings.commet = "Заметка" + ++temp1;
        }

        private void OutputHelpCommand()
        {
            _isHelpCommands = !_isHelpCommands;
            if (_isHelpCommands)
            {
                textBoxInfo.Visibility = Visibility.Visible;
                textBoxInfo.Text = "Список доступных голосовых команд:\n" +
                                   "Главная     -  главная камера\n" +
                                   "Эндо        -  камера эндоскопа\n" +
                                   "Термо       -  камера тепловизора\n" +
                                   "Двойной     -  главная камера и камера тепловизора\n" +
                                   "Фотография  -  режим фотографирования\n" +
                                   "Запись      -  режим видео записи\n" +
                                   "Стоп        -  остановка видео записи\n" +
                                   "Пауза       -  пауза видео записи\n" +
                                   "Вспышка     -  включить/выключить вспышку\n" +
                                   "Плюс        -  увеличить яркость вспышки\n" +
                                   "Минус       -  уменьшить яркость вспышки\n" +
                                   "Заметка     -  режим заметок\n" +
                                   "Альбом      -  открыть альбом\n" +
                                   "Помощь      -  вывести/скрыть список голосовых команд\n";
            }
            else
            {
                textBoxInfo.Visibility = Visibility.Collapsed;
                textBoxInfo.Text = "";
            }

        }

        private void MicrophoneButton_Click(object sender, RoutedEventArgs e)
        {
            if (!_isMicrophone)
            {
                _isMicrophone = true;
                MicrophoneOn();
            }
            else
            {
                _isMicrophone = false;
                MicrophoneOff();
            }

            UpdateUIControls();
        }

        private void MicrophoneOn()
        {
            if (serialPortLepton.IsOpen)
            {
                try
                {
                    serialPortLepton.DiscardInBuffer();
                    serialPortLepton.Write("on\n");

                    Debug.WriteLine("serialPortLepton open");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        private void MicrophoneOff()
        {
            if (serialPortLepton.IsOpen)
            {
                try
                {
                    serialPortLepton.DiscardInBuffer();
                    serialPortLepton.Write("off\n");

                    Debug.WriteLine("serialPortLepton open");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

    }


}
