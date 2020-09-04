using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;

namespace Accord.Video.DirectShow.Internals
{
    public enum TunerInputType
    {
        Cable = 0,
        Antenna = 1
    }
    [Guid("211A8766-03AC-11d1-8D13-00AA00BD8339")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressUnmanagedCodeSecurity]
    public interface IAMTVTuner : IAMTuner
    {
        new int put_Channel(int lChannel, AMTunerSubChannel lVideoSubChannel, AMTunerSubChannel lAudioSubChannel);
        new int get_Channel(out int plChannel, out AMTunerSubChannel plVideoSubChannel, out AMTunerSubChannel plAudioSubChannel);
        new int ChannelMinMax(out int lChannelMin, out int lChannelMax);
        new int put_CountryCode(int lCountryCode);
        new int get_CountryCode(out int plCountryCode);
        new int put_TuningSpace(int lTuningSpace);
        new int get_TuningSpace(out int plTuningSpace);
        new int Logon(IntPtr hCurrentUser);
        new int Logout();
        new int SignalPresent(out AMTunerSignalStrength plSignalStrength);
        new int put_Mode(AMTunerModeType lMode);
        new int get_Mode(out AMTunerModeType plMode);
        new int GetAvailableModes(out AMTunerModeType plModes);
        new int RegisterNotificationCallBack(IAMTunerNotification pNotify, AMTunerEventType lEvents);
        new int UnRegisterNotificationCallBack(IAMTunerNotification pNotify);
        int get_AvailableTVFormats(out AnalogVideoStandard lAnalogVideoStandard);
        int get_TVFormat(out AnalogVideoStandard lAnalogVideoStandard);
        int AutoTune(int lChannel, out int plFoundSignal);
        int StoreAutoTune();
        int get_NumInputConnections(out int plNumInputConnections);
        int put_InputType(int lIndex, TunerInputType inputType);
        int get_InputType(int lIndex, out TunerInputType inputType);
        int put_ConnectInput(int lIndex);
        int get_ConnectInput(out int lIndex);
        int get_VideoFrequency(out int lFreq);
        int get_AudioFrequency(out int lFreq);
    }
}
