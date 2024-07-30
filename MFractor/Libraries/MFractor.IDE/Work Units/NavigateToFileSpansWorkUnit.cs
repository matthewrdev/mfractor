using System.Collections.Generic;
using System.Linq;
using MFractor.Work;

namespace MFractor.Ide.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that lists one or many <see cref="NavigateToFileSpanWorkUnit"/>'s in the IDEs native search display.
    /// <para/>
    /// Use <see cref="AutoOpenSingleResults"/> to specify if the IDE should skip listing single files and open them instead.
    /// </summary>
    public class NavigateToFileSpansWorkUnit : WorkUnit
    {
        public IReadOnlyList<NavigateToFileSpanWorkUnit> Locations { get; }

        public bool AutoOpenSingleResults { get; }

        public NavigateToFileSpansWorkUnit(IEnumerable<NavigateToFileSpanWorkUnit> locations, bool autoOpenSingleResults = true)
        {
            Locations = locations.ToList();
            AutoOpenSingleResults = autoOpenSingleResults;
        }
    }
}
