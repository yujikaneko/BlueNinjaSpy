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

using Windows.Devices.Bluetooth;
using Windows.Devices.Bluetooth.Advertisement;
using Windows.Devices.Bluetooth.GenericAttributeProfile;
using Windows.Storage.Streams;
using Windows.UI.Xaml.Shapes;

namespace BlueNinjaSpy
{
    /// <summary>
    /// "Main" Page
    /// has 2 area: Left side menu bar and right side frame.
    /// 
    /// menu bar has each service of HyouRouGan and Setting button, implemented as radio buttons.
    /// When the radio button is clicked, right side frame is navigated to each child page,
    /// PageGPIO, PagePWM, PageMotion, PageAirPressure or PageSetting
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            RadioButton_Motion.IsChecked = true;
        }

        private void RadioButton_GPIO_Checked(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(PageGPIO));
        }

        private void RadioButton_PWM_Checked(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(PagePWM));
        }

        private void RadioButton_Motion_Checked(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(PageMotion));
        }

        private void RadioButton_AirPressure_Checked(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(PageAirPressure));
        }

        private void RadioButton_Setting_Checked(object sender, RoutedEventArgs e)
        {
            MainContentFrame.Navigate(typeof(PageSetting));
        }
    }
}
