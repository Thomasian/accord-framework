// http://nativelibs4java.sourceforge.net/bridj/api/0.3.1/org/bridj/cpp/com/UUIDs.html
// Before updating to a later version, check if everything is working on this app
// Issues encountered using a later version
// 1. Thread does not exit successfully (never ends) when closed (VideoSource_VideoSourceError fires continously)
// 2. SignalToStop causes an Access Violation exception. PreventFreezing = true fixes this issue but messes up the playback speed
//
// Changes:
// Accord.Video.DirectShow:
//  VideoCaptureDevice.cs:
//      WorkerThread: Added captureGraph.RenderStream(PinCategory.Capture, MediaType.Audio, sourceBase, null, null); to render audio from TV tuner
//

using Accord.Video;
using Accord.Video.DirectShow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace WPFCoreTestApp
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


        IVideoSource videoSource;

        private void button_Click(object sender, RoutedEventArgs e)
        {
            if (videoSource != null)
            {
                CloseCurrentVideoSource();
                return;
            }

            var videoDevices = new FilterInfoCollection(FilterCategory.VideoInputDevice);

            //*
            var videoCaptureDevice = new VideoCaptureDevice(videoDevices[0].MonikerString);
            videoSource = videoCaptureDevice;

            RefreshSupportedFrameSizes(videoCaptureDevice);
            VideoCapabilities caps = videoCapabilitiesDictionary[videoResolutions.Last()];
            videoCaptureDevice.VideoResolution = caps;
            var captureSize = caps.FrameSize;
            //*/

            /*
            //var fileVideoDevice = new FileVideoSource(@"D:\Videos\Genius S01 Einstein (2017 NG 360p re-webrip)\Genius S01E01 Einstein Chapter One.mp4");
            //videoSource = new ScreenCaptureStream(Screen.AllScreens[0].Bounds, 100);
            //videoSource = new FileVideoSource(@"E:\Movies\Argo (2012)\Argo.2012.720p.BluRay.x264.YIFY.mp4");
            //videoSource = new FileVideoSource(@"D:\Videos\Genius S01 Einstein (2017 NG 360p re-webrip)\Genius S01E01 Einstein Chapter One.mp4");
            var fileVideoDevice = new FileVideoSource(@"D:\Videos\2011.03.04.Michael.Jordan.to.the.Max.2000.BluRay.720p.x264.DTS-MySiLU\Michael.Jordan.to.the.Max.2000.BluRay.720p.x264.DTS-MySiLU.mkv");
            //fileVideoDevice.PreventFreezing = true;
            videoSource = fileVideoDevice;
            */

            videoSource.NewFrame += new NewFrameEventHandler(video_NewFrame);
            videoSource.VideoSourceError += VideoSource_VideoSourceError;

            videoSource.Start();
        }

        private void VideoSource_VideoSourceError(object sender, VideoSourceErrorEventArgs eventArgs)
        {
            MessageBox.Show(eventArgs.Description);
        }

        private void CloseCurrentVideoSource()
        {
            if (videoSource != null)
            {
                videoSource.SignalToStop();
                videoSource = null;
            }
        }

        System.Drawing.Bitmap bmp;
        private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
        {
            if (videoSource == null)
                return;

            bmp = eventArgs.Frame;
            if (!Dispatcher.CheckAccess())
            {
                try
                {
                    Dispatcher.Invoke(() =>
                    {
                        image.Source = Convert(bmp);
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }
        }

        public static BitmapSource Convert(System.Drawing.Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(
                new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height),
                System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);

            var bitmapSource = BitmapSource.Create(
                bitmapData.Width, bitmapData.Height,
                bitmap.HorizontalResolution, bitmap.VerticalResolution,
                PixelFormats.Bgr24, null,
                bitmapData.Scan0, bitmapData.Stride * bitmapData.Height, bitmapData.Stride);

            bitmap.UnlockBits(bitmapData);
            return bitmapSource;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            CloseCurrentVideoSource();
        }


        #region Video Capture Device Properties
        private Dictionary<string, VideoCapabilities> videoCapabilitiesDictionary = new Dictionary<string, VideoCapabilities>();
        private Dictionary<string, VideoCapabilities> snapshotCapabilitiesDictionary = new Dictionary<string, VideoCapabilities>();
        private VideoInput[] availableVideoInputs = null;
        // flag telling if user wants to configure snapshots as well
        private bool configureSnapshots = false;

        private HashSet<string> videoResolutions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private HashSet<string> snapshotResolutions = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private HashSet<string> videoInputs = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        // Collect supported video and snapshot sizes
        private void RefreshSupportedFrameSizes(VideoCaptureDevice videoDevice)
        {
            videoResolutions.Clear();
            snapshotResolutions.Clear();
            videoInputs.Clear();
            videoCapabilitiesDictionary.Clear();
            snapshotCapabilitiesDictionary.Clear();

            try
            {
                // collect video capabilities
                VideoCapabilities[] videoCapabilities = videoDevice.VideoCapabilities;

                foreach (VideoCapabilities capabilty in videoCapabilities)
                {
                    string item = string.Format(
                        "{0} x {1}", capabilty.FrameSize.Width, capabilty.FrameSize.Height);

                    videoResolutions.Add(item);
                    videoCapabilitiesDictionary[item] = capabilty;
                }

                if (videoCapabilities.Length == 0)
                    videoResolutions.Add("Not supported");


                if (configureSnapshots)
                {
                    // collect snapshot capabilities
                    VideoCapabilities[] snapshotCapabilities = videoDevice.SnapshotCapabilities;

                    foreach (VideoCapabilities capabilty in snapshotCapabilities)
                    {
                        string item = string.Format(
                            "{0} x {1}", capabilty.FrameSize.Width, capabilty.FrameSize.Height);

                        snapshotResolutions.Add(item);
                        snapshotCapabilitiesDictionary[item] = capabilty;
                    }

                    if (snapshotCapabilities.Length == 0)
                        snapshotResolutions.Add("Not supported");
                }

                // get video inputs
                availableVideoInputs = videoDevice.AvailableCrossbarVideoInputs;

                foreach (VideoInput input in availableVideoInputs)
                {
                    string item = string.Format("{0}: {1}", input.Index, input.Type);
                    videoInputs.Add(item);
                }

                if (videoInputs.Count == 0)
                    videoInputs.Add("Not supported");
            }
            catch { }
        }

        #endregion

    }
}