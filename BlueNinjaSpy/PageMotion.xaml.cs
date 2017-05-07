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
using Windows.UI.Xaml.Shapes;
using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;

namespace BlueNinjaSpy
{
    /// <summary>
    /// "Motion" Page
    /// </summary>
    public sealed partial class PageMotion : Page
    {
        public PageMotion()
        {
            this.InitializeComponent();
        }

        private async void Page_Loading(FrameworkElement sender, object args)
        {
            if (App.BlueNinja.CharMotion == null)
            {
                Debug.WriteLine("No connection");
                textBlockRaw.Text = "No connection";
                return;
            }
            App.BlueNinja.CharMotion.ValueChanged += theCharactValueChanged;
            var ret = await App.BlueNinja.CharMotion.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.Notify);
            if (ret != GattCommunicationStatus.Success)
            {
                Debug.WriteLine("WriteClientCharacteristicConfigurationDescriptorAsync() failed");
                textBlockRaw.Text = "WriteClientCharacteristicConfigurationDescriptorAsync() failed";
                return;
            }

            g1 = new Graph(Grid1);
            g2 = new Graph(Grid2);
            g3 = new Graph(Grid3);

            Yaw = new NumericalIntegral(0.5);
            Pitch = new NumericalIntegral(0.5);
            Roll = new NumericalIntegral(0.5);
        }

        class xyz16
        {
            public xyz16(byte[] d, int s)
            {
                x = (Int16)((d[s + 1] << 8) | d[s]);
                y = (Int16)((d[s + 3] << 8) | d[s + 2]);
                z = (Int16)((d[s + 5] << 8) | d[s + 4]);
            }
            public override string ToString()
            {
                string str = "(";
                str += x.ToString() + ",";
                str += y.ToString() + ",";
                str += z.ToString() + ")";
                return str;
            }
            public string ToCsv()
            {
                string str = x.ToString() + ",";
                str += y.ToString() + ",";
                str += z.ToString();
                return str;
            }
            public Int16 x { get; set; }
            public Int16 y { get; set; }
            public Int16 z { get; set; }
        }

        class NinjaData
        {
            public NinjaData(byte[] d, int s)
            {
                Gyro = new xyz16(d, s);
                Acc = new xyz16(d, s + 6);
                Mag = new xyz16(d, s + 12);
            }
            public override string ToString()
            {
                string str = "Gyro:" + Gyro.ToString();
                str += " Acc:" + Acc.ToString();
                str += " Mag:" + Mag.ToString();
                return str;
            }
            public string ToCsv()
            {
                string str = Gyro.ToCsv() + ",";
                str += Acc.ToCsv() + ",";
                str += Mag.ToCsv();
                return str;
            }
            public xyz16 Gyro { get; set; }
            public xyz16 Acc { get; set; }
            public xyz16 Mag { get; set; }
        }

        class NumericalIntegral
        {
            public NumericalIntegral(double DeltaT)
            {
                x = 0.0;
                dx = 0.0;
                dt = DeltaT;
                dx_l = new List<double>();
            }
            public int ZeroDrift(double _x)
            {
                x = 0.0;
                dx_l.Add(_x);
                dx = dx_l.Average();
                return dx_l.Count;
            }
            public void Integral(double _x)
            {
                x += ((_x - dx) * dt);
                while(x < -180.0)
                {
                    x += 360.0;
                }
                while(x >= 180.0)
                {
                    x -= 360.0;
                }
            }

            public double x { get; set; }
            private List<double> dx_l;
            public double dx { get; set; }
            private double dt;
        }

        static Graph g1, g2, g3;
        static NumericalIntegral Yaw, Pitch, Roll;

