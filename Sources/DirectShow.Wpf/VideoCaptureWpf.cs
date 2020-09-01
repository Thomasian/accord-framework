using Accord.Imaging;
using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DirectShow.Wpf
{
    public class VideoCaptureWpf : IDisposable
    {

        private VideoCaptureDevice _videoCaptureDevice;

        #region Properties

        public string _deviceName;
        public string DeviceName
        {
            get { return _deviceName; }
            set
            {
                if (_deviceName != value)
                {
                    _deviceName = value;
                    if (_isStarted)
                    {
                        Stop();
                        Start();
                    }
                }
            }
        }

        public string _videoResolution;
        public string VideoResolution
        {
            get { return _videoResolution; }
            set
            {
                if (_videoResolution != value)
                {
                    _videoResolution = value;
                    if (_isStarted)
                    {
                        Stop();
                        Start();
                    }
                }
            }
        }

        public string _videoInput;
        public string VideoInput
        {
            get { return _videoInput; }
            set
            {
                if (_videoInput != value)
                {
                    _videoInput = value;
                    if (_isStarted)
                    {
                        Stop();
                        Start();
                    }
                }
            }
        }

        private int _volume = 0;
        public int Volume
        {
            get { return _volume; }
            set
            {
                _volume = value;
                SetVolume();
            }
        }

        private bool _isStarted;
        public bool IsStarted
        {
            get { return _isStarted; }
        }

        #endregion

        public VideoCaptureWpf()
        {
            //RefreshSupportedFrameSizes(videoCaptureDevice);
            //VideoCapabilities caps = videoCapabilitiesDictionary[videoResolutions.Last()];
            //videoCaptureDevice.VideoResolution = caps;
            //videoCaptureDevice.CrossbarVideoInput = availableVideoInputs[1];
            //var captureSize = caps.FrameSize;
        }

        private void VideoCaptureDevice_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            if (_isStarted)
                return;

            Stop();
            _videoCaptureDevice = new VideoCaptureDevice(DeviceName);

            var resolution = _videoCaptureDevice.VideoCapabilities.First(c => GetVideoResolutionString(c) == VideoResolution);
            if (resolution != null)
                _videoCaptureDevice.VideoResolution = resolution;

            var input = _videoCaptureDevice.AvailableCrossbarVideoInputs.First(i => GetVideoInputString(i) == VideoInput);
            if (input != null)
                _videoCaptureDevice.CrossbarVideoInput = input;

            _videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            _videoCaptureDevice.NewFrameArray += VideoCaptureDevice_NewFrameArray; ;
            _videoCaptureDevice.VideoSourceError += VideoCaptureDevice_VideoSourceError;

            SetVolume();
            _videoCaptureDevice.Start();

            _isStarted = true;
        }

        public void Stop()
        {
            if (_videoCaptureDevice != null)
            {
                _videoCaptureDevice.NewFrame -= VideoCaptureDevice_NewFrame;
                _videoCaptureDevice.NewFrameArray -= VideoCaptureDevice_NewFrameArray; ;
                _videoCaptureDevice.VideoSourceError -= VideoCaptureDevice_VideoSourceError;
                try
                {
                    _videoCaptureDevice.SignalToStop();
                }
                catch { }
                _videoCaptureDevice = null;
            }

            _isStarted = false;
        }

        private void SetVolume()
        {
            _videoCaptureDevice?.SetLinearVolume(Volume);
        }

        #region Capture Display

        private System.Windows.Controls.Image _image;
        private System.Windows.Controls.Image Image
        {
            get { return _image; }
            set
            {
                Stop();
                _image = value;
            }
        }

        private void VideoCaptureDevice_NewFrameArray(object sender, NewFrameArrayEventArgs eventArgs)
        {
            Image?.Dispatcher.Invoke(() =>
            {
                UpdateBitmapSource(eventArgs.Pixels, eventArgs.PixelWidth, eventArgs.PixelHeight);
            });
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            Image?.Dispatcher.Invoke(() =>
            {
                UpdateBitmapSource(eventArgs.Frame);
            });
        }

        private void UpdateBitmapSource(System.Drawing.Bitmap bitmap)
        {
            if (_image == null)
                return;

            lock (_image)
            {
                try
                {
                    var bitmapData = bitmap.LockBits(
                        new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                        System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

                    var writableBitmap = Image.Source as WriteableBitmap;
                    if (Image.Source == null || Image.Source.Width != bitmapData.Width || Image.Source.Height != bitmapData.Height)
                    {
                        writableBitmap = new WriteableBitmap(bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null);
                        Image.Source = writableBitmap;
                    }

                    writableBitmap.WritePixels(
                        new Int32Rect(0, 0, bitmapData.Width, bitmapData.Height),
                        bitmapData.Scan0,
                        bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                    bitmap.UnlockBits(bitmapData);
                }
                catch { }
            }
        }
        private void UpdateBitmapSource(byte[] pixels, int width, int height)
        {
            if (_image == null)
                return;

            lock (_image)
            {
                try
                {
                    var writableBitmap = Image.Source as WriteableBitmap;
                    if (Image.Source == null || Image.Source.Width != width || Image.Source.Height != height)
                    {
                        writableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
                        Image.Source = writableBitmap;
                    }

                    writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * (PixelFormats.Bgr24.BitsPerPixel / 8), 0);
                }
                catch { }
            }
        }

        public void Dispose()
        {
            Stop();
        }

        #endregion

        #region Static

        public static List<VideoInputDevice> GetVideoInputDevices()
        {
            return new FilterInfoCollection(FilterCategory.VideoInputDevice).Select(item => new VideoInputDevice(item.MonikerString, item.Name)).ToList();
        }

        public static SupportedValues GetSupportedValues(string deviceMoniker)
        {
            var videoDevice = new VideoCaptureDevice(deviceMoniker);

            var resolutions = videoDevice.VideoCapabilities.Select(item => GetVideoResolutionString(item)).ToList();
            if (resolutions.Count == 0)
                resolutions.Add("Not supported");

            var videoInputs = videoDevice.AvailableCrossbarVideoInputs.Select(item => GetVideoInputString(item)).ToList();
            if (videoInputs.Count == 0)
                videoInputs.Add("Not supported");


            return new SupportedValues( resolutions, videoInputs);
        }

        private static string GetVideoResolutionString(VideoCapabilities capability)
        {
            return string.Format("{0} x {1}", capability.FrameSize.Width, capability.FrameSize.Height);
        }

        private static string GetVideoInputString(VideoInput videoInput)
        {
            return string.Format("{0}: {1}", videoInput.Index, videoInput.Type);
        }

        public sealed class VideoInputDevice
        {
            public VideoInputDevice(string deviceMoniker, string name)
            {
                DeviceMoniker = deviceMoniker;
                Name = name;
            }

            public string DeviceMoniker { get; }
            public string Name { get; }
        }

        public sealed class SupportedValues
        {
            public SupportedValues(List<string> videoResolutions, List<string> videoInputs)
            {
                VideoResolutions = videoResolutions;
                VideoInputs = videoInputs;
            }

            public List<string> VideoResolutions { get; }
            public List<string> VideoInputs { get; }
        }

        #endregion
    }
}
