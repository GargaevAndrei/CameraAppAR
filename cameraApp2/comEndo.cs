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
    abstract public class ComEndo
    {
        protected MainPage page;
        protected SerialPort serialPortEndo; //= new SerialPort("COM4", 9600, Parity.None, 8, StopBits.One);
        protected double Xf, Yf, Zf;
        protected bool bCalibration;
        protected static bool laser;

        public ComEndo(string EndoComName, MainPage mainPage)
        {
            page = mainPage;
            
            serialPortEndo = new SerialPort(EndoComName, 9600, Parity.None, 8, StopBits.One);           

            if (serialPortEndo != null)
            {
                try
                {
                    serialPortEndo.Open();
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }        

        public void SerialPortClose()
        {
            
                try
                {
                    serialPortEndo.Close();
                    serialPortEndo.DiscardInBuffer();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            
    }

        public void SerialPortOpen()
        {
            if (serialPortEndo != null && !serialPortEndo.IsOpen)
            {
                try
                {
                    serialPortEndo.Open();
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }
        }

        public virtual void BrightnessSet(int light) { }
        public virtual void StartMeasure() { }
        public virtual void StopMeasure() { }
        public virtual void GetDistance() { }
        public virtual void getAccel() { }
        public virtual void EndoDiameterSet(float diameter) { }
        public virtual void LaserToggle() { }
        public virtual void LaserOn() { }
        public virtual void LaserOff() { }
        public virtual bool isLaserSet() { return laser ? true : false; }

    }

    class ComEndoHead1 : ComEndo
    {
        public ComEndoHead1(string EndoComName, MainPage mainPage) : base(EndoComName, mainPage)
        {
            serialPortEndo.DataReceived += SerialPortEndo_DataReceived;
        }

        public void SerialPortEndo_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            byte[] ByteArray = new byte[8];
            try
            {
                serialPort.Read(ByteArray, 0, 8);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

            double x, y, z;

            short Xaccel = (short)((((short)(ByteArray[1]) << 2 | (short)(ByteArray[0]) >> 6)) << 6);
            short Yaccel = (short)((((short)(ByteArray[3]) << 2 | (short)(ByteArray[2]) >> 6)) << 6);
            short Zaccel = (short)((((short)(ByteArray[5]) << 2 | (short)(ByteArray[4]) >> 6)) << 6);

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

        public override void BrightnessSet(int light)
        {
            try
            {

                byte[] s = new byte[4];
                s[0] = 65; //'A';     //65
                s[1] = (byte)((light >> 8));
                s[2] = (byte)((light) & 0xff);
                s[3] = 90; // 'Z';     //90

                if (!serialPortEndo.IsOpen)
                    serialPortEndo.Open();


                serialPortEndo.Write(s, 0, 4);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }

        public override void getAccel()
        {
            try
            {

                byte[] s = new byte[4];
                s[0] = 66; //'B';     //66
                s[1] = 0;
                s[2] = 0;
                s[3] = 90; // 'Z';     //90

                if (!serialPortEndo.IsOpen)
                    serialPortEndo.Open();


                serialPortEndo.Write(s, 0, 4);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }

        }
    }

    class ComEndoHead2 : ComEndo
    {
        byte[] bufferTx = new byte[65];

        public ComEndoHead2 (string EndoComName, MainPage mainPage) : base(EndoComName, mainPage)
        {
            bufferTx[0] = 0xaa;
            bufferTx[1] = 0xbb;
            bufferTx[2] = 0x00;
            bufferTx[3] = 0x06;

            serialPortEndo.DataReceived += SerialPortEndo_DataReceived;
        }

        public void SerialPortEndo_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var serialPort = (SerialPort)sender;
            byte[] ByteArray = new byte[64];
            double x, y, z;

            try
            {
                serialPort.Read(ByteArray, 0, 64);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }


            if (ByteArray[4] == 25)     // AMP_FIND_SUCCESS
            {
                //MainPage.watchDogEndoTimer.Stop();
                page.watchDogEndoTimerStop();
                MainPage._isLenght = true;
                //MainPage.lenghtMeterTimer.Start();
                //MainPage.lenghtRunMeterTimer.Start();
                //MainPage.endoTimer.Stop();


                videoEffectSettings.getLenghtFlag = true;

                page.UpdateUI();
            }

            if (ByteArray[5] == 35 && MainPage._isLenght == false)  // AMP_FIND_NOT_SUCCESS
            {
                MainPage._isLenght = false;
                videoEffectSettings.getLenghtFlag = false;
                MainPage._findLenghtZero = false;
                page.UpdateUI();

                if (serialPortEndo != null && serialPortEndo.IsOpen)
                {
                    bufferTx[4] = 33;                          // stop measure
                    serialPortEndo.Write(bufferTx, 0, 6);
                }



            }

            if (ByteArray[4] == 28)    // GET_DISTANCE
            {
                byte[] bytes = new byte[4];

                bytes[0] = (byte)(ByteArray[5]);
                bytes[1] = (byte)(ByteArray[6]);
                bytes[2] = (byte)(ByteArray[7]);
                bytes[3] = (byte)(ByteArray[8]);

                double value = BitConverter.ToSingle(bytes, 0);


                videoEffectSettings.lenght = "L = " + value.ToString("0.00") + " м";
                //videoEffectSettings.coordinate = "\n x = " + Xf.ToString() + "\n y = " + Yf.ToString() + "\n z = " + Zf.ToString();
                //videoEffectSettings.coordinate = Xf.ToString() + ";" + Yf.ToString() + ";" + Zf.ToString();


            }

            if (ByteArray[4] == 32)     // MSG_GET_ACCEL_DATA
            {
                short Xaccel = (short)((((short)(ByteArray[6]) << 2 | (short)(ByteArray[5]) >> 6)) << 6);
                short Yaccel = (short)((((short)(ByteArray[8]) << 2 | (short)(ByteArray[7]) >> 6)) << 6);
                short Zaccel = (short)((((short)(ByteArray[10]) << 2 | (short)(ByteArray[9]) >> 6)) << 6);

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
        }

        public override void BrightnessSet(int light)
        {
             bufferTx[3] = 0x07;
             bufferTx[4] = 31;       // start measure            
             bufferTx[5] = (byte)(light >> 8);
             bufferTx[6] = (byte)(light & 0xff);

             if (serialPortEndo != null && serialPortEndo.IsOpen)
             {
                 try
                 {
                     //serialPortEndo.Open();

                     serialPortEndo.Write(bufferTx, 0, 64);
                 }
                 catch (Exception e)
                 {
                     Debug.WriteLine(e.Message);
                 }
             }            
        }


        public override void StartMeasure()
        {

            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    bufferTx[3] = 0x06;
                    bufferTx[4] = 23;                           // start measure
                    bufferTx[5] = 0;
                    bufferTx[6] = 0;
                    bufferTx[7] = 0;
                    serialPortEndo.Write(bufferTx, 0, 6);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public override void StopMeasure()
        {
            //lenghtMeterTimer.Stop();
            bufferTx[3] = 0x06;
            bufferTx[5] = 0;
            bufferTx[6] = 0;
            bufferTx[7] = 0;
            page.UpdateUI();
            MainPage._isLenght = false;
            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    bufferTx[4] = 33;                          // stop measure
                    serialPortEndo.Write(bufferTx, 0, 6);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public override void GetDistance()
        {
            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    bufferTx[4] = 28;                             // get distance
                    serialPortEndo.Write(bufferTx, 0, 6);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public override void getAccel()
        {
            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    bufferTx[4] = 32;                            // get accel data               
                    serialPortEndo.Write(bufferTx, 0, 6);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }

        public override void EndoDiameterSet(float diameter)
        {
            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    byte[] vOut = BitConverter.GetBytes(diameter);
                    bufferTx[4] = vOut[0];
                    bufferTx[5] = vOut[1];
                    bufferTx[6] = vOut[2];
                    bufferTx[7] = vOut[3];
                    serialPortEndo.Write(bufferTx, 0, 8);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }
        }

        public override void LaserToggle()
        {

            bufferTx[4] = 56;       // start measure
            bufferTx[5] = (byte)(laser ? 0 : 1);


            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    //serialPortEndo.Open();
                    laser = !laser;
                    serialPortEndo.Write(bufferTx, 0, 64);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }

        public override void LaserOn()
        {

            bufferTx[4] = 56;       // start measure
            bufferTx[5] = 1;


            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    serialPortEndo.Write(bufferTx, 0, 64);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }

        public override void LaserOff()
        {

            bufferTx[4] = 56;       // start measure
            bufferTx[5] = 0;


            if (serialPortEndo != null && serialPortEndo.IsOpen)
            {
                try
                {
                    serialPortEndo.Write(bufferTx, 0, 64);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

        }

    }
}
