using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

using System.Diagnostics;
using System.Collections.ObjectModel;

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BlueNinjaSpy
{
    /// <summary>
    /// "Connect" Page
    /// </summary>
    public sealed partial class PageConnect : Page
    {
        /// <summary>
        /// Found Bluetooth LE devices to be listed
        /// </summary>
        class BLEDevice
        {
            public BLEDevice(BluetoothLEAdvertisementReceivedEventArgs btAdv)
            {
                BluetoothAddress = btAdv.BluetoothAddress;
                LocalName = btAdv.Advertisement.LocalName;
                RawSignalStrengthInDBm = btAdv.RawSignalStrengthInDBm;
            }
            public override string ToString()
            {
                string str = LocalName + " : ";
                str += BluetoothAddress.ToString("X") + " : ";
                str += RawSignalStrengthInDBm.ToString() + "dBm";
                return str;
            }
            public ulong BluetoothAddress { get; set; }
            public string LocalName { get; set; }
            public short RawSignalStrengthInDBm { get; set; }
        }

        static ObservableCollection<BLEDevice> BLEDevices;
        static BluetoothLEAdvertisementWatcher BleWatcher;
        private ContentDialog msg;

        /// <summary>
        /// Constructor
        /// </summary>
        public PageConnect()
        {
            this.InitializeComponent();

            BLEDevices = new ObservableCollection<BLEDevice>();
            listViewNinja.ItemsSource = BLEDevices;

            BleWatcher = new BluetoothLEAdvertisementWatcher();
            BleWatcher.ScanningMode = BluetoothLEScanningMode.Active;
            BleWatcher.AdvertisementFilter.Advertisement.ServiceUuids.Add(GattServiceUuids.GenericAccess);
            BleWatcher.Received += Watcher_Received;
        }

        /// <summary>
        /// when the page is loaded
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            BleWatcher.Start();
        }

        /// <summary>
        /// callback when receiving advertisement
        /// </summary>
        /// <param name="w"></param>
        /// <param name="btAdv"></param>
        private async void Watcher_Received(BluetoothLEAdvertisementWatcher w, BluetoothLEAdvertisementReceivedEventArgs btAdv)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                bool exist = false;
                foreach (BLEDevice dev in BLEDevices)
                {
                    if (btAdv.BluetoothAddress == dev.BluetoothAddress)
                    {
                        // already found, update RSSI
                        int id = BLEDevices.IndexOf(dev);
                        BLEDevice newdev = new BLEDevice(btAdv);
                        BLEDevices[id] = newdev;
                        exist = true;
                        break;
                    }
                }
                if (!exist)
                {
                    // found a new one
                    BLEDevice dev = new BLEDevice(btAdv);
                    BLEDevices.Add(dev);
                }
            });
        }

        /// <summary>
        /// When BLE device is clicked
        /// TODO: error code when connecting is failed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void listViewNinja_ItemClick(object sender, ItemClickEventArgs e)
        {
            listViewNinja.IsEnabled = false;
            BleWatcher.Stop();

            BLEDevice dev = (BLEDevice)e.ClickedItem;
            if(dev == null)
            {
                listViewNinja.IsEnabled = true;
                BleWatcher.Start();
                return;
            }
            ulong BTAddr = dev.BluetoothAddress;
            App.BlueNinja.Connect(BTAddr, ConnectFinished);

            msg = new ContentDialog();
            msg.Title = "Connecting...";
            msg.Content = "Connecting to " + dev.LocalName + "...";
            await msg.ShowAsync();
        }

        /// <summary>
        /// callback when connected.
        /// </summary>
        /// <param name="isConnected"></param>
        public async void ConnectFinished(bool isConnected)
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                msg.Hide();
                if (isConnected)
                {
                    // move to MainPage
                    RootFrame.Navigate(typeof(MainPage));
                }
                else
                {
                    listViewNinja.IsEnabled = true;
                    BleWatcher.Start();
                }
            });
        }
    }
}
