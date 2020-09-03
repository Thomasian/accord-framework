using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace DirectShow.Wpf
{
    public class TvTunerSettings
    {
        public const Int32 MinVolume = 0;
        public const Int32 MaxVolume = 100;
        public const Int32 MinChannel = 1;
        public const Int32 MaxChannel = 200;

        private int _volume = MinVolume;
        public Int32 Volume
        {
            get { return _volume; }
            set
            {
                if (value > MaxVolume)
                    _volume = MaxVolume;
                else if (value < MinVolume)
                    _volume = MinVolume;
                else
                    _volume = value;
            }
        }

        public bool MaintainAspectRatio { get; set; } = true;
        public string VideoDeviceName { get; set; }
        public string VideoResolution { get; set; }
        public string VideoInput { get; set; }
        public Int32 Channel { get; set; } = MinChannel;
        public ObservableCollection<Int32> ChannelList { get; set; } = new ObservableCollection<int>();
    }
}
