using Windows.Win32.Foundation;

namespace ShortDev.DirectShow.VirtualDevices.Internal;

internal sealed class PinEnumerator : IEnumPins
{
    readonly OutputFilter _filter;
    public PinEnumerator(OutputFilter filter)
        => _filter = filter;

    uint _index = 0;
    HRESULT IEnumPins.Next(uint cPins, IPin[] ppPins, out uint pcFetched)
    {
        ppPins[0] = _filter.Pin;
        ++_index;
        test(out pcFetched);
        return _index > 1 ? HRESULT.S_FALSE : HRESULT.S_OK;

        static unsafe void test(out uint test)
        {
            fixed (uint* pTest = &test)
            {

            }
        }
    }

    HRESULT IEnumPins.Skip(uint cPins)
        => HRESULT.S_FALSE;

    HRESULT IEnumPins.Reset()
    {
        _index = 0;
        return HRESULT.S_OK;
    }

    HRESULT IEnumPins.Clone(out IEnumPins ppEnum)
    {
        ppEnum = new PinEnumerator(_filter);
        return HRESULT.S_OK;
    }
}
