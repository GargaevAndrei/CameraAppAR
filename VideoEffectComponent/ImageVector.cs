//using CameraCOT;

using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Text;
using System;
using Windows.Foundation;
using Windows.UI;

namespace VideoEffectComponent
{
    class ImageVector
    {
        public static int x = 10;
        public static int y = 800;
        public static int widtch = 200;
        public static int height = 200;

        public static int x_offset = x + widtch / 2;
        public static int y_offset = y + height / 2;

        static double X, Y, Z;
        static int x1, y1, z1, x2;
        static int x1_s, x2_s, y1_s, y2_s;
        static int x_k, y_k, z_k;
        static double x_zero, y_zero, z_zero, alpha_zero;
        static double R = 60, R1xy, R1yz, R1xz;
        static double alpha, l_strelka = 15, alpha_s = Math.PI / 4;

        static int x_mod, y_mod;
        static int x2_s_mod, y2_s_mod;
        static int x1_s_mod, y1_s_mod;

        public static bool bOne;



        public static void ParseXYZ_Zero(string coordinateXYZ)
        {
            var tempCoordinateXYZ = coordinateXYZ.Split(';');
            x_zero = Convert.ToDouble(tempCoordinateXYZ[0]);
            y_zero = Convert.ToDouble(tempCoordinateXYZ[1]);
            z_zero = Convert.ToDouble(tempCoordinateXYZ[2]);
        }
       
        public static void ParseXYZ(string coordinateXYZ)
        {
            var tempCoordinateXYZ = coordinateXYZ.Split(';');
            X = Convert.ToDouble(tempCoordinateXYZ[0]);
            Y = Convert.ToDouble(tempCoordinateXYZ[1]);
            Z = Convert.ToDouble(tempCoordinateXYZ[2]);
        }

        public static void DrawRectangle(CanvasDrawingSession ds)
        {
            Rect rec = new Rect(ImageVector.x, ImageVector.y, ImageVector.widtch, ImageVector.height);
            ds.DrawRectangle(rec, Colors.Beige);
            ds.DrawLine(x, y + height / 2, x + widtch, y + height / 2, Colors.Beige);
            ds.DrawLine(x + widtch / 2, y, x + widtch / 2, y + height, Colors.Beige);

        }

        public static void DefineAlphaZero()
        {
            if (y_zero != 0)
            {

                if (y_zero < 0)
                    alpha_zero = Math.Atan(z_zero / (-y_zero)) + Math.PI / 2;

                if (y_zero > 0)
                    alpha_zero = Math.Atan(z_zero / (-y_zero)) - Math.PI / 2;
            }
        }
        
        public static void DefineAlpha()
        {
            if (Y < 0)
                alpha = Math.Atan(Z / (-Y)) - alpha_zero;
            if (Y > 0)
                alpha = Math.Atan(Z / (-Y)) - alpha_zero + Math.PI;

            y_k = (int)Math.Round(R * 0.7 * Math.Cos(alpha));
            z_k = (int)Math.Round(R * 0.7 * Math.Sin(alpha));
        }

        public static void DefineArrowCoordinate(bool flag)
        {
            if (flag)
            {
                x1_s = (int)Math.Round(l_strelka * Math.Cos(alpha + alpha_s));
                x2_s = (int)Math.Round(l_strelka * Math.Cos(alpha - alpha_s));
                y1_s = (int)Math.Round(l_strelka * Math.Sin(alpha + alpha_s));
                y2_s = (int)Math.Round(l_strelka * Math.Sin(alpha - alpha_s));
            }
            else
            {
                x1_s = (int)Math.Round(l_strelka * Math.Cos(alpha + alpha_s));
                y1_s = (int)Math.Round(l_strelka * Math.Sin(alpha + alpha_s));
                x2_s = (int)Math.Round(l_strelka * Math.Cos(alpha - alpha_s));
                y2_s = (int)Math.Round(l_strelka * Math.Sin(alpha - alpha_s));

            }
        }

        public static void DrawVerticalVector(CanvasDrawingSession ds, Color color)
        {
            ds.DrawLine(x_offset, y_offset, x_offset + x_mod, y_offset + y_mod,
                        color, 4);

            ds.DrawLine(x_offset + x_mod, y_offset + y_mod, x_offset + x_mod + x1_s_mod, y_offset + y_mod + y1_s_mod,
                        color, 4);
            ds.DrawLine(x_offset + x_mod, y_offset + y_mod, x_offset + x_mod + x2_s_mod, y_offset + y_mod + y2_s_mod,
                        color, 4);
        }

        public static void RotateVector(int x1, int y1, out int x2, out int y2, double Angle)
        {
            x2 = (int)Math.Round(x1 * Math.Cos(Angle) + y1 * Math.Sin(Angle));          //Math.PI / 4
            y2 = (int)Math.Round(-x1 * Math.Sin(Angle) + y1 * Math.Cos(Angle));
        }

