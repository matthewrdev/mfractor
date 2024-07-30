using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Formatting;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.Mvvm.BindingContextConnectors
{
    class DefaultBindingContextConnector : BindingContextConnector
    {
        public const string Id = "com.mfractor.binding_context_connector.default";

        public override string Identifier => Id;

        public override string Name => "No Binding Context Initialisation";

        public override string Documentation => "Do not initialise the View's binding context using the ViewModel.\n\nIf you are using a framework such as Prism or FreshMVVM that includes automatic ViewModel construction, use this option.";

        [ImportingConstructor]
        public DefaultBindingContextConnector(Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                              Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService,
                                              Lazy<IProjectService> projectService)
            : base(formattingPolicyService, xmlFormattingPolicyService, projectService)
        {

        }

        public override bool IsAvailable(ProjectIdentifier projectIdentifier)
        {
            return true;
        }

        public override IReadOnlyList<IWorkUnit> Connect(CreateProjectFileWorkUnit view, CreateProjectFileWorkUnit codeBehind, CreateProjectFileWorkUnit viewModel, string viewModelMetaType, string viewMetaType, ProjectIdentifier projectIdentifier)
        {
            return new List<IWorkUnit>()
            {
                view, codeBehind, viewModel
            };
        }
    }
}