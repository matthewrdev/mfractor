using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Progress;
using MFractor.IOC;

namespace MFractor.Work
{
    /// <summary>
    /// A <see cref="IWorkEngine"/> actions <see cref="IWorkUnit"/>'s inside the current product, converting the work unit object into a meaningful action. IE: Navigation, code generation, opening a file or a search.
    /// <para/>
    /// The <see cref="IWorkEngine"/> API provides a high-degree of separation and loose coupling between system components as well as maximum code reuse between features.
    /// <para/>
    /// To action an <see cref="IWorkUnit"/>, pass it into one of the ApplyWorkUnit methods and the workUnit engine will handle triggering the correct behaviour within the outer product.
    /// <para/>
    /// The <see cref="IWorkEngine"/> infrastructure is composed of the following APIs:
    /// <para/>
    /// <see cref="IWorkUnit"/>: A model that represents an action or unit of work that should be performed by the outer product.
    /// <para/>
    /// <see cref="IWorkUnitHandler"/>: A handler that accepts a <see cref="IWorkUnit"/> and applies the action it represents in the product.
    /// <para/>
    /// <see cref="IWorkExecutionResult"/>: The result of an <see cref="IWorkUnitHandler"/> handling of an <see cref="IWorkUnit"/>. This includes the files changed, the projects to save and a list of any text replacements required.
    /// <para/>
    /// <see cref="IWorkUnitHandlerRepository"/>: An <see cref="IPartRepository{T}"/> that collects and manages the <see cref="IWorkUnitHandler"/> that are available in the current product.
    /// </summary>
    public interface IWorkEngine
    {
        /// <summary>
        /// Applies the provided <paramref name="workUnits"/> to the IDE.
        /// </summary>
        /// <returns>The workUnits async.</returns>
        /// <param name="workUnits">WorkUnits.</param>
        Task<bool> ApplyAsync(IReadOnlyList<IWorkUnit> workUnits, IProgressMonitor progressMonitor = null);

        /// <summary>
        /// Applies the <paramref name="workUnit"/> to the IDE.
        /// </summary>
        /// <returns>The workUnit async.</returns>
        /// <param name="workUnit">WorkUnit.</param>
        Task<bool> ApplyAsync(IWorkUnit workUnit, IProgressMonitor progressMonitor = null);

        /// <summary>
        /// Applies the <paramref name="workUnits"/> to the IDE.
        /// </summary>
        /// <returns>The workUnits async.</returns>
        /// <param name="workUnits">WorkUnits.</param>
        Task<bool> ApplyAsync(IReadOnlyList<IWorkUnit> workUnits, string description, IProgressMonitor progressMonitor = null);

        /// <summary>
        /// Applies the <paramref name="workUnit"/> to the IDE.
        /// </summary>
        /// <returns>The workUnit async.</returns>
        /// <param name="workUnit">WorkUnit.</param>
        Task<bool> ApplyAsync(IWorkUnit workUnit, string description, IProgressMonitor progressMonitor = null);
    }
}
