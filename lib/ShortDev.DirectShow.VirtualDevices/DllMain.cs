using System.Runtime.InteropServices;
using Windows.Win32.Foundation;

namespace ShortDev.DirectShow.VirtualDevices;

internal class DllMain
{
    [UnmanagedCallersOnly(EntryPoint = nameof(DllCanUnloadNow))]
    public static HRESULT DllCanUnloadNow()
    {
        return HRESULT.S_FALSE;
    }

    [UnmanagedCallersOnly(EntryPoint = nameof(DllGetClassObject))]
    public static unsafe HRESULT DllGetClassObject(Guid* rclsid, Guid* riid, void** ppv)
    {
        var clsid = *rclsid;
        var iid = *riid;

        if (clsid != typeof(TestFilter).GUID)
            return HRESULT.S_FALSE;

        var pUnk = Marshal.GetIUnknownForObject(new TestFilter());
        var hr = (HRESULT)Marshal.QueryInterface(pUnk, ref iid, out var result);
        hr.ThrowOnFailure();

        *ppv = (void*)result;

        return HRESULT.S_OK;
    }
}
