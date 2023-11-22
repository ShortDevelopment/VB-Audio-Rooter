
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Windows.Win32.Media.KernelStreaming;

[Guid("C6E13340-30AC-11D0-A18C-00A0C9118956"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport()]
[SupportedOSPlatform("windows5.0")]
internal unsafe interface IKsPropertySet
{
    Foundation.HRESULT Set(
        in Guid PropSet,
        ulong Id,
        void* InstanceData,
        ulong InstanceLength,
        void* PropertyData,
        ulong DataLength
    );

    Foundation.HRESULT Get(
        in Guid guidPropSet,
        in uint dwPropID,
        void* pInstanceData,
        in uint cbInstanceData,
        void* pPropData,
        in uint cbPropData,
        out uint pcbReturned
    );

    Foundation.HRESULT QuerySupported(
        in Guid PropSet,
        ulong Id,
        ulong* TypeSupport
    );
}