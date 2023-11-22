using ShortDev.DirectShow.VirtualDevices.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Windows.Win32.Foundation;
using Windows.Win32.Media.KernelStreaming;
using Windows.Win32.Media.MediaFoundation;
using Windows.Win32.System.Com;

namespace ShortDev.DirectShow.VirtualDevices;

public sealed class OutputPin : IPin, IAMStreamConfig_Mod, IKsPropertySet
{
    public AM_MEDIA_TYPE MediaType { get; private set; }

    readonly OutputFilter _filter;
    public OutputPin(OutputFilter filter, AM_MEDIA_TYPE mediaType)
    {
        _filter = filter;
        MediaType = mediaType;
    }

    public string Name { get; } = nameof(OutputPin);

    public PIN_DIRECTION Direction
        => _filter.IsOutput ? PIN_DIRECTION.PINDIR_INPUT : PIN_DIRECTION.PINDIR_OUTPUT;

    IPin? _connectedPin;
    HRESULT IPin.Connect(IPin pReceivePin, in AM_MEDIA_TYPE pmt)
    {
        if (_filter.State == FILTER_STATE.State_Running)
            return HRESULT.VFW_E_NOT_STOPPED;

        if (_connectedPin != null)
            return HRESULT.VFW_E_ALREADY_CONNECTED;

        if (!pReceivePin.ReceiveConnection(this, MediaType).Succeeded)
            return HRESULT.E_FAIL;

        if (!TryAllocateBuffers(pReceivePin))
            return HRESULT.E_FAIL;

        _connectedPin = pReceivePin;
        return HRESULT.S_OK;
    }

    HRESULT IPin.ReceiveConnection(IPin pConnector, in AM_MEDIA_TYPE pmt)
        => HRESULT.S_OK;

    public HRESULT Disconnect()
    {
        if (_connectedPin == null)
            return HRESULT.S_FALSE;

        _allocator?.Decommit();
        _allocator = null;

        _connectedPin = null;
        return HRESULT.S_OK;
    }

    public HRESULT ConnectedTo(out IPin? pPin)
    {
        pPin = null;

        if (_connectedPin == null)
            return HRESULT.VFW_E_NOT_CONNECTED;

        pPin = _connectedPin;

        return HRESULT.S_OK;
    }

    public HRESULT ConnectionMediaType(out AM_MEDIA_TYPE pmt)
    {
        pmt = default;

        if (_connectedPin == null)
            return HRESULT.VFW_E_NOT_CONNECTED;

        pmt = MediaType; // ToDo: Copy?!
        return HRESULT.S_OK;
    }

    public HRESULT QueryPinInfo(out PIN_INFO pInfo)
    {
        pInfo.pFilter = _filter;
        pInfo.achName = Name;
        pInfo.dir = Direction;

        return HRESULT.S_OK;
    }

    public unsafe HRESULT QueryDirection(PIN_DIRECTION* pPinDir)
    {
        *pPinDir = Direction;
        return HRESULT.S_OK;
    }

    public unsafe HRESULT QueryId(PWSTR* Id)
    {
        fixed (char* pIdStr = Name)
            *Id = new PWSTR(pIdStr);

        return HRESULT.S_OK;
    }

    public HRESULT QueryAccept(in AM_MEDIA_TYPE pmt)
        => HRESULT.S_OK;

    public HRESULT EnumMediaTypes(out IEnumMediaTypes ppEnum)
    {
        ppEnum = new MediaTypeEnumerator(this);

        return HRESULT.S_OK;
    }

