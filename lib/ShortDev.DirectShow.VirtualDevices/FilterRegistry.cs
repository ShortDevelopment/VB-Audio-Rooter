using Windows.Win32.System.Com;

namespace ShortDev.DirectShow.VirtualDevices;

public static class FilterRegistry
{
    public static unsafe void RegisterFilter<T>(bool register)
    {
        CoCreateInstance<IFilterMapper2>(CLSID_FilterMapper2, null, CLSCTX.CLSCTX_INPROC_SERVER, out var mapper).ThrowOnFailure();

        Guid mediType = MEDIATYPE_Audio;
        REGPINTYPES types = new()
        {
            clsMajorType = &mediType,
            // clsMinorType = MEDIASUBTYPE_NULL
        };
        REGFILTERPINS pinInfo = new()
        {
            bMany = false,
            bOutput = false,
            bRendered = true,
            bZero = false,
            nMediaTypes = 1,
            lpMediaType = &types
        };
        REGFILTER2 filter = new()
        {
            dwVersion = 1,
            dwMerit = (uint)IFILTERMAPPER_MERIT.MERIT_NORMAL,
            Anonymous = new()
            {
                Anonymous1 = new()
                {
                    cPins = 1,
                    rgPins = &pinInfo
                }
            }
        };

        Type type = typeof(T);
        var clsid = type.GUID;
        var category = CLSID_AudioInputDeviceCategory;
        fixed (char* pName = type.Name)
        fixed (char* pInstanceName = clsid.ToString("B"))
        {
            if (register)
                mapper.RegisterFilter(&clsid, pName, null, &category, pInstanceName, &filter).ThrowOnFailure();
            else
                mapper.UnregisterFilter(&category, pInstanceName, &clsid).ThrowOnFailure();
        }

    }
}
