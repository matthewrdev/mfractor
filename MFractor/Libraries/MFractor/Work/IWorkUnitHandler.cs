using System;
using System.ComponentModel.Composition;
using System.Threading.Tasks;
using MFractor.Progress;

namespace MFractor.Work
{
    /// <summary>
    /// A <see cref="IWorkUnitHandler"/> is responsible for taking an implemenation of <see cref="IWorkUnit"/>, interpreting it and applying it within the IDE.
    /// <para/>
    /// <see cref="IWorkUnitHandler"/> handler implementations are automatically registered into the work engine via MEF.
    /// </summary>
    [InheritedExport]
    public interface IWorkUnitHandler
    {
        /// <summary>
        /// The type of <see cref="IWorkUnit"/> that this work unit handler supports.
        /// </summary>
        /// <value>The type of the supported workUnit.</value>
        Type SupportedWorkUnitType { get; }

        /// <summary>
        /// Execute the <paramref name="workUnit"/>.
        /// </summary>
        /// <param name="workUnit"></param>
        /// <param name="progressMonitor"></param>
        /// <returns></returns>
        Task<IWorkExecutionResult> Execute(IWorkUnit workUnit, IProgressMonitor progressMonitor);
    }
}
