using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Devices.HumanInterfaceDevice;
using Windows.Storage;
using Windows.Storage.Streams;

namespace CameraCOT
{
    public class HidEndo
    {
        public static HidDevice device;
        bool _isHidExist;
        string HidInfo;
        static bool light1;

        public async void EnumerateHidDevices()
        {

            ushort vendorId  = 0x0483;      // 0x045E;
            ushort productId = 0x5750;      // 0x07CD;
            ushort usagePage = 0xff00;      // 0x000D;
            ushort usageId   = 0x0001;

            string selector = HidDevice.GetDeviceSelector(usagePage, usageId, vendorId, productId);
            var devices = await DeviceInformation.FindAllAsync(selector);

            if (devices.Any())
            {
                HidInfo = "\nHID devices found: " + devices.Count + Environment.NewLine;
                _isHidExist = true;
                
                device = await HidDevice.FromIdAsync(devices.ElementAt(0).Id, FileAccessMode.ReadWrite);

                //readHid();
            }
            else           
                HidInfo = "\nHID device not found" + Environment.NewLine;
            
        }

        public async void LizerOn()
        {
            if (device != null)
            {
                HidOutputReport outReport = device.CreateOutputReport();

                //byte[] buffer = new byte[] { 10, 20, 30, 40 };

                byte[] bufferTx = new byte[65];
                bufferTx[0] = 0x00;
                bufferTx[1] = 0xaa;
                bufferTx[2] = 0xbb;
                bufferTx[3] = 0x00;
                bufferTx[4] = 0x07;
                bufferTx[5] = 56;       // switch on lazer targer
                bufferTx[6] = (byte)(light1 ? 1 : 0);
                light1 = !light1;


                DataWriter dataWriter = new DataWriter();
                dataWriter.WriteBytes(bufferTx);

                outReport.Data = dataWriter.DetachBuffer();

                await device.SendOutputReportAsync(outReport);

                //await Task.Delay(3000);
                try
                {

                    HidInputReport inReport = await device.GetInputReportAsync();

                    if (inReport != null)
                    {
                        UInt16 id = inReport.Id;
                        var bytes = new byte[4];
                        DataReader dataReader = DataReader.FromBuffer(inReport.Data);
                        dataReader.ReadBytes(bytes);
                    }
                    else
                    {
                        HidInfo = "Invalid input report received";
                    }
                }
                catch(Exception ex)
                {
                    HidInfo = ex.ToString();
                }
            }
            else
            {
                HidInfo = "device is NULL";
            }
        }
    }
}
