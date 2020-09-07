using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DirectShow.Wpf
{
    public class VideoCaptureWpf : IDisposable
    {

        private VideoCaptureDevice _videoCaptureDevice;

        #region Properties

        public string DeviceName { get; set; }
        public string VideoResolution { get; set; }
        public string VideoInput { get; set; }

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

        private int _channel = 1;
        public int Channel
        {
            get { return _channel; }
            set
            {
                _channel = value;
                SetChannel();
            }
        }

        public bool IsStarted { get; private set; }

        public bool IsPaused { get { return _videoCaptureDevice?.IsPaused ?? false; } }

        /// <summary>
        /// Video source error event.
        /// </summary>
        /// 
        /// <remarks>This event is used to notify clients about any type of errors occurred in
        /// video source object, for example internal exceptions.</remarks>
        /// 
        public event VideoSourceErrorEventHandler VideoSourceError;

        /// <summary>
        /// Video playing finished event.
        /// </summary>
        /// 
        /// <remarks><para>This event is used to notify clients that the video playing has finished.</para>
        /// </remarks>
        /// 
        public event PlayingFinishedEventHandler PlayingFinished;

        #endregion

        public VideoCaptureWpf()
        {
            //RefreshSupportedFrameSizes(videoCaptureDevice);
            //VideoCapabilities caps = videoCapabilitiesDictionary[videoResolutions.Last()];
            //videoCaptureDevice.VideoResolution = caps;
            //videoCaptureDevice.CrossbarVideoInput = availableVideoInputs[1];
            //var captureSize = caps.FrameSize;
        }

        public void Start()
        {
            if (IsStarted)
                return;

            Stop();

            var inputDevice = GetVideoInputDevice(DeviceName);
            if (inputDevice == null)
                throw new Exception($"Cannot find video input device '{DeviceName}'.");

            _videoCaptureDevice = new VideoCaptureDevice(inputDevice.MonikerString);

            var resolution = _videoCaptureDevice.VideoCapabilities.FirstOrDefault(c => GetVideoResolutionString(c) == VideoResolution);
            if (resolution != null)
                _videoCaptureDevice.VideoResolution = resolution;
            else if (_videoCaptureDevice.VideoCapabilities.Length > 0)
                _videoCaptureDevice.VideoResolution = _videoCaptureDevice.VideoCapabilities[0];

            var input = _videoCaptureDevice.AvailableCrossbarVideoInputs.FirstOrDefault(i => GetVideoInputString(i) == VideoInput);
            if (input != null)
                _videoCaptureDevice.CrossbarVideoInput = input;
            else if (_videoCaptureDevice.AvailableCrossbarVideoInputs.Length > 0)
                _videoCaptureDevice.CrossbarVideoInput = _videoCaptureDevice.AvailableCrossbarVideoInputs[0];

            _videoCaptureDevice.NewFrame += VideoCaptureDevice_NewFrame;
            _videoCaptureDevice.NewFrameArray += VideoCaptureDevice_NewFrameArray; ;
            _videoCaptureDevice.VideoSourceError += VideoCaptureDevice_VideoSourceError;
            _videoCaptureDevice.PlayingFinished += VideoCaptureDevice_PlayingFinished;

            SetVolume();
            SetChannel();
            _videoCaptureDevice.Start();

            IsStarted = true;
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
                    if (IsStarted)
                        _videoCaptureDevice.SignalToStop();
                }
                catch { }
                _videoCaptureDevice = null;
            }

            IsStarted = false;
        }

        public void Pause()
        {
            _videoCaptureDevice?.Pause();
        }

        public void Resume()
        {
            _videoCaptureDevice?.Resume();
        }

        private void SetVolume()
        {
            _videoCaptureDevice?.SetLinearVolume(Volume);
        }

        private void SetChannel()
        {
            if (_videoCaptureDevice != null)
                _videoCaptureDevice.Channel = _channel;
        }

        #region Binding

        private List<System.Windows.Controls.Image> _bindedImages = new List<System.Windows.Controls.Image>();
        public void BindImageControl(System.Windows.Controls.Image image)
        {
            image.Source = Image;
            _bindedImages.Add(image);
        }

        public void UnbindImageControl(System.Windows.Controls.Image image)
        {
            image.Source = null;
            _bindedImages.Remove(image);
        }

        public void UnbindAll()
        {
            foreach (var img in _bindedImages)
                img.Source = null;
            _bindedImages.Clear();
        }

        public System.Windows.Controls.Image ImageControl
        {
            get
            {
                if (_bindedImages.Count > 0)
                    return _bindedImages[0];
                else
                    return null;
            }
        }

        #endregion

        #region Capture Display

        private ImageSource _image;
        public ImageSource Image
        {
            get { return _image; }
            set
            {
                _image = value;
                foreach (var img in _bindedImages)
                    img.Source = Image;
            }
        }

        private void VideoCaptureDevice_NewFrameArray(object sender, NewFrameArrayEventArgs eventArgs)
        {
            try
            {
                ImageControl?.Dispatcher.Invoke(() =>
                {
                    UpdateBitmapSource(eventArgs.Pixels, eventArgs.PixelWidth, eventArgs.PixelHeight);
                });
            }
            catch { }
        }

        private void VideoCaptureDevice_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            try
            {
                ImageControl?.Dispatcher.Invoke(() =>
                {
                    UpdateBitmapSource(eventArgs.Frame);
                });
            }
            catch { }
        }

        private void UpdateBitmapSource(System.Drawing.Bitmap bitmap)
        {
            try
            {
                var bitmapData = bitmap.LockBits(
                    new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                    System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

                var writableBitmap = Image as WriteableBitmap;
                if (writableBitmap == null || writableBitmap.Width != bitmapData.Width || writableBitmap.Height != bitmapData.Height)
                {
                    writableBitmap = new WriteableBitmap(bitmapData.Width, bitmapData.Height, 96, 96, PixelFormats.Bgr24, null);
                    Image = writableBitmap;
                }

                writableBitmap.WritePixels(
                    new Int32Rect(0, 0, bitmapData.Width, bitmapData.Height),
                    bitmapData.Scan0,
                    bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

                bitmap.UnlockBits(bitmapData);
            }
            catch { }
        }

        private void UpdateBitmapSource(byte[] pixels, int width, int height)
        {
            try
            {
                var writableBitmap = Image as WriteableBitmap;
                if (writableBitmap == null || writableBitmap.Width != width || writableBitmap.Height != height)
                {
                    writableBitmap = new WriteableBitmap(width, height, 96, 96, PixelFormats.Bgr24, null);
                    Image = writableBitmap;
                }

                writableBitmap.WritePixels(new Int32Rect(0, 0, width, height), pixels, width * (PixelFormats.Bgr24.BitsPerPixel / 8), 0);
            }
            catch { }
        }

        public void Dispose()
        {
            UnbindAll();
            Stop();
        }

        #endregion

        #region Events

        private void VideoCaptureDevice_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            if (object.ReferenceEquals(_videoCaptureDevice, sender))
                Stop();
            VideoSourceError?.Invoke(this, eventArgs);
        }

        private void VideoCaptureDevice_PlayingFinished(object sender, ReasonToFinishPlaying reason)
        {
            if (object.ReferenceEquals(_videoCaptureDevice, sender))
                Stop();
            PlayingFinished?.Invoke(this, reason);
        }

        #endregion

        #region Static

        private static FilterInfo GetVideoInputDevice(string deviceName)
        {
            return new FilterInfoCollection(FilterCategory.VideoInputDevice).FirstOrDefault(item => item.Name == deviceName);
        }

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


            return new SupportedValues(resolutions, videoInputs);
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
