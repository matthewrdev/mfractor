using MFractor.Code.Scaffolding;
using MFractor.Work;

namespace MFractor.Code.WorkUnits
{
    /// <summary>
    /// An <see cref="IWorkUnit"/> that launches the scaffolder dialog.
    /// </summary>
    public class ScaffolderWorkUnit : WorkUnit
    {
        public string InputValue { get; set; }

        public IScaffoldingContext Context { get; set; }
    }
}