        private async void theCharactValueChanged(GattCharacteristic sender, GattValueChangedEventArgs eventArgs)
        {
            byte[] byteData = new byte[eventArgs.CharacteristicValue.Length];
            DataReader.FromBuffer(eventArgs.CharacteristicValue).ReadBytes(byteData);

            NinjaData ninja2 = new NinjaData(byteData, 0);
            NinjaData ninja1 = new NinjaData(byteData, 18);
            Debug.WriteLine(ninja2.ToString());
            Debug.WriteLine(ninja1.ToString());

            if (App.CsvFile != null)
            {
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, eventArgs.Timestamp.ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss.fff"));
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, ",");
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, ninja2.ToCsv());
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, "\r\n");
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, eventArgs.Timestamp.ToLocalTime().ToString("yyyy/MM/dd hh:mm:ss.fff"));
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, ",");
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, ninja1.ToCsv());
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, "\r\n");
            }

            string str = ninja2.ToString() + "\n" + ninja1.ToString();
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                textBlockRaw.Text = str;

                if(toggleSwitchZeroReset.IsOn)
                {
                    Pitch.ZeroDrift((double)ninja2.Gyro.x / 16.4);
                    Pitch.ZeroDrift((double)ninja1.Gyro.x / 16.4);
                    Roll.ZeroDrift((double)ninja2.Gyro.y / 16.4);
                    Roll.ZeroDrift((double)ninja1.Gyro.y / 16.4);
                    Yaw.ZeroDrift((double)ninja2.Gyro.z / 16.4);
                    Yaw.ZeroDrift((double)ninja1.Gyro.z / 16.4);
                }
                else
                {
                    Pitch.Integral((double)ninja2.Gyro.x / 16.4);
                    Pitch.Integral((double)ninja1.Gyro.x / 16.4);
                    Roll.Integral((double)ninja2.Gyro.y / 16.4);
                    Roll.Integral((double)ninja1.Gyro.y / 16.4);
                    Yaw.Integral((double)ninja2.Gyro.z / 16.4);
                    Yaw.Integral((double)ninja1.Gyro.z / 16.4);
                }

                g1.Add(Pitch.x);
                g2.Add(Roll.x);
                g3.Add(Yaw.x);
            });
        }

        public class Graph
        {
            private int MarginTopBottom = 10;
            private int MarginLeft = 10;
            private int MarginRight = 10;
            private int Width = 512;
            private int Num = 64;
            private int Height = 208;

            public Graph(Grid grid)
            {
                g = grid;
                datalist = new List<double>();
            }

            public void Add(double d)
            {
                datalist.Add(d);
                if (datalist.Count > Num)
                {
                    datalist.RemoveAt(0);
                }

                var polygon1 = new Polygon();
                polygon1.Fill = new SolidColorBrush(Windows.UI.Colors.DeepSkyBlue);

                var rect = new PointCollection();
                rect.Add(new Windows.Foundation.Point(0, 0));
                rect.Add(new Windows.Foundation.Point(Width, 0));
                rect.Add(new Windows.Foundation.Point(Width, Height));
                rect.Add(new Windows.Foundation.Point(0, Height));
                polygon1.Points = rect;

                g.Children.Add(polygon1);

                var polyline1 = new Polyline();
                polyline1.Stroke = new SolidColorBrush(Windows.UI.Colors.Blue);
                polyline1.StrokeThickness = 2;

                var points = new PointCollection();
                for (int i = 0; i < datalist.Count; i++)
                {
                    double x = MarginLeft + (i * (Width - MarginLeft - MarginRight) / Num);
                    double y = Height / 2 - (datalist[i] / 180.0 * (Height / 2));
                    if (y < MarginTopBottom)
                    {
                        y = MarginTopBottom;
                    }
                    if (y > Height - 2 * MarginTopBottom)
                    {
                        y = Height - 2 * MarginTopBottom;
                    }
                    points.Add(new Windows.Foundation.Point(x, y));
                }
                polyline1.Points = points;

                g.Children.Add(polyline1);

            }
            private Grid g;
            List<double> datalist;
        }

        private async void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            App.BlueNinja.CharMotion.ValueChanged -= theCharactValueChanged;
            var ret = await App.BlueNinja.CharMotion.WriteClientCharacteristicConfigurationDescriptorAsync(GattClientCharacteristicConfigurationDescriptorValue.None);
        }
    }
}
