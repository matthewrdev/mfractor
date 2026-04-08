using JetBrains.Application;
using MFractor.IOC;

namespace MFractor.Rider
{
    [ShellComponent]
    public class MFractorRiderShellComponent
    {
        public MFractorRiderShellComponent()
        {
            var exportResolver = Resolver.ExportResolver;
        }
    }
}
