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
        public static string coordinate_zero;
        public static string commet;
        public static int X;
        public static int Y;
        public static int FontSize;
        public static bool getLenghtFlag;
        public static bool getCoordinateFlag;
        public static bool termo;
        public static double Tmin;
        public static double Tmax;
        public static double temperature;
        public static int XRect;
        public static int YRect;
        public static int widthRect;
        public static int heightRect;
        public static int indexBadPixel;
        public static bool bHorizont;

        /*public void SetParamNotes(int _x, int _y, int _fontSize, int _widthRect)
        {
            X = _x;
            Y = _y;
            FontSize = _fontSize;
            widthRect = _widthRect;
        }*/
    }

    static class ImageForVector
    {
        public static int x = 10;
        public static int y = 800;
        public static int widtch = 200;
        public static int height = 200;
    }

    //public static double temperature;
    public sealed class ExampleVideoEffect : IBasicVideoEffect
    {
        static int[] colormap_fusion = new int[] { 17, 18, 48, 11, 14, 33, 13, 16, 39, 15, 18, 44, 16, 19, 46, 18, 19, 53, 19, 20, 57, 21, 21, 62, 20, 22, 63, 16, 23, 72, 18, 24, 77, 23, 21, 79, 27, 24, 85, 30, 28, 94, 30, 30, 97, 41, 38, 118, 40, 37, 119, 41, 40, 124, 40, 41, 123, 41, 42, 126, 46, 40, 126, 50, 41, 127, 50, 41, 126, 52, 42, 126, 51, 44, 128, 53, 45, 131, 55, 46, 135, 57, 45, 133, 58, 47, 129, 59, 47, 133, 61, 46, 138, 65, 44, 137, 68, 45, 140, 68, 48, 142, 71, 46, 141, 74, 46, 141, 77, 45, 142, 81, 45, 143, 84, 44, 143, 86, 45, 144, 86, 47, 144, 87, 46, 142, 91, 46, 143, 93, 47, 144, 95, 44, 143, 99, 44, 145, 105, 44, 145, 105, 45, 145, 108, 45, 147, 109, 45, 145, 113, 43, 143, 115, 43, 144, 117, 43, 145, 120, 42, 144, 123, 40, 145, 124, 41, 146, 128, 41, 149, 130, 41, 148, 135, 39, 146, 136, 39, 147, 138, 39, 147, 142, 39, 148, 144, 39, 147, 145, 40, 143, 148, 39, 142, 151, 36, 143, 153, 35, 144, 155, 37, 145, 157, 37, 147, 160, 35, 147, 161, 34, 146, 162, 32, 145, 167, 31, 147, 169, 31, 147, 170, 30, 143, 171, 30, 142, 175, 31, 144, 177, 31, 145, 179, 28, 145, 178, 30, 143, 179, 28, 142, 184, 26, 144, 186, 26, 143, 186, 27, 141, 189, 25, 144, 191, 24, 146, 191, 24, 144, 190, 25, 143, 191, 23, 141, 194, 21, 142, 196, 22, 137, 197, 22, 135, 198, 23, 138, 198, 23, 137, 199, 23, 135, 200, 22, 127, 200, 21, 127, 206, 21, 128, 204, 24, 125, 202, 24, 122, 206, 25, 120, 207, 26, 120, 207, 26, 117, 209, 28, 118, 213, 28, 111, 213, 28, 107, 212, 30, 106, 211, 32, 105, 213, 35, 102, 215, 37, 100, 216, 39, 95, 216, 41, 89, 215, 45, 84, 220, 45, 84, 218, 46, 78, 218, 47, 76, 221, 49, 70, 222, 50, 68, 221, 54, 60, 223, 56, 56, 226, 57, 51, 226, 58, 48, 225, 59, 47, 225, 61, 47, 226, 63, 42, 228, 66, 39, 225, 68, 40, 224, 73, 40, 225, 76, 34, 228, 76, 34, 227, 77, 34, 228, 77, 38, 230, 79, 38, 230, 79, 36, 231, 82, 33, 232, 82, 33, 233, 84, 37, 233, 85, 36, 234, 87, 33, 235, 89, 34, 236, 90, 34, 237, 91, 35, 235, 94, 35, 236, 97, 29, 237, 98, 30, 239, 99, 27, 237, 101, 31, 236, 101, 31, 237, 102, 35, 239, 103, 37, 240, 105, 35, 241, 106, 36, 241, 109, 38, 240, 111, 38, 240, 111, 36, 240, 114, 36, 239, 116, 33, 241, 117, 34, 241, 117, 34, 240, 120, 34, 242, 121, 34, 246, 122, 32, 243, 124, 31, 243, 127, 30, 243, 127, 31, 242, 130, 32, 244, 131, 28, 247, 133, 29, 244, 136, 30, 244, 139, 33, 246, 138, 30, 246, 138, 28, 243, 143, 26, 243, 145, 26, 245, 146, 29, 245, 147, 31, 246, 146, 26, 252, 147, 28, 250, 151, 29, 248, 152, 29, 250, 153, 28, 249, 156, 26, 248, 159, 25, 251, 161, 27, 250, 161, 30, 250, 164, 29, 247, 167, 23, 248, 168, 23, 250, 167, 27, 249, 169, 26, 248, 175, 25, 251, 177, 24, 249, 180, 25, 248, 181, 23, 247, 181, 26, 247, 182, 26, 250, 183, 19, 252, 183, 18, 249, 188, 18, 248, 190, 17, 254, 190, 16, 254, 190, 13, 249, 195, 11, 250, 197, 11, 253, 195, 10, 253, 196, 10, 252, 199, 10, 251, 201, 6, 249, 205, 8, 248, 206, 3, 249, 205, 4, 250, 207, 3, 248, 209, 4, 247, 210, 3, 250, 210, 8, 250, 211, 8, 249, 215, 9, 250, 217, 13, 254, 216, 17, 252, 218, 17, 248, 220, 15, 251, 220, 17, 251, 221, 21, 251, 226, 17, 251, 223, 28, 253, 224, 40, 252, 227, 39, 251, 227, 41, 251, 229, 48, 250, 231, 53, 250, 232, 61, 250, 232, 67, 249, 235, 72, 250, 237, 80, 250, 236, 88, 251, 238, 96, 250, 240, 101, 252, 239, 107, 251, 240, 114, 251, 245, 128, 251, 243, 137, 251, 241, 150, 251, 244, 153, 251, 246, 162, 252, 245, 165, 253, 245, 172, 250, 247, 177, 250, 247, 186, 251, 246, 189, 253, 245, 195, 251, 247, 201, 252, 249, 209, 254, 249, 214, 253, 249, 224, 252, 250, 228, 255, 253, 238 };
        //static double[] colormap_gray = new double[colormap_fusion.Length / 3];
        int indexMin = 0;
        
        double err, errMin = 100000;

        int indexBadPixel;
        string[] coordinateXYZ = new string[3];
        string[] coordinateXYZ_zero = new string[3];
        double X, Y, Z;
        double xf, yf, zf;
        int x1, y1, z1, R, x2;
        int x1_s, x2_s, y1_s, y2_s;
        int x_k, y_k, z_k;
        double x_zero, y_zero, z_zero, alpha_zero;
        double R1xy, R1yz, R1xz;
        double alpha, l_strelka, alpha_s;
        bool bOne;
        //bool bHorizont = false;

        
        


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
                ds.DrawImage(inputBitmap);          

                if (videoEffectSettings.termo)  //
                {
                    //for (int i = 0, j = 0; i < colormap_fusion.Length - 2; i += 3, j++)
                    //    colormap_gray[j] = 0.2126 * colormap_fusion[i] + 0.7152 * colormap_fusion[i + 1] + 0.0722 * colormap_fusion[i + 2];

                    var buf = inputBitmap.GetPixelBytes();
                    var R = buf[9436];
                    var G = buf[9437];
                    var B = buf[9438];

                    //buf[9436] = 0;
                    //buf[9437] = 255;
                    //buf[9438] = 0;

                    /*indexBadPixel = videoEffectSettings.indexBadPixel;

                    buf[indexBadPixel + 0] = 255;
                    buf[indexBadPixel + 1] = 255;
                    buf[indexBadPixel + 2] = 255;

                    buf[indexBadPixel + 8 + 0] = (byte)((buf[indexBadPixel + 4 + 0] + buf[indexBadPixel + 12 + 0])/2);
                    buf[indexBadPixel + 8 + 1] = (byte)((buf[indexBadPixel + 4 + 1] + buf[indexBadPixel + 12 + 1])/2);
                    buf[indexBadPixel + 8 + 2] = (byte)((buf[indexBadPixel + 4 + 2] + buf[indexBadPixel + 12 + 2])/2);

                    buf[indexBadPixel + 16 + 0] = 255;
                    buf[indexBadPixel + 16 + 1] = 255;
                    buf[indexBadPixel + 16 + 2] = 255;*/

                    // Линейная интерполяция пикселей--------------

                    //---------------------------------------------

                    //var Y = 0.2126 * R + 0.7152 * G + 0.0722 * B;

                    inputBitmap.SetPixelBytes(buf);
                    ds.DrawImage(inputBitmap);

                    indexMin = 0;
                    errMin = 1000000;
                    err = 0;

                    for (int i = 0; i < 255; i++)
                    {
                        err = Math.Abs(colormap_fusion[i*3] - G) + Math.Abs(colormap_fusion[i*3 + 1] - R) + Math.Abs(colormap_fusion[i*3 + 2] - B);
                        //err = Math.Sqrt(  Math.Pow((colormap_fusion[i * 3] - G),2) + Math.Pow((colormap_fusion[i * 3 + 1] - R),2) + Math.Pow((colormap_fusion[i * 3 + 2] - B),2)  ) / 3;
                        //err = Math.Abs(colormap_gray[i] - Y);
                        if (err < errMin)
                        {
                            errMin = err;
                            indexMin = i;
                        }
                    }

                    videoEffectSettings.temperature = (videoEffectSettings.Tmax - videoEffectSettings.Tmin) / 255 * indexMin + videoEffectSettings.Tmin;


                }

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

                if (videoEffectSettings.coordinate_zero != null && !bOne)
                {
                    bOne = true;
                    coordinateXYZ_zero = videoEffectSettings.coordinate_zero.Split(';');
                    x_zero = Convert.ToDouble(coordinateXYZ_zero[0]);
                    y_zero = Convert.ToDouble(coordinateXYZ_zero[1]);
                    z_zero = Convert.ToDouble(coordinateXYZ_zero[2]);
                    videoEffectSettings.coordinate_zero = null;
                }

                if (videoEffectSettings.coordinate != null && videoEffectSettings.getCoordinateFlag)
                {
                    //ds.DrawText(videoEffectSettings.coordinate, 200, 200, Colors.PaleTurquoise, new CanvasTextFormat
                    //{
                    //    FontSize = 44,
                    //    FontWeight = Windows.UI.Text.FontWeights.Bold
                    //});

                 
                    coordinateXYZ = videoEffectSettings.coordinate.Split(';');
                    X = Convert.ToDouble(coordinateXYZ[0]);
                    Y = Convert.ToDouble(coordinateXYZ[1]);
                    Z = Convert.ToDouble(coordinateXYZ[2]);

                    xf = X;
                    yf = Y;
                    zf = Z;


                    Rect rec = new Rect(ImageForVector.x, ImageForVector.y, ImageForVector.widtch, ImageForVector.height);
                    ds.DrawRectangle(rec, Colors.Beige);
                    ds.DrawLine(ImageForVector.x, ImageForVector.y + ImageForVector.height/2, 
                                ImageForVector.x + ImageForVector.widtch, ImageForVector.y + ImageForVector.height / 2, Colors.Beige);
                    ds.DrawLine(ImageForVector.x + ImageForVector.widtch/2, ImageForVector.y ,
                                ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height, Colors.Beige);





                    R = 60;
                    alpha_s = Math.PI / 4;
                    l_strelka = 15;
                    //alpha_zero := -2.0196094001943;

                    if (y_zero != 0)
                    {

                        if (y_zero < 0)
                            alpha_zero = Math.PI / 2 + Math.Atan(z_zero / (-y_zero));

                        if (y_zero > 0)
                            alpha_zero = -Math.PI / 2 + Math.Atan(z_zero / (-y_zero));
                        
                    }


                    if (yf != 0) 
                    {
                        if (yf < 0) 
                            alpha = Math.Atan(zf / (-yf)) - alpha_zero;
                        if (yf > 0)
                            alpha = Math.Atan(zf / (-yf)) + Math.PI - alpha_zero;

                        y_k = (int)Math.Round(R * 0.7 * Math.Cos(alpha));
                        z_k = (int)Math.Round(R * 0.7 * Math.Sin(alpha));

                        if (videoEffectSettings.bHorizont)
                        {
                            x1_s = (int)Math.Round(l_strelka *  Math.Cos(alpha + alpha_s));
                            x2_s = (int)Math.Round(l_strelka *  Math.Cos(alpha + alpha_s - Math.PI / 2));
                            y1_s = (int)Math.Round(l_strelka *  Math.Sin(alpha + alpha_s));
                            y2_s = (int)Math.Round(l_strelka *  Math.Sin(alpha + alpha_s - Math.PI / 2));
                        }
                        else
                        {
                            x1_s = (int)Math.Round(l_strelka * Math.Cos(alpha + alpha_s + alpha_zero));
                            x2_s = (int)Math.Round(l_strelka * Math.Cos(alpha + alpha_s + alpha_zero - Math.PI / 2));
                            y1_s = (int)Math.Round(l_strelka * Math.Sin(alpha + alpha_s + alpha_zero));
                            y2_s = (int)Math.Round(l_strelka * Math.Sin(alpha + alpha_s + alpha_zero - Math.PI / 2));
                        }

                    }


                    if (!videoEffectSettings.bHorizont)    
                    {
                        x1 = (int)Math.Round((X - x_zero) * 15);
                        y1 = (int)Math.Round((Y) * 15);
                        z1 = (int)Math.Round((Z) * 15);
                        R =  (int)Math.Round(Math.Sqrt(y1 * y1 + z1 * z1));

                        if (x1 < 0)
                        {
                            ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2,
                                        ImageForVector.x + ImageForVector.widtch / 2 - y1, ImageForVector.y + ImageForVector.height / 2 + z1,
                                        Colors.Red, 4);

                            ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y1, ImageForVector.y + ImageForVector.height / 2 + z1,
                                        ImageForVector.x + ImageForVector.widtch / 2 - y1 - x1_s, ImageForVector.y + ImageForVector.height / 2 + z1 - y1_s,
                                        Colors.Red, 4);
                            ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y1, ImageForVector.y + ImageForVector.height / 2 + z1,
                                        ImageForVector.x + ImageForVector.widtch / 2 - y1 - x2_s, ImageForVector.y + ImageForVector.height / 2 + z1 - y2_s,
                                        Colors.Red, 4);

                        }
                        else
                        {
                            ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2,
                                        ImageForVector.x + ImageForVector.widtch / 2 + y1, ImageForVector.y + ImageForVector.height / 2 + z1,
                                        Colors.Yellow, 4);

                            ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y1, ImageForVector.y + ImageForVector.height / 2 + z1,
                                        ImageForVector.x + ImageForVector.widtch / 2 - y1 + x1_s, ImageForVector.y + ImageForVector.height / 2 + z1 - y1_s,
                                        Colors.Yellow, 4);
                            ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y1, ImageForVector.y + ImageForVector.height / 2 + z1,
                                        ImageForVector.x + ImageForVector.widtch / 2 - y1 + x2_s, ImageForVector.y + ImageForVector.height / 2 + z1 - y2_s,
                                        Colors.Yellow, 4);
                        }
                    }
                    else
                    {

                        R1xz = Math.Sqrt(zf * zf + xf * xf);
                        if (R1xz != 0)
                        {
                            x2 = (int)(R * ((xf - x_zero) / R1xz));
                            x1 = (int)Math.Round(x2 * 0.7);
                            z1 = (int)Math.Round(R * 0.7 * Math.Cos(alpha + Math.PI / 2));
                            y1 = (int)Math.Round(R * 0.7 * Math.Sin(alpha + Math.PI / 2));


                            if (x1 < 0)
                            {
                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2 + x1,
                                            ImageForVector.x + ImageForVector.widtch / 2 - z1, ImageForVector.y + ImageForVector.height / 2 + x1 + y1,
                                            Colors.Yellow, 4);

                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2 + x1,
                                            ImageForVector.x + ImageForVector.widtch / 2 + z1, ImageForVector.y + ImageForVector.height / 2 + x1 - y1,
                                            Colors.Yellow, 4);


                                // перпендикуляр

                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2 + x1,
                                            ImageForVector.x + ImageForVector.widtch / 2 - y_k, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k,
                                            Colors.Yellow, 4);
                                // стрелка
                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y_k, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k,
                                            ImageForVector.x + ImageForVector.widtch / 2 - y_k + x1_s, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k - y1_s,
                                            Colors.Yellow, 4);
                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y_k, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k,
                                           ImageForVector.x + ImageForVector.widtch / 2 - y_k + x2_s, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k - y2_s,
                                           Colors.Yellow, 4);

                            }
                            else
                            {

                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2 + x1,
                                            ImageForVector.x + ImageForVector.widtch / 2 - z1, ImageForVector.y + ImageForVector.height / 2 + x1 + y1,
                                            Colors.Cyan, 4);

                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2 + x1,
                                            ImageForVector.x + ImageForVector.widtch / 2 + z1, ImageForVector.y + ImageForVector.height / 2 + x1 - y1,
                                            Colors.Cyan, 4);


                                // перпендикуляр

                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2, ImageForVector.y + ImageForVector.height / 2 + x1,
                                            ImageForVector.x + ImageForVector.widtch / 2 - y_k, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k,
                                            Colors.Cyan, 4);
                                // стрелка
                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y_k, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k,
                                            ImageForVector.x + ImageForVector.widtch / 2 - y_k + x1_s, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k - y1_s,
                                            Colors.Cyan, 4);
                                ds.DrawLine(ImageForVector.x + ImageForVector.widtch / 2 - y_k, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k,
                                           ImageForVector.x + ImageForVector.widtch / 2 - y_k + x2_s, ImageForVector.y + ImageForVector.height / 2 + x1 + z_k - y2_s,
                                           Colors.Cyan, 4);

                            }
                        }

                    }

                }

                if (videoEffectSettings.commet != null)
                {
                    Rect rect = new Rect(videoEffectSettings.XRect, videoEffectSettings.YRect, videoEffectSettings.widthRect, 200);  // 2100    //1000
                    //ds.DrawText(videoEffectSettings.commet, 50, 2200, Colors.Cyan, new CanvasTextFormat
                    ds.DrawText(videoEffectSettings.commet, rect, Colors.Cyan, new CanvasTextFormat
                    {
                        FontSize = videoEffectSettings.FontSize,    //88   //44

                    });
                }
                

            }
        }

        


    }
}
