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


        TvTunerSettings _tvTunerSettings;
        VideoCaptureWpf _videoCaptureWpf;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            _tvTunerSettings = new TvTunerSettings();
            _tvTunerSettings.Channel = 2;
            _tvTunerSettings.ChannelList.Add(2);
            _tvTunerSettings.ChannelList.Add(7);
            _tvTunerSettings.ChannelList.Add(9);
            _tvTunerSettings.ChannelList.Add(17);
            _tvTunerSettings.ChannelList.Add(19);
            _tvTunerSettings.ChannelList.Add(27);
            _tvTunerSettings.ChannelList.Add(29);
            _tvTunerSettings.Volume = 50;
            _tvTunerSettings.VideoDeviceName = "USB TV Device";
            _tvTunerSettings.VideoResolution = "720 x 480a";
            _tvTunerSettings.VideoInput = "0: VideoTunera";

            _videoCaptureWpf = new VideoCaptureWpf();
            _videoCaptureWpf.DeviceName = _tvTunerSettings.VideoDeviceName;
            _videoCaptureWpf.VideoResolution = _tvTunerSettings.VideoResolution;
            _videoCaptureWpf.VideoInput = _tvTunerSettings.VideoInput;
            _videoCaptureWpf.Volume = _tvTunerSettings.Volume;
            _videoCaptureWpf.BindImageControl(imgTvTuner);
            _videoCaptureWpf.BindImageControl(imgTvTuner2);
            _videoCaptureWpf.Start();

            var wTvTunerController = new wTvTunerController(_tvTunerSettings);
            wTvTunerController.ChannelChanged += WTvTunerController_ChannelChanged;
            wTvTunerController.VolumeChanged += WTvTunerController_VolumeChanged;
            wTvTunerController.VideoInputChanged += WTvTunerController_VideoInputChanged;
            wTvTunerController.Show();
        }

        private void WTvTunerController_VideoInputChanged(object sender, wTvTunerController.VideoInputChangedEventArgs e)
        {
            _videoCaptureWpf.Stop();
            _videoCaptureWpf.DeviceName = e.VideoDevice;
            _videoCaptureWpf.VideoResolution = e.VideoResolution;
            _videoCaptureWpf.VideoInput = e.VideoInput;
            _videoCaptureWpf.Start();
        }

        private void WTvTunerController_VolumeChanged(object sender, wTvTunerController.VolumeChangedEventArgs e)
        {
            _videoCaptureWpf.Volume = e.Volume;
        }

        private void WTvTunerController_ChannelChanged(object sender, wTvTunerController.ChannelChangedEventArgs e)
        {
            
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _videoCaptureWpf.Dispose();
        }
    }
}
