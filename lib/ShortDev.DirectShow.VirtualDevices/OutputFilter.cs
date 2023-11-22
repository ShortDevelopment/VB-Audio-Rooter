using ShortDev.DirectShow.VirtualDevices.Internal;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Media;
using Windows.Win32.Media.Audio;
using Windows.Win32.Media.MediaFoundation;

namespace ShortDev.DirectShow.VirtualDevices;

// https://github.com/obsproject/libdshowcapture/blob/master/source/output-filter.cpp

public abstract class OutputFilter : IBaseFilter
{
    public abstract string Name { get; }
    public abstract bool IsOutput { get; }

    public OutputPin Pin { get; }
    public AM_MEDIA_TYPE MediaType { get; }
    public OutputFilter(AM_MEDIA_TYPE mediaType)
    {
        Pin = new(this, mediaType);
        MediaType = mediaType;
    }

    IFilterGraph? _graph;

    public FILTER_STATE State { get; private set; } = FILTER_STATE.State_Stopped;

    public unsafe HRESULT GetState(uint dwMilliSecsTimeout, FILTER_STATE* state)
    {
        *state = State;

        return HRESULT.S_OK;
    }

    public HRESULT Stop()
    {
        if (State != FILTER_STATE.State_Stopped)
            Pin.Flush();

        State = FILTER_STATE.State_Stopped;

        return HRESULT.S_OK;
    }

    public HRESULT Pause()
    {
        // ToDo: Implementation?!

        State = FILTER_STATE.State_Paused;

        return HRESULT.S_OK;
    }

    public HRESULT Run(long tStart)
    {
        State = FILTER_STATE.State_Running;

        return HRESULT.S_OK;
    }

    #region IReferenceClock
    IReferenceClock? _clock;
    public HRESULT SetSyncSource(IReferenceClock pClock)
    {
        _clock = pClock;

        return HRESULT.S_OK;
    }

    public HRESULT GetSyncSource(out IReferenceClock? pClock)
    {
        pClock = _clock;

        return HRESULT.S_OK;
    }
    #endregion

    HRESULT IBaseFilter.EnumPins(out IEnumPins ppEnum)
    {
        ppEnum = new PinEnumerator(this);

        return HRESULT.S_OK;
    }

    HRESULT IBaseFilter.FindPin(PCWSTR Id, out IPin? ppPin)
    {
        ppPin = null;

        var id = Id.ToString();
        if (id != Pin.Name)
            return HRESULT.VFW_E_NOT_FOUND;

        ppPin = Pin;

        return HRESULT.S_OK;
    }

    HRESULT IBaseFilter.QueryFilterInfo(out FILTER_INFO pInfo)
    {
        pInfo.achName = Name;
        pInfo.pGraph = _graph; // ToDo: AddRef ?!

        return HRESULT.S_OK;
    }

    HRESULT IBaseFilter.JoinFilterGraph(IFilterGraph pGraph, PCWSTR pName)
    {
        _graph = pGraph;

        return HRESULT.S_OK;
    }

    #region Not Implemented
    public unsafe HRESULT GetClassID(Guid* pClassID)
        => HRESULT.E_NOTIMPL;

    unsafe HRESULT IBaseFilter.QueryVendorInfo(PWSTR* pVendorInfo)
        => HRESULT.E_NOTIMPL;
    #endregion

    public static unsafe AM_MEDIA_TYPE CreateMediaType()
    {
        AM_MEDIA_TYPE result = default;
        result.majortype = MEDIATYPE_Audio;
        result.subtype = MEDIASUBTYPE_PCM;

        var cbFormat = sizeof(WAVEFORMATEX);
        var pFormat = (WAVEFORMATEX*)Marshal.AllocCoTaskMem(cbFormat);

        pFormat->wFormatTag = (ushort)WAVE_FORMAT_PCM;
        pFormat->nChannels = 2;
        pFormat->wBitsPerSample = 32;
        pFormat->nSamplesPerSec = 48000;
        pFormat->nBlockAlign = (ushort)(pFormat->nChannels * pFormat->wBitsPerSample / 8);
        pFormat->nAvgBytesPerSec = pFormat->nBlockAlign * pFormat->nSamplesPerSec;
        pFormat->cbSize = 0;

        result.formattype = FORMAT_WaveFormatEx;
        result.cbFormat = (uint)cbFormat;
        result.pbFormat = (byte*)pFormat;

        result.bFixedSizeSamples = true;
        // result.lSampleSize = ;

        return result;
    }
}
