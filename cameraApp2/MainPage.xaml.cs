using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Media.Effects;
using Windows.Media;
using Windows.Devices.HumanInterfaceDevice;
using System.Linq;
using Windows.UI.Core;
using Windows.Security.Cryptography;
using VideoEffectComponent;
using System.IO.Ports;



// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace cameraApp2
{
    

    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {

        private MediaCapture mediaCapture;        
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

        //private MediaFrameReader mediaFrameReader;
        //StorageFile fileVideo;
        //private SoftwareBitmap backBuffer;
        //private bool taskRunning = false;
        //CanvasBitmap canvasBitmap;
        //MediaComposition mediaComposition;
        //private MediaCapture mediaCaptureNote;

        public MainPage()
        {

            this.InitializeComponent();
            EnumerateHidDevices();

            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 500);

            serialPortEndo = new SerialPort("COM5", 9600, Parity.None, 8, StopBits.One);
            
            if(serialPortEndo != null)
            {
                serialPortEndo.Open();
                serialPortEndo.DataReceived += SerialPortEndo_DataReceived;
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
                        tempValue.lenght = "\n Lenght = " + value.ToString();
                        tempValue.coordinate = "\n x = " + Xf.ToString() + "\n y = " + Yf.ToString() + "\n z = " + Zf.ToString();
                         
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

        private static async Task<DeviceInformationCollection> FindCameraDeviceAsync()
        {
            var allVideoDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
            return allVideoDevices;
        }

        private async Task initializeCameraAsync()
        {
            Debug.WriteLine("initializeCametaAsync");

            if (mediaCapture == null)
            {
                var cameraDeviceList = await FindCameraDeviceAsync();

                if (cameraDeviceList.Count == 0)
                {
                    Debug.WriteLine("No camera device found");
                    return;
                }

                DeviceInformation cameraDevice;
                cameraDevice = cameraDeviceList[0];
                mediaCapture = new MediaCapture(); 
                



                var settings = new MediaCaptureInitializationSettings { VideoDeviceId = cameraDevice.Id };

                try
                {
                    await mediaCapture.InitializeAsync(settings);
                    isInitialized = true;

                }
                catch (UnauthorizedAccessException)
                {
                    Debug.WriteLine("camera denided");
                }




                if (isInitialized)
                {
                    await StartPreviewAsync();
                }


                /*var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
                fileVideo = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
                mediaComposition = new MediaComposition();*/
            }

        }

        private async Task StartPreviewAsync()
        {
            displayRequest.RequestActive();
            PreviewControl.Source = mediaCapture;


            //------------ add video effect -----------------
            var videoEffectDefinition = new VideoEffectDefinition("VideoEffectComponent.ExampleVideoEffect");

            IMediaExtension videoEffect =
               await mediaCapture.AddVideoEffectAsync(videoEffectDefinition, MediaStreamType.VideoPreview);

            videoEffect.SetProperties(new PropertySet() { { "FadeValue", 0.1 } });
            videoEffect.SetProperties(new PropertySet() { { "LenghtValue",  Lenght} });
            //-----------------------------------------------

            await mediaCapture.StartPreviewAsync();
        }

        private async void SwitchCam_Click(object sender, RoutedEventArgs e)
        {
            await initializeCameraAsync();
        }

        private async void MakePhoto_Click(object sender, RoutedEventArgs e)
        {
            await MakePhotoAsync();
        }

        private async Task MakePhotoAsync()
        {
            Debug.WriteLine("Make photo");
            var ImagesLib = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            storageFolder = ImagesLib.SaveFolder ?? ApplicationData.Current.LocalFolder;

            var stream = new InMemoryRandomAccessStream();
            await mediaCapture.CapturePhotoToStreamAsync(ImageEncodingProperties.CreateJpeg(), stream);

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

        private async Task StartRecordAsync()
        {
            Debug.WriteLine("StartRecord");

            var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            try
            {
                _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Auto), file);
                await _mediaRecording.StartAsync();
            }
            catch(Exception e)
            {
                Debug.WriteLine("err3 = " + e.ToString());
            }
        }


        private async void StartRecord_Click(object sender, RoutedEventArgs e)
        {
            await StartRecordAsync();

        }        

        

        private async Task RecordAsync()
        {
            Debug.WriteLine("StartRecord");
            //await mediaComposition.RenderToFileAsync(fileVideo, MediaTrimmingPreference.Fast, MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p));
        }


        private async void StopRecord_Click(object sender, RoutedEventArgs e)
        {
           await _mediaRecording.FinishAsync();
             //await RecordAsync();
        }

        private async void GetFrame_Click(object sender, RoutedEventArgs e)
        {
/*
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

            mediaCapture = new MediaCapture();

            var settings = new MediaCaptureInitializationSettings()
            {
                SourceGroup = selectedGroup,
                SharingMode = MediaCaptureSharingMode.ExclusiveControl,
                MemoryPreference = MediaCaptureMemoryPreference.Cpu,
                StreamingCaptureMode = StreamingCaptureMode.Video
            };
            try
            {
                await mediaCapture.InitializeAsync(settings);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("MediaCapture initialization failed: " + ex.Message);
                return;
            }


            var colorFrameSourse = mediaCapture.FrameSources[colorSourceInfo.Id];
            var preferredFormat = colorFrameSourse.SupportedFormats.Where(format =>
            {
                return format.VideoFormat.Width >= 1080;               //&& format.Subtype == MediaEncodingSubtypes.Argb32
            }).FirstOrDefault();
            if (preferredFormat == null)
            {
                Debug.WriteLine("designed format not supported");
                return;
            }

            await colorFrameSourse.SetFormatAsync(preferredFormat);

            imageElement.Source = new SoftwareBitmapSource();
            imageElementNote.Source = new SoftwareBitmapSource();

           
            mediaFrameReader = await mediaCapture.CreateFrameReaderAsync(colorFrameSourse, MediaEncodingSubtypes.Argb32);
            mediaFrameReader.FrameArrived += ColorFrameReader_FrameArrived;
            await mediaFrameReader.StartAsync();
 */
        }
       


        
        //private MediaComposition mediaComposition = new MediaComposition();

        private async void ColorFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
           /* var mediaFrameReference = sender.TryAcquireLatestFrame();
            var videoMediaFrame = mediaFrameReference?.VideoMediaFrame;
            var softwareBitmap = videoMediaFrame?.SoftwareBitmap;

            if(softwareBitmap != null)
            {
                if (softwareBitmap.BitmapPixelFormat != BitmapPixelFormat.Bgra8 || softwareBitmap.BitmapAlphaMode != BitmapAlphaMode.Premultiplied)
                {
                    softwareBitmap = SoftwareBitmap.Convert(softwareBitmap, BitmapPixelFormat.Bgra8, BitmapAlphaMode.Premultiplied);
                }


                //------------*******************----------------


                //!!!!!! Here i can create writableBitmap from sotfwareBitmap and softwareBitmap from writableBitmap                
                // await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                //{
                //    writeableBitmap = new WriteableBitmap((int)softwareBitmap.PixelWidth, (int)softwareBitmap.PixelHeight);
                //    softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);                   
                //    //softwareBitmap.CopyFromBuffer(writeableBitmap.PixelBuffer);
                //});
                //


                var device = CanvasDevice.GetSharedDevice();
                //var image = default(CanvasBitmap);
                CanvasBitmap image = CanvasBitmap.CreateFromSoftwareBitmap(device, softwareBitmap);
                



                var offscreen = new CanvasRenderTarget(device, (float)image.Bounds.Width, (float)image.Bounds.Height, 96);
                using (var ds = offscreen.CreateDrawingSession())
                {
                    ds.DrawImage(image, 0, 0);
                    ds.DrawText("Hello augmented reality", 15, 400, Colors.Aqua, new CanvasTextFormat
                    {
                        FontSize = 24,
                        FontWeight = Windows.UI.Text.FontWeights.Bold
                    });
                    ds.DrawText(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"), 15, 430, Colors.Aquamarine, new CanvasTextFormat
                    {
                        FontSize = 20,
                        FontWeight = Windows.UI.Text.FontWeights.Bold
                    });
                    Rect rect = new Rect(10, 400, 300, 200);                    
                    ds.DrawRectangle(rect, Colors.Chartreuse);

                }
                             

                MediaClip mediaClip = MediaClip.CreateFromSurface(offscreen, TimeSpan.FromMilliseconds(80));
               // mediaComposition = new MediaComposition();
                mediaComposition.Clips.Add(mediaClip);
                


                var bytePixel = offscreen.GetPixelBytes();
                IBuffer buffrPixel = bytePixel.AsBuffer();
                softwareBitmap.CopyFromBuffer(buffrPixel);

                // MediaClip <- IDirectSurfase(CreateFromSurgase) <- IBuffer 

                //----------------                


                softwareBitmap = Interlocked.Exchange(ref backBuffer, softwareBitmap);
                softwareBitmap?.Dispose();

                var task = imageElement.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.High,
                    async () =>
                    {
                        if (taskRunning)
                        {
                            return;
                        }
                        taskRunning = true;

                        SoftwareBitmap latestBitmap;
                        while((latestBitmap = Interlocked.Exchange(ref backBuffer, null)) != null)
                        {
                            var imageSource  = (SoftwareBitmapSource)imageElement.Source;
                            await imageSource.SetBitmapAsync(latestBitmap);                            
                        }

                        taskRunning = false;

                    });

                
            }

            //+++++++++++++++++++++++++++++++

            // These files could be picked from a location that we won't have access to later

            //var storageItemAccessList = Windows.Storage.AccessCache.StorageApplicationPermissions.FutureAccessList;
            //storageItemAccessList.Add(softwareBitmap);

            //var clip = await MediaClip.CreateFromSurface;
            //composition.Clips.Add(clip);


            //Debug.WriteLine("StartRecord");           

            //+++++++++++++++++++++++++++++++

            if (mediaFrameReference != null)
                mediaFrameReference.Dispose();
         */   
        }
        
        
        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
           /* args.DrawingSession.DrawImage(canvasBitmap);
            args.DrawingSession.DrawText("hello test", 100, 100, Colors.Aqua);
            args.DrawingSession.DrawText("hello test", 100, 120, Colors.Brown);
           */ 
        }        

        private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            /*try
            {
                args.TrackAsyncAction(CreateResourceAsync(sender).AsAsyncAction());
            }
            catch (Exception ex)
            { Debug.WriteLine("Err1 " + ex.ToString()); }
            */
        }

        async Task CreateResourceAsync(CanvasControl sender)
        {
           /* try 
            {                
                var photoFile = await KnownFolders.PicturesLibrary.CreateFileAsync("CameraPhoto.jpg", CreationCollisionOption.OpenIfExists);

                if(photoFile != null)
                {
                    using(var stream = await photoFile.OpenAsync(FileAccessMode.ReadWrite))
                    {
                        canvasBitmap = await CanvasBitmap.LoadAsync(sender, stream);
                    }
                }

            }
            catch(Exception ex)
            { Debug.WriteLine("Err2 " + ex.ToString()); }
           */
        }

       
    }
}
