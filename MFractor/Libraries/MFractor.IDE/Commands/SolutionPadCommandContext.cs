using MFractor.Ide.Commands;
using MFractor.IOC;

namespace MFractor.Ide.Commands
{
    public class SolutionPadCommandContext : ISolutionPadCommandContext
    {
        public SolutionPadCommandContext(object selectedItem)
        {
            SelectedItem = selectedItem;
        }

        public object SelectedItem { get; }

        public static ISolutionPadCommandContext Create()
        {
            var item = Resolver.Resolve<ISolutionPad>().SelectedItem;

            return new SolutionPadCommandContext(item);
        }
    }
}