        public static void DrawVerticalMode(CanvasDrawingSession ds)
        {
            x1 = (int)Math.Round((X - x_zero) * 20);
            y1 = (int)Math.Round( Y * 20);
            z1 = (int)Math.Round( Z * 20);

            if (x1 > 0)
            {
                RotateVector(-y1, z1, out x_mod, out y_mod, alpha_zero);

                x1_s_mod = -x1_s;
                y1_s_mod = -y1_s;
                x2_s_mod = -x2_s;
                y2_s_mod = -y2_s;

                DrawVerticalVector(ds, Colors.Red);

            }
            else
            {

                RotateVector(y1, -z1, out x_mod, out y_mod, alpha_zero);

                /*double alpha_temp = 0; if(x_mod != 0)
                alpha_temp = 360 * Math.Atan((double)(-y_mod) / (x_mod) ) / (2*Math.PI);                          

                ds.DrawText(alpha_temp.ToString(), 200, 200, Colors.Cyan, new CanvasTextFormat
                {   FontSize = videoEffectSettings.FontSize, });  */


                x1_s_mod = x1_s;
                y1_s_mod = y1_s;
                x2_s_mod = x2_s;
                y2_s_mod = y2_s;

                DrawVerticalVector(ds, Colors.Yellow);
            }
        }
    
        public static void DrawHorizontalMode(CanvasDrawingSession ds)
        {
            R1xz = Math.Sqrt(Z * Z + X * X);
            if (R1xz != 0)
            {
                x2 = (int)(R * ((X - x_zero) / R1xz));
                double betta = Math.Asin((float)-x2 / R);
                x1 = (int)Math.Round(x2 * 0.7);
                z1 = (int)Math.Round(R * 0.7 * Math.Cos(alpha + Math.PI / 2));
                y1 = (int)Math.Round(R * 0.7 * Math.Sin(alpha + Math.PI / 2));


                if (x1 < 0)
                {
                    ds.DrawLine(x_offset, y_offset + x1, x_offset - z1, y_offset + x1 + y1,
                                Colors.Yellow, 4);

                    ds.DrawLine(x_offset, y_offset + x1, x_offset + z1, y_offset + x1 - y1,
                                Colors.Yellow, 4);


                    // перпендикуляр

                    ds.DrawLine(x_offset, y_offset + x1, x_offset - y_k, y_offset + x1 + z_k,
                                Colors.Yellow, 4);
                    // стрелка
                    ds.DrawLine(x_offset - y_k, y_offset + x1 + z_k,
                                x_offset - y_k + x1_s, y_offset + x1 + z_k - y1_s,
                                Colors.Yellow, 4);
                    ds.DrawLine(x_offset - y_k, y_offset + x1 + z_k,
                               x_offset - y_k + x2_s, y_offset + x1 + z_k - y2_s,
                               Colors.Yellow, 4);

                }
                else
                {

                    ds.DrawLine(x_offset, y_offset + x1, x_offset - z1, y_offset + x1 + y1,
                                Colors.Cyan, 4);

                    ds.DrawLine(x_offset, y_offset + x1, x_offset + z1, y_offset + x1 - y1,
                                Colors.Cyan, 4);


                    // перпендикуляр

                    ds.DrawLine(x_offset, y_offset + x1, x_offset - y_k, y_offset + x1 + z_k,
                                Colors.Cyan, 4);
                    // стрелка
                    ds.DrawLine(x_offset - y_k, y_offset + x1 + z_k,
                                x_offset - y_k + x1_s, y_offset + x1 + z_k - y1_s,
                                Colors.Cyan, 4);
                    ds.DrawLine(x_offset - y_k, y_offset + x1 + z_k,
                               x_offset - y_k + x2_s, y_offset + x1 + z_k - y2_s,
                               Colors.Cyan, 4);

                }

                ds.DrawText(String.Format("крен = {0:0.0}", 360 * (alpha - Math.PI) / (2 * Math.PI)),
                            10, 650, Colors.Cyan, new CanvasTextFormat { FontSize = videoEffectSettings.FontSize, });

                ds.DrawText(String.Format("крен = {0:0.0}", 360 * (alpha - Math.PI) / (2 * Math.PI)),
                            10, 700, Colors.Cyan, new CanvasTextFormat { FontSize = videoEffectSettings.FontSize, });

                if (Math.Abs(betta) < 0.785)
                    ds.DrawText(String.Format("тангаж = {0:0.0}", 360 * (betta) / (2 * Math.PI)),
                                10, 740, Colors.Cyan, new CanvasTextFormat { FontSize = videoEffectSettings.FontSize, });

            }
        }
    }
}
