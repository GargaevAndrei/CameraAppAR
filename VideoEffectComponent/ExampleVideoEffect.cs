using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.Effects;
using Windows.Media.MediaProperties;
using Windows.Foundation.Collections;
using Windows.Graphics.DirectX.Direct3D11;
using Windows.Graphics.Imaging;
using System.Runtime.InteropServices;
using Microsoft.Graphics.Canvas.Effects;
using Microsoft.Graphics.Canvas;
using Windows.UI;
using Microsoft.Graphics.Canvas.Text;

//using CameraCOT;




/*[ComImport]
[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]


unsafe interface IMemoryBufferByteAccess
{
    void GetBuffer(out byte* buffer, out uint capacity);
}*/


namespace VideoEffectComponent
{
    public struct videoEffectSettings
    {
        public static string lenght;
        public static string coordinate;
        public static string commet;
        public static int X;
        public static int Y;
        public static int FontSize;
        public static bool getLenghtFlag;
    }
    public sealed class ExampleVideoEffect : IBasicVideoEffect
    {        

        public void Close(MediaEffectClosedReason reason)
        {

        }

        private int frameCount;
        public void DiscardQueuedFrames()
        {
            frameCount = 0;
        }


        public bool IsReadOnly { get { return false; } }

        private VideoEncodingProperties encodingProperties;
        //public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        //{
        //    this.encodingProperties = encodingProperties;
        //}

        private CanvasDevice canvasDevice;
        public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)   
        {
            canvasDevice = CanvasDevice.CreateFromDirect3D11Device(device);
        }

        public IReadOnlyList<VideoEncodingProperties> SupportedEncodingProperties
        {
            get
            {
                var encodingProperties = new VideoEncodingProperties();
                encodingProperties.Subtype = "ARGB32";
                return new List<VideoEncodingProperties>() { encodingProperties };
            }
        }



        //public MediaMemoryTypes SupportedMemoryTypes { get { return MediaMemoryTypes.Cpu; } }
        public MediaMemoryTypes SupportedMemoryTypes { get { return MediaMemoryTypes.Gpu; } }


        public bool TimeIndependent { get { return true; } }
        

        private IPropertySet configuration;

        public void SetProperties(IPropertySet configuration)
        {
            this.configuration = configuration;
        }

        public double FadeValue
        {
            get
            {
                object val;
                if (configuration != null && configuration.TryGetValue("FadeValue", out val))
                {
                    return (double)val;
                }
                return .5;
            }
        }

        //public string LenghtValue 
        //{ 
        //    get
        //    {
        //        object val;
        //        if (configuration != null && configuration.TryGetValue("LenghtValut", out val))
        //        {
        //            return (string)val;
        //        }
        //        return "test";
        //    }

        //}



        public double BlurAmount
        {
            get
            {
                object val;
                if (configuration != null && configuration.TryGetValue("BlurAmount", out val))
                {
                    return (double)val;
                }
                return 3;
            }
        }


        public void ProcessFrame(ProcessVideoFrameContext context)
        {

            using (CanvasBitmap inputBitmap = CanvasBitmap.CreateFromDirect3D11Surface(canvasDevice, context.InputFrame.Direct3DSurface))
            using (CanvasRenderTarget renderTarget = CanvasRenderTarget.CreateFromDirect3D11Surface(canvasDevice, context.OutputFrame.Direct3DSurface))
            using (CanvasDrawingSession ds = renderTarget.CreateDrawingSession())
            {

                ds.DrawImage(inputBitmap); //gaussianBlurEffect

                if (videoEffectSettings.getLenghtFlag && videoEffectSettings.lenght != null)
                {
                    ds.DrawText(videoEffectSettings.lenght, videoEffectSettings.X, videoEffectSettings.Y, Colors.PaleTurquoise, new CanvasTextFormat
                    {
                        FontSize = videoEffectSettings.FontSize,
                        FontWeight = Windows.UI.Text.FontWeights.Bold
                    });
                }

                //ds.DrawText(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"), videoEffectSettings.X, videoEffectSettings.Y + 50, Colors.PaleTurquoise, new CanvasTextFormat
                //{
                //    FontSize = videoEffectSettings.FontSize,
                //    FontWeight = Windows.UI.Text.FontWeights.Bold
                //});


                if (videoEffectSettings.coordinate != null)
                {
                    ds.DrawText(videoEffectSettings.coordinate, 200, 200, Colors.PaleTurquoise, new CanvasTextFormat
                    {
                        FontSize = 44,
                        FontWeight = Windows.UI.Text.FontWeights.Bold
                    });
                }

                if (videoEffectSettings.commet != null)
                {
                    ds.DrawText(videoEffectSettings.commet, 50, 2200, Colors.PaleTurquoise, new CanvasTextFormat
                    {
                        FontSize = 88,

                    });
                }


                // Rect rect = new Rect(190, 700, 600, 600);
                // ds.DrawRectangle(rect, Colors.Chartreuse);

            }
        }

        


    }
}
