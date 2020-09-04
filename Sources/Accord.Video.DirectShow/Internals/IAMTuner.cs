using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Accord.Video.DirectShow.Internals
{
    //
    // Summary:
    //     From AMTunerSubChannel
    public enum AMTunerSubChannel
    {
        NoTune = -2,
        Default = -1
    }

    //
    // Summary:
    //     From AMTunerSignalStrength
    public enum AMTunerSignalStrength
    {
        HasNoSignalStrength = -1,
        NoSignal = 0,
        SignalPresent = 1
    }

    //
    // Summary:
    //     From AMTunerModeType
    [Flags]
    public enum AMTunerModeType
    {
        Default = 0,
        TV = 1,
        FMRadio = 2,
        AMRadio = 4,
        Dss = 8,
        DTV = 16
    }

    //
    // Summary:
    //     From AMTunerEventType
    public enum AMTunerEventType
    {
        Changed = 1
    }

    [Guid("211A8760-03AC-11d1-8D13-00AA00BD8339")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressUnmanagedCodeSecurity]
    public interface IAMTunerNotification
    {
        int OnEvent(AMTunerEventType Event);
    }

    [Guid("211A8761-03AC-11d1-8D13-00AA00BD8339")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    [SuppressUnmanagedCodeSecurity]
    public interface IAMTuner
    {

        int put_Channel(int lChannel, AMTunerSubChannel lVideoSubChannel, AMTunerSubChannel lAudioSubChannel);
        int get_Channel(out int plChannel, out AMTunerSubChannel plVideoSubChannel, out AMTunerSubChannel plAudioSubChannel);
        int ChannelMinMax(out int lChannelMin, out int lChannelMax);
        int put_CountryCode(int lCountryCode);
        int get_CountryCode(out int plCountryCode);
        int put_TuningSpace(int lTuningSpace);
        int get_TuningSpace(out int plTuningSpace);
        int Logon(IntPtr hCurrentUser);
        int Logout();
        int SignalPresent(out AMTunerSignalStrength plSignalStrength);
        int put_Mode(AMTunerModeType lMode);
        int get_Mode(out AMTunerModeType plMode);
        int GetAvailableModes(out AMTunerModeType plModes);
        int RegisterNotificationCallBack(IAMTunerNotification pNotify, AMTunerEventType lEvents);
        int UnRegisterNotificationCallBack(IAMTunerNotification pNotify);
    }
}
