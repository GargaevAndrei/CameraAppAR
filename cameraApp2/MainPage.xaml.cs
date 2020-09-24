using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Imaging;
using Windows.Media.Capture;
using Windows.Media.Capture.Frames;
using Windows.Media.MediaProperties;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.Display;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

using Windows.UI.Xaml.Navigation;

using System.Numerics;
using System.Drawing;
using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas.UI.Xaml;
using Windows.Storage.Pickers;
using Windows.Media.Core;
using Windows.Media.Playback;
using Microsoft.Graphics.Canvas.Text;
using Windows.Media.Editing;


// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x419

namespace cameraApp2
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private MediaCapture mediaCapture;
        private MediaCapture mediaCaptureNote;
        bool isInitialized = false;
        private readonly DisplayRequest displayRequest = new DisplayRequest();
        private StorageFolder storageFolder = null;

        private InMemoryRandomAccessStream videoStream = new InMemoryRandomAccessStream();
        private LowLagMediaRecording _mediaRecording;

        private MediaFrameReader mediaFrameReader;
        StorageFile fileVideo;
        private SoftwareBitmap backBuffer;
        private bool taskRunning = false;
        CanvasBitmap canvasBitmap;
        MediaComposition mediaComposition;

        public MainPage()
        {
            this.InitializeComponent();
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


                var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
                fileVideo = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
                mediaComposition = new MediaComposition();
            }

        }

        private async Task StartPreviewAsync()
        {
            displayRequest.RequestActive();
            PreviewControl.Source = mediaCapture;
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


        private async void StartRecord_Click(object sender, RoutedEventArgs e)
        {
            await StartRecordAsync();

        }


        private async Task StartRecordAsync()
        {
            Debug.WriteLine("StartRecord");

            var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            _mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga), file);
            await _mediaRecording.StartAsync();

        }

        

        private async Task RecordAsync()
        {
            Debug.WriteLine("StartRecord");
            await mediaComposition.RenderToFileAsync(fileVideo, MediaTrimmingPreference.Fast, MediaEncodingProfile.CreateMp4(VideoEncodingQuality.HD720p));
        }


        private async void StopRecord_Click(object sender, RoutedEventArgs e)
        {
            //await _mediaRecording.FinishAsync();
            await RecordAsync();
        }

        private async void GetFrame_Click(object sender, RoutedEventArgs e)
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

        }



        
        //private MediaComposition mediaComposition = new MediaComposition();

        private async void ColorFrameReader_FrameArrived(MediaFrameReader sender, MediaFrameArrivedEventArgs args)
        {
            var mediaFrameReference = sender.TryAcquireLatestFrame();
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
                /* await this.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
                {
                    writeableBitmap = new WriteableBitmap((int)softwareBitmap.PixelWidth, (int)softwareBitmap.PixelHeight);
                    softwareBitmap.CopyToBuffer(writeableBitmap.PixelBuffer);                   
                    //softwareBitmap.CopyFromBuffer(writeableBitmap.PixelBuffer);
                });
                */


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

            //var myVideos = await Windows.Storage.StorageLibrary.GetLibraryAsync(Windows.Storage.KnownLibraryId.Pictures);
            //StorageFile file = await myVideos.SaveFolder.CreateFileAsync("video.mp4", CreationCollisionOption.GenerateUniqueName);
            //_mediaRecording = await mediaCapture.PrepareLowLagRecordToStorageFileAsync(MediaEncodingProfile.CreateMp4(VideoEncodingQuality.Vga), file);
            //await _mediaRecording.StartAsync();

            //+++++++++++++++++++++++++++++++

            if (mediaFrameReference != null)
                mediaFrameReference.Dispose();
            
        }
        
        
        private void canvas_Draw(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.Xaml.CanvasDrawEventArgs args)
        {
            args.DrawingSession.DrawImage(canvasBitmap);
            args.DrawingSession.DrawText("hello test", 100, 100, Colors.Aqua);
            args.DrawingSession.DrawText("hello test", 100, 120, Colors.Brown);
            
        }        

        private void canvas_CreateResources(Microsoft.Graphics.Canvas.UI.Xaml.CanvasControl sender, Microsoft.Graphics.Canvas.UI.CanvasCreateResourcesEventArgs args)
        {
            try
            {
                args.TrackAsyncAction(CreateResourceAsync(sender).AsAsyncAction());
            }
            catch (Exception ex)
            { Debug.WriteLine("Err1 " + ex.ToString()); }

        }

        async Task CreateResourceAsync(CanvasControl sender)
        {
            try 
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
        }

        /*private void InitializeMediaStreamSource()
        {
            // create VideoEncodingProperties 
            VideoEncodingProperties videoProps = VideoEncodingProperties.CreateH264();

            // create VideoStreamDescriptor 
            VideoStreamDescriptor videoDescriptor = new VideoStreamDescriptor(videoProps);

            // create MediaStreamSource 
            var MSS = new Windows.Media.Core.MediaStreamSource(videoDescriptor);

            // hooking up the MediaStreamSource event handlers 
            MSS.Starting += MSS_Starting;
            MSS.SampleRequested += MSS_SampleRequested;
            MSS.Closed += MSS_Closed;

            // hook up our MediaPlayer with the MediaStreamSource 
            MediaPlayer.SetStreamSource(MSS);
        }

        private void MSS_SampleRequested(MediaStreamSource source, MediaStreamSourceSampleRequestedEventArgs args)
        {
            // Get Direct3DSurface from WinRT screen capture API 
            IDirect3DSurface surface = GetSurfaceFromScreenCapture();
            TimeSpan timestamp = GetCurrentTime();

            // Create a media stream sample 
            MediaStreamSample sample = MediaStreamSample.CreateFromDirect3D11Surface(surface, timestamp);
            sample.OnProcessed += MSS_SampleProcessed;

            // Complete the sample request 
            args.Request.Sample = sample;
        }

        private void MSS_SampleProcessed(MediaStreamSample sample)
        {
            // Get the surface from the processed sample so that it can be reused 
            IDirect3DSurface surface = sample.Direct3D11Surface;

            // Allow the surface to be reused in the next call to GetSurfaceFromScreenCapture() 
            // (Implementation details not shown) 
        }*/

    }
}
