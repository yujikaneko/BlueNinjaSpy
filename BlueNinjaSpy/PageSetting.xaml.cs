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

namespace BlueNinjaSpy
{
    /// <summary>
    /// "Setting" Page
    /// </summary>
    public sealed partial class PageSetting : Page
    {
        public PageSetting()
        {
            this.InitializeComponent();
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            var picker = new Windows.Storage.Pickers.FileSavePicker();
            picker.FileTypeChoices.Add("CSV File", new List<string>() { ".csv" });
            picker.SuggestedFileName = "testdata";

            App.CsvFile = await picker.PickSaveFileAsync();

            if (App.CsvFile != null)
            {
                Debug.WriteLine(App.CsvFile.Name);
                await Windows.Storage.FileIO.WriteTextAsync(App.CsvFile, "Time,Gyro,,,Acc,,,Mag,,\r\n");
                await Windows.Storage.FileIO.AppendTextAsync(App.CsvFile, "Time,x,y,z,x,y,z,x,y,z\r\n");
            }
            else
            {
                Debug.WriteLine("File selection canceled");
            }
        }

        private void TextBlock_SelectionChanged(object sender, RoutedEventArgs e)
        {
            Button_Click(sender, e);
        }
    }
}
