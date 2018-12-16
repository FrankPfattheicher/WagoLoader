using System;
using System.Collections.Generic;

namespace WagoLoader.Wago
{
    //[Serializable]
    public class DeviceInfo
    {
        public string OrderNumber { get; private set; }
        public string Description { get; private set; }
        public string SerialNumber { get; private set; }
        public string SoftwareVersion { get; private set; }
        public string HardwareVersion { get; private set; }
        public string FirmwareLoaderVersion { get; private set; }
        public string BaudRate { get; private set; }
        public string FirmwareBurnDate { get; private set; }
        public string ProductSerialNumber { get; private set; }
        public string QsString { get; private set; }


        /// <summary>
        /// Parses device info query response.
        /// </summary>
        /// <param name="text"></param>
        /// <returns>Instance of device info</returns>
        public static DeviceInfo Parse(string text)
        {
            /*
            ORDER=750-8202/0025-0002;
            DESCR=WAGO 750-8202 PFC200 2ETH RS Tele T ECO;
            SN=SN20180307T201337-1101305#PFC|0030DE4279FC;
            SW-VER=02.07.07(10);
            HW-VER=04;
            FWL-VER=2014.11.0-pXc-02.01.05 IDX=03;
            BAUD=100MBaud;
            FW-BURN-DATE=0000;
            PSN=750-8202/0025-0002;
            QS-STRING=0000;
             */
            var dict = new Dictionary<string, string>();
            foreach (var property in text.Split(";", StringSplitOptions.RemoveEmptyEntries))
            {
                var keyValue = property.Split("=");
                dict.Add(keyValue[0], keyValue[1]);
            }

            var di = new DeviceInfo
            {
                OrderNumber = dict["ORDER"],
                Description = dict["DESCR"],
                SerialNumber = dict["SN"],
                SoftwareVersion = dict["SW-VER"],
                HardwareVersion = dict["HW-VER"],
                FirmwareLoaderVersion = dict["FWL-VER"],
                BaudRate = dict["BAUD"],
                FirmwareBurnDate = dict["FW-BURN-DATE"],
                ProductSerialNumber = dict["PSN"],
                QsString = dict["QS-STRING"]
            };
            return di;
        }

        public override string ToString()
        {
            return $"{Description} - v{SoftwareVersion}";
        }
    }
}
