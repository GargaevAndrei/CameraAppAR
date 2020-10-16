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
using Windows.Foundation;




/*[ComImport]
[Guid("5B0D3235-4DBA-4D44-865E-8F1D0E4FD04D")]
[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]


unsafe interface IMemoryBufferByteAccess
{
    void GetBuffer(out byte* buffer, out uint capacity);
}*/


namespace VideoEffectComponent
{
    public struct tempValue
    {
        public static string lenght;
        public static string coordinate;
        
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
        /*public void SetEncodingProperties(VideoEncodingProperties encodingProperties, IDirect3DDevice device)
        {
            this.encodingProperties = encodingProperties;
        }*/

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

       /* public unsafe void ProcessFrame(ProcessVideoFrameContext context)
        {
            using (BitmapBuffer buffer = context.InputFrame.SoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Read))
            using (BitmapBuffer targetBuffer = context.OutputFrame.SoftwareBitmap.LockBuffer(BitmapBufferAccessMode.Write))
            {
                using (var reference = buffer.CreateReference())
                using (var targetReference = targetBuffer.CreateReference())
                {
                    byte* dataInBytes;
                    uint capacity;
                    ((IMemoryBufferByteAccess)reference).GetBuffer(out dataInBytes, out capacity);

                    byte* targetDataInBytes;
                    uint targetCapacity;
                    ((IMemoryBufferByteAccess)targetReference).GetBuffer(out targetDataInBytes, out targetCapacity);

                    var fadeValue = FadeValue;

                    // Fill-in the BGRA plane
                    BitmapPlaneDescription bufferLayout = buffer.GetPlaneDescription(0);
                    for (int i = 0; i < bufferLayout.Height; i++)
                    {
                        for (int j = 0; j < bufferLayout.Width; j++)
                        {

                            byte value = (byte)((float)j / bufferLayout.Width * 255);

                            int bytesPerPixel = 4;
                            if (encodingProperties.Subtype != "ARGB32")
                            {
                                // If you support other encodings, adjust index into the buffer accordingly
                            }


                            int idx = bufferLayout.StartIndex + bufferLayout.Stride * i + bytesPerPixel * j;

                            targetDataInBytes[idx + 0] = (byte)(fadeValue * (float)dataInBytes[idx + 0]);
                            targetDataInBytes[idx + 1] = (byte)(fadeValue * (float)dataInBytes[idx + 1]);
                            targetDataInBytes[idx + 2] = (byte)(fadeValue * (float)dataInBytes[idx + 2]);
                            targetDataInBytes[idx + 3] = dataInBytes[idx + 3];
                        }
                    }
                }
            }
        }*/



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


                var gaussianBlurEffect = new GaussianBlurEffect
                {
                    Source = inputBitmap,
                    BlurAmount = (float)BlurAmount,
                    Optimization = EffectOptimization.Speed
                };

                ds.DrawImage(inputBitmap); //gaussianBlurEffect
                ds.DrawText("Augmented reality", 100, 400, Colors.Aqua, new CanvasTextFormat
                {
                    FontSize = 24,
                    FontWeight = Windows.UI.Text.FontWeights.Bold
                });
                ds.DrawText(DateTime.Now.ToString("dd MMM yyyy HH:mm:ss"), 100, 430, Colors.Aquamarine, new CanvasTextFormat
                {
                    FontSize = 20,
                    FontWeight = Windows.UI.Text.FontWeights.Bold
                });

                ds.DrawText(tempValue.lenght, 100, 460, Colors.Aquamarine, new CanvasTextFormat
                {
                    FontSize = 20,
                    FontWeight = Windows.UI.Text.FontWeights.Bold
                });

                ds.DrawText(tempValue.coordinate, 100, 490, Colors.Aquamarine, new CanvasTextFormat
                {
                    FontSize = 20,
                    FontWeight = Windows.UI.Text.FontWeights.Bold
                });

                Rect rect = new Rect(95, 400, 300, 200);
                ds.DrawRectangle(rect, Colors.Chartreuse);

            }
        }


    }
}
