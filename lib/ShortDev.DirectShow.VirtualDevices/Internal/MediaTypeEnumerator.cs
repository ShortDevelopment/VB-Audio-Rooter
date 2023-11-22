using Windows.Win32.Foundation;
using Windows.Win32.Media.MediaFoundation;

namespace ShortDev.DirectShow.VirtualDevices.Internal;

internal sealed class MediaTypeEnumerator : IEnumMediaTypes
{
    readonly OutputPin _pin;
    public MediaTypeEnumerator(OutputPin pin)
        => _pin = pin;

    uint _index = 0;
    HRESULT IEnumMediaTypes.Next(uint cMediaTypes, AM_MEDIA_TYPE[][] ppMediaTypes, out uint pcFetched)
    {
        ppMediaTypes[0] = new[] { _pin.MediaType };

        pcFetched = ++_index;

        return pcFetched > 1 ? HRESULT.S_FALSE : HRESULT.S_OK;
    }

    HRESULT IEnumMediaTypes.Skip(uint cMediaTypes)
        => HRESULT.S_FALSE;

    HRESULT IEnumMediaTypes.Reset()
    {
        _index = 0;
        return HRESULT.S_OK;
    }

    HRESULT IEnumMediaTypes.Clone(out IEnumMediaTypes ppEnum)
    {
        ppEnum = new MediaTypeEnumerator(_pin);
        return HRESULT.S_OK;
    }
}
