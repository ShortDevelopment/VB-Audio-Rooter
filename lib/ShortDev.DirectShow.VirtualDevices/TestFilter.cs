using System.Runtime.InteropServices;

namespace ShortDev.DirectShow.VirtualDevices;

[ComVisible(true)]
[Guid("C2738EF7-CA39-4CC7-B80E-74A8E2EFD411")]
public sealed class TestFilter : OutputFilter
{
    public TestFilter() : base(CreateMediaType())
    {
    }

    public override string Name
        => nameof(TestFilter);

    public override bool IsOutput { get; } = false;

    public void Run()
    {

    }
}
