using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;

namespace BlueNinjaSpy
{
    /// <summary>
    /// Gatt agent for HyouRouGan @ BlueNinja
    /// TODO: other service
    /// </summary>
    public class BlueNinjaGatt
    {
        /// <summary>
        /// callback function for finishing connect to BlueNinja
        /// </summary>
        /// <param name="isConnected"></param>
        public delegate void FinishConnect(bool isConnected);

        Guid UUID_Service_Motion_sensor = new Guid("00050000-6727-11e5-988e-f07959ddcdfb");
        Guid UUID_Characteristic_Motion_sensor = new Guid("00050001-6727-11e5-988e-f07959ddcdfb");

        /// <summary>
        /// Characteristic: Motion Sensor
        /// </summary>
        public GattCharacteristic CharMotion { get; set; }

        /// <summary>
        /// Connect to BlueNinja
        /// </summary>
        /// <param name="BTAddr">Bluetooth address</param>
        /// <param name="func">FinishConnect callback</param>
        public async void Connect(ulong BTAddr, FinishConnect func)
        {
            BluetoothLEDevice device = await BluetoothLEDevice.FromBluetoothAddressAsync(BTAddr, BluetoothAddressType.Unspecified);
            if (device == null)
            {
                Debug.WriteLine("FromBluetoothAddressAsync() == null");
                func(false);
                return;
            }
            Debug.WriteLine("Device found: {" + device.Name + "}");

            GattDeviceServicesResult gatt = await device.GetGattServicesAsync();
            if (gatt == null)
            {
                Debug.WriteLine("GetGattServicesAsync() == null");
                func(false);
                return;
            }
            if (gatt.Services == null)
            {
                Debug.WriteLine("gatt.Services == null");
                func(false);
                return;
            }
            Debug.WriteLine("Services found: {" + gatt.Services.Count.ToString() + "}");

            GattDeviceService theService = null;
            foreach (var srv in gatt.Services)
            {
                Debug.WriteLine("uuid: {" + srv.Uuid.ToString() + "}");
                if (srv.Uuid == UUID_Service_Motion_sensor)
                {
                    theService = srv;
                    break;
                }
            }
            if (theService == null)
            {
                Debug.WriteLine("No service");
                func(false);
                return;
            }

            var chars = await theService.GetCharacteristicsAsync();
            if (chars == null)
            {
                Debug.WriteLine("GetCharacteristicsAsync() == null");
                func(false);
                return;
            }
            if (chars.Characteristics == null)
            {
                Debug.WriteLine("chars.Characteristics == null");
                func(false);
                return;
            }
            if (chars.Characteristics.Count == 0)
            {
                Debug.WriteLine("chars.Characteristics.Count == 0");
                func(false);
                return;
            }

            foreach (var charac in chars.Characteristics)
            {
                Debug.WriteLine("uuid:" + charac.Uuid.ToString());
                if (charac.Uuid == UUID_Characteristic_Motion_sensor)
                {
                    Debug.WriteLine("NINJA Characteristics found!");
                    CharMotion = charac;
                    break;
                }
            }
            func(CharMotion != null);
        }
    }
}
