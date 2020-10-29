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

// Документацию по шаблону элемента "Пустая страница" см. по адресу https://go.microsoft.com/fwlink/?LinkId=234238

namespace CameraCOT
{
    /// <summary>
    /// Пустая страница, которую можно использовать саму по себе или для перехода внутри фрейма.
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        public SettingsPage()
        {
            this.InitializeComponent();
        }

        private void MainCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MainPreviewSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MainPhotoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void MainVideoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoPreviewSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoPhotoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void TermoVideoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EndoCameraList_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EndoPreviewSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EndoPhotoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void EndoVideoSettings_Changed(object sender, SelectionChangedEventArgs e)
        {

        }

        private void getMain_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
