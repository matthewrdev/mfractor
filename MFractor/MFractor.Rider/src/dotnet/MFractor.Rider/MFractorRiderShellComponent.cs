using JetBrains.Application;

namespace MFractor.Rider
{
    [ShellComponent]
    public class MFractorRiderShellComponent
    {
        // Keep the Rider shell bootstrap empty until Rider-native features explicitly
        // request MFractor services. UI for Rider belongs in the IntelliJ frontend
        // and should talk to this backend via protocol instead of Xwt or VS stacks.
    }
}
