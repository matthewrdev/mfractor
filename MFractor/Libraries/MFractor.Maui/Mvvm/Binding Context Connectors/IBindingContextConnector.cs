using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Configuration;
using MFractor.Work;
using MFractor.Workspace.WorkUnits;

namespace MFractor.Maui.Mvvm.BindingContextConnectors
{
    /// <summary>
    /// A <see cref="IBindingContextConnector"/> takes the <see cref="IWorkUnit"/>'s that create a new View/ViewModel pair and manipulates the Views code to initialise it using the view model.
    /// </summary>
    [InheritedExport]
    public interface IBindingContextConnector : IConfigurable
    {
        /// <summary>
        /// Is this <see cref="IBindingContextConnector"/> available for the given <paramref name="projectIdentifier"/>.
        /// </summary>
        /// <returns><c>true</c>, if available was ised, <c>false</c> otherwise.</returns>
        /// <param name="projectIdentifier">Project identifier.</param>
        bool IsAvailable(ProjectIdentifier projectIdentifier);

        /// <summary>
        /// Given the <paramref name="view"/> <paramref name="codeBehind"/> and <paramref name="viewModel"/> workUnits, manipulate them to initialise the view with the view model.
        /// </summary>
        /// <returns>The connect.</returns>
        /// <param name="view">View.</param>
        /// <param name="codeBehind">Code behind.</param>
        /// <param name="viewModel">View model.</param>
        /// <param name="viewModelMetaType">The fully qualified symbol name of the ViewModel.</param>
        /// <param name="viewMetaType">The fully qualified symbol name of the View.</param>
        /// <param name="projectIdentifier">Project identifier.</param>
        IReadOnlyList<IWorkUnit> Connect(CreateProjectFileWorkUnit view, CreateProjectFileWorkUnit codeBehind, CreateProjectFileWorkUnit viewModel, string viewModelMetaType, string viewMetaType, ProjectIdentifier projectIdentifier);
    }
}
