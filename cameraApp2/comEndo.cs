using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VideoEffectComponent;
using Windows.Storage.Streams;

namespace CameraCOT
{
    public class ComEndo
    {
        SerialPort serialPortEndo;
        double Xf, Yf, Zf;
        bool bCalibration;
        byte[] bufferTx = new byte[65];
        static bool laser;

        public ComEndo()
        {
            bufferTx[0] = 0x00;
            bufferTx[1] = 0xaa;
            bufferTx[2] = 0xbb;
            bufferTx[3] = 0x00;
            bufferTx[4] = 0x06;

            serialPortEndo = new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
            serialPortEndo.DataReceived += SerialPortEndo_DataReceived;

            if (serialPortEndo != null)
            {
                try
                {
                    serialPortEndo.Open();

                    serialPortEndo.Write("GAAZ");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        private void SerialPortEndo_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            byte[] data = new byte[8];
            try
            {
                serialPort.Read(data, 0, 8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            double x, y, z;

            short Xaccel = (short)((((short)(data[1]) << 2 | (short)(data[0]) >> 6)) << 6);
            short Yaccel = (short)((((short)(data[3]) << 2 | (short)(data[2]) >> 6)) << 6);
            short Zaccel = (short)((((short)(data[5]) << 2 | (short)(data[4]) >> 6)) << 6);

            double Xf1 = (double)Xaccel / 4096;
            double Yf1 = (double)Yaccel / 4096;
            double Zf1 = (double)Zaccel / 4096;

            x = Xf1;
            y = Yf1;
            z = Zf1;

            double k = 0.2;

            if (bCalibration)   // true
            {
                bCalibration = false;
                videoEffectSettings.coordinate_zero = x.ToString() + ";" + y.ToString() + ";" + z.ToString();
                //endoTimer.Start();
            }


            Xf = k * x + (1 - k) * Xf;
            Yf = k * y + (1 - k) * Yf;
            Zf = k * z + (1 - k) * Zf;

            //Xf = Xf1;
            //Yf = Yf1;
            //Zf = Zf1;

            //await Dispatcher.RunAsync(CoreDispatcherPriority.High, () => textBoxInfo.Text = String.Format("x = {0} y = {1} z = {2}", Xf, Yf, Zf) );
            //videoEffectSettings.coordinate = "\n x = " + Xf.ToString() + "\n y = " + Yf.ToString() + "\n z = " + Zf.ToString();
            videoEffectSettings.coordinate = Xf.ToString() + ";" + Yf.ToString() + ";" + Zf.ToString();

            serialPortEndo.DiscardInBuffer();
        }

        private void StartMeasure()
        {
            if (serialPortEndo != null)
            {
                bufferTx[5] = 23;                           // start measure
                serialPortEndo.Write(bufferTx, 0, 6);
            }
        }

        private void StopMeasure()
        {
            //lenghtMeterTimer.Stop();
            if (serialPortEndo != null)
            {
                bufferTx[5] = 33;                          // stop measure
                serialPortEndo.Write(bufferTx, 0, 6);
            }
        }

        private void GetDistance()
        {
            if (serialPortEndo != null)
            {
                bufferTx[5] = 28;                             // get distance
                serialPortEndo.Write(bufferTx, 0, 6);
            }
        }

        private void getAccel()
        {
            if (serialPortEndo != null)
            {               
                bufferTx[5] = 32;                            // get accel data               
                serialPortEndo.Write(bufferTx, 0, 6);
            }
        }
        
        private void LaserOn()
        {

            bufferTx[5] = 56;       // start measure
            bufferTx[6] = (byte)(laser ? 0 : 1);
            laser = !laser;
            serialPortEndo.Write(bufferTx, 0, 7);

        }



    }
}
