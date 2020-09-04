using System;
using System.Runtime.InteropServices;
using System.Security;

namespace Accord.Video.DirectShow.Internals
{
    [Guid("56a868b3-0ad4-11ce-b03a-0020af0ba770")]
    [InterfaceType(ComInterfaceType.InterfaceIsDual)]
    [SuppressUnmanagedCodeSecurity]
    public interface IBasicAudio
    {
        int put_Volume(int lVolume);
        int get_Volume(out int plVolume);
        int put_Balance(int lBalance);
        int get_Balance(out int plBalance);
    }
}