    public HRESULT QueryInternalConnections([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1), Out] IPin[] apPin, ref uint nPin)
        => HRESULT.E_NOTIMPL;

    public HRESULT EndOfStream()
        => HRESULT.S_OK;

    bool _flusing = false;
    public HRESULT BeginFlush()
    {
        _flusing = true;

        return HRESULT.S_OK;
    }

    public HRESULT EndFlush()
    {
        _flusing = false;

        return HRESULT.S_OK;
    }

    public HRESULT NewSegment(long tStart, long tStop, double dRate)
        => HRESULT.S_OK;

    public HRESULT SetFormat(in AM_MEDIA_TYPE pmt)
    {
        MediaType = pmt;

        return HRESULT.S_OK;
    }

    public HRESULT GetFormat(out AM_MEDIA_TYPE[] ppmt)
    {
        ppmt = new[] { MediaType }; // ToDo: Copy?!

        return HRESULT.S_OK;
    }

    public unsafe HRESULT GetNumberOfCapabilities(out int piCount, out int piSize)
    {
        piCount = 1;
        piSize = sizeof(AUDIO_STREAM_CONFIG_CAPS);

        return HRESULT.S_OK;
    }

    public unsafe HRESULT GetStreamCaps(int iIndex, out AM_MEDIA_TYPE[] ppmt, void* pSCC)
    {
        ppmt = new[] { MediaType };

        if (iIndex != 0)
            return HRESULT.E_INVALIDARG;

        AUDIO_STREAM_CONFIG_CAPS caps = *(AUDIO_STREAM_CONFIG_CAPS*)pSCC;
        caps.guid = MEDIATYPE_Audio;
        // ToDo: Add stuff

        return HRESULT.S_OK;
    }

    public void Flush()
    {
        if (_connectedPin == null)
            return;

        _connectedPin.BeginFlush();
        _connectedPin.EndFlush();
    }

    IMemAllocator? _allocator;
    bool TryAllocateBuffers(IPin target)
    {
        _allocator?.Decommit();

        var memPin = (IMemInputPin)target;

        var hr = memPin.GetAllocator(out _allocator);
        if (hr != HRESULT.S_OK)
            hr = CoCreateInstance<IMemAllocator>(CLSID_MemoryAllocator, null, CLSCTX.CLSCTX_INPROC_SERVER, out _allocator);

        if (hr.Failed || _allocator == null)
            return false;

        // ToDo: Set properties
        //if (_allocator.SetProperties().Failed)
        //    return false;

        _allocator.Commit();

        memPin.NotifyAllocator(_allocator, false);

        return true;
    }

    public bool TryGetBuffer([MaybeNullWhen(false)] out IMediaSample sample, long startTime = 0, long endTime = 0, uint flags = AM_GBF_NOWAIT)
    {
        sample = null;

        if (_allocator == null)
            return false;

        return _allocator.GetBuffer(out sample, startTime, endTime, flags).Succeeded;
    }

    #region PropertySet
    unsafe HRESULT IKsPropertySet.Set(in Guid PropSet, ulong Id, void* InstanceData, ulong InstanceLength, void* PropertyData, ulong DataLength)
        => HRESULT.E_NOTIMPL;

    unsafe HRESULT IKsPropertySet.Get(in Guid guidPropSet, in uint dwPropID, void* pInstanceData, in uint cbInstanceData, void* pPropData, in uint cbPropData, out uint pcbReturned)
    {
        pcbReturned = 0;

        if (guidPropSet != AMPROPSETID_Pin)
            return HRESULT.E_PROP_SET_UNSUPPORTED;

        if ((AMPROPERTY_PIN)dwPropID != AMPROPERTY_PIN.AMPROPERTY_PIN_CATEGORY)
            return HRESULT.E_PROP_SET_UNSUPPORTED;

        pcbReturned = (uint)sizeof(Guid);
        *(Guid*)pPropData = PIN_CATEGORY_CAPTURE;

        return HRESULT.S_OK;
    }

    unsafe HRESULT IKsPropertySet.QuerySupported(in Guid PropSet, ulong Id, ulong* TypeSupport)
    {
        if (PropSet != AMPROPSETID_Pin)
            return HRESULT.E_PROP_SET_UNSUPPORTED;

        if ((AMPROPERTY_PIN)Id != AMPROPERTY_PIN.AMPROPERTY_PIN_CATEGORY)
            return HRESULT.E_PROP_SET_UNSUPPORTED;

        if (TypeSupport != (void*)0)
            *TypeSupport = KSPROPERTY_SUPPORT_GET;

        return HRESULT.S_OK;
    }
    #endregion
}
