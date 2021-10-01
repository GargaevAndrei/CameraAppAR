using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Security.Cryptography;
using Windows.Storage.Streams;
using Windows.UI.Core;

namespace cameraApp2
{
    public static class HidEndo
    {
        static HidDevice device;
        static int count = 0;
        public static async void EnumerateHidDevices()
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
                _isHidExist = true;

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

        public static async void sendOutReport(DataWriter dataWriter)
        {
            HidOutputReport outReport = device.CreateOutputReport(0x00);
            outReport.Data = dataWriter.DetachBuffer();



            //refreshCameraTimer.Tick += refreshCameraTimer_Tick;
            device.InputReportReceived += async (sender, args) =>
            {
                HidInputReport inputReport = args.Report;
                IBuffer buffer = inputReport.Data;

                byte[] ByteArray;
                CryptographicBuffer.CopyToByteArray(buffer, out ByteArray);

                await Dispatcher.RunAsync(CoreDispatcherPriority.High, new DispatchedHandler(() =>
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

                        videoEffectSettings.lenght = "L = " + value.ToString("0.00") + " м";
                        //videoEffectSettings.coordinate = "\n x = " + Xf.ToString() + "\n y = " + Yf.ToString() + "\n z = " + Zf.ToString();
                        videoEffectSettings.coordinate = Xf.ToString() + ";" + Yf.ToString() + ";" + Zf.ToString();


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
                        videoEffectSettings.coordinate = Xf.ToString() + ";" + Yf.ToString() + ";" + Zf.ToString();
                    }

                }));
            };

            // Send the output report asynchronously
            await device.SendOutputReportAsync(outReport);

        }

        public static void readHid()
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

        public static void startMeasure()
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

        public static void stopMeasure()
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

        public static void getDistance()
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

        public static void getAccel()
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
    }
}
