using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace DirectShow.Wpf
{
    /// <summary>
    /// Interaction logic for wTvTunerController.xaml
    /// </summary>
    public partial class wTvTunerController : Window
    {

        public bool IsModified { get; set; }

        public TvTunerSettings TvTunerSettings { get; }

        public wTvTunerController(TvTunerSettings tvTunerSettings)
        {
            InitializeComponent();

            udVolume.Minimum = TvTunerSettings.MinVolume;
            udVolume.Maximum = TvTunerSettings.MaxVolume;
            sldVolume.Minimum = TvTunerSettings.MinVolume;
            sldVolume.Maximum = TvTunerSettings.MaxVolume;
            udChannel.Minimum = TvTunerSettings.MinChannel;
            udChannel.Maximum = TvTunerSettings.MaxChannel;

            TvTunerSettings = tvTunerSettings;

            udChannel.Value = TvTunerSettings.Channel;
            lstChannels.ItemsSource = TvTunerSettings.ChannelList;
            udVolume.Value = TvTunerSettings.Volume;
            sldVolume.Value = TvTunerSettings.Volume;

            RefreshVideoDevicesAsync();
            IsModified = false;
        }

        #region Channels
        private void udChannel_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (udChannel.Value.HasValue && udChannel.Value != TvTunerSettings.Channel)
            {
                TvTunerSettings.Channel = udChannel.Value.Value;
                IsModified = true;
                OnChannelChanged();
            }
            CheckAllowAddChannel();
        }

        private void CheckAllowAddChannel()
        {
            if (!udChannel.Value.HasValue || TvTunerSettings.ChannelList.Contains(udChannel.Value.Value))
            {
                bttnAddChannel.IsHitTestVisible = false;
                faAddChannel.Foreground = Brushes.Gray;
            }
            else
            {
                bttnAddChannel.IsHitTestVisible = true;
                faAddChannel.Foreground = Brushes.Green;
            }
        }

        private void bttnAddChannel_Click(object sender, RoutedEventArgs e)
        {
            if (udChannel.Value.HasValue)
            {
                var channel = udChannel.Value.Value;
                if (!TvTunerSettings.ChannelList.Contains(channel))
                {
                    int idx;
                    for (idx = 0; idx < TvTunerSettings.ChannelList.Count; idx++)
                    {
                        if (TvTunerSettings.ChannelList[idx] > channel)
                            break;
                    }
                    TvTunerSettings.ChannelList.Insert(idx, channel);
                    IsModified = true;
                }
            }
            CheckAllowAddChannel();
        }

        private void GoToChannel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext.GetType() == typeof(Int32))
            {
                udChannel.Value = (Int32)button.DataContext;
            }
        }

        private void DeleteChannel_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext.GetType() == typeof(Int32))
            {
                TvTunerSettings.ChannelList.Remove((Int32)button.DataContext);
                CheckAllowAddChannel();
                IsModified = true;
            }
        }

        #endregion

        #region Volume

        private void udVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (udVolume.Value.HasValue && udVolume.Value != TvTunerSettings.Volume)
            {
                TvTunerSettings.Volume = udVolume.Value.Value;
                IsModified = true;
                sldVolume.Value = TvTunerSettings.Volume;
                OnVolumeChanged();
            }
            CheckAllowAddChannel();
        }

        private void sldVolume_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (sldVolume.Value != TvTunerSettings.Volume)
            {
                TvTunerSettings.Volume = Convert.ToInt32(sldVolume.Value);
                IsModified = true;
                udVolume.Value = TvTunerSettings.Volume;
                OnVolumeChanged();
            }
        }

        #endregion

        #region Advanced

        private void bttnRefreshDevices_Click(object sender, RoutedEventArgs e)
        {
            RefreshVideoDevicesAsync();
        }

        private async void RefreshVideoDevicesAsync()
        {
            var devices = await Task<List<VideoCaptureWpf.VideoInputDevice>>.Factory.StartNew(() => VideoCaptureWpf.GetVideoInputDevices());
            if (devices != null)
            {
                cboVideoDevice.ItemsSource = devices;
                cboVideoDevice.SelectedItem = devices.FirstOrDefault(d => d.Name == TvTunerSettings.VideoDeviceName);
            }
        }

        private void cboVideoDevice_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var selectedDevice = cboVideoDevice.SelectedItem as VideoCaptureWpf.VideoInputDevice;
            if (selectedDevice == null)
            {
                cboVideoResolution.ItemsSource = null;
                cboVideoInput.ItemsSource = null;
            }
            else
            {
                var supported = VideoCaptureWpf.GetSupportedValues(selectedDevice.DeviceMoniker);

                var resolustions = supported.VideoResolutions;
                cboVideoResolution.ItemsSource = resolustions;
                if (resolustions.Contains(TvTunerSettings.VideoResolution))
                    cboVideoResolution.SelectedItem = TvTunerSettings.VideoResolution;
                else if (resolustions.Count > 0)
                    cboVideoResolution.SelectedIndex = 0;


                var inputs = supported.VideoInputs;
                cboVideoInput.ItemsSource = inputs;
                if (inputs.Contains(TvTunerSettings.VideoInput))
                    cboVideoInput.SelectedItem = TvTunerSettings.VideoInput;
                else if (inputs.Count > 0)
                    cboVideoInput.SelectedIndex = 0;
            }

            bttnSave.IsEnabled = CheckAllowSaveVideoDevice();
        }

        private void cboVideoResolution_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bttnSave.IsEnabled = CheckAllowSaveVideoDevice();
        }

        private void cboVideoInput_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            bttnSave.IsEnabled = CheckAllowSaveVideoDevice();
        }

        private bool CheckAllowSaveVideoDevice()
        {
            if (cboVideoDevice.SelectedItem == null ||
                cboVideoResolution.SelectedItem == null ||
                cboVideoInput.SelectedItem == null)
                return false;

            var selectedDevice = (cboVideoDevice.SelectedItem as VideoCaptureWpf.VideoInputDevice)?.Name;
            var selectedResolution = cboVideoResolution.SelectedItem as string;
            var selectedInput = cboVideoInput.SelectedItem as string;

            return selectedDevice != TvTunerSettings.VideoDeviceName ||
                   selectedResolution != TvTunerSettings.VideoResolution ||
                   selectedInput != TvTunerSettings.VideoInput;
        }

        private void bttnSave_Click(object sender, RoutedEventArgs e)
        {
            if (CheckAllowSaveVideoDevice())
            {
                TvTunerSettings.VideoDeviceName = (cboVideoDevice.SelectedItem as VideoCaptureWpf.VideoInputDevice)?.Name;
                TvTunerSettings.VideoResolution = cboVideoResolution.SelectedItem as string;
                TvTunerSettings.VideoInput = cboVideoInput.SelectedItem as string;
                IsModified = true;
                OnVideoInputChanged();
            }
        }

        #endregion

        #region Events

        #region Channel Events

        public event EventHandler<ChannelChangedEventArgs> ChannelChanged;
        private void OnChannelChanged()
        {
            ChannelChanged?.Invoke(this, new ChannelChangedEventArgs(TvTunerSettings.Channel));
        }
        public class ChannelChangedEventArgs : EventArgs
        {
            internal ChannelChangedEventArgs(int channel)
            {
                Channel = channel;
            }

            public int Channel { get; }
        }

        #endregion

        #region Volume Events

        public event EventHandler<VolumeChangedEventArgs> VolumeChanged;
        private void OnVolumeChanged()
        {
            VolumeChanged?.Invoke(this, new VolumeChangedEventArgs(TvTunerSettings.Volume));
        }
        public class VolumeChangedEventArgs : EventArgs
        {
            internal VolumeChangedEventArgs(int volume)
            {
                Volume = volume;
            }

            public int Volume { get; }
        }

        #endregion

        #region VideoInput Events

        public event EventHandler<VideoInputChangedEventArgs> VideoInputChanged;
        private void OnVideoInputChanged()
        {
            VideoInputChanged?.Invoke(this,
                new VideoInputChangedEventArgs(TvTunerSettings.VideoDeviceName, TvTunerSettings.VideoResolution, TvTunerSettings.VideoInput));
        }
        public class VideoInputChangedEventArgs : EventArgs
        {
            internal VideoInputChangedEventArgs(string videoDevice, string videoResolution, string videoInput)
            {
                VideoDevice = videoDevice;
                VideoResolution = videoResolution;
                VideoInput = videoInput;
            }

            public string VideoDevice { get; }
            public string VideoResolution { get; }
            public string VideoInput { get; }
        }

        #endregion

        #endregion
    }
}
