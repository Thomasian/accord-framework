using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace DirectShow.Wpf.Test
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cboVideoDevices.ItemsSource = VideoCaptureWpf.GetVideoInputDevices();
        }

        private void cboVideoDevices_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selected = cboVideoDevices.SelectedItem as VideoCaptureWpf.VideoInputDevice;
            var supported = VideoCaptureWpf.GetSupportedValues(selected.DeviceMoniker);
            cboVideoResolutions.ItemsSource = supported.VideoResolutions;
            cboVideoInputs.ItemsSource = supported.VideoInputs;
        }
    }
}
