using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using MFractor.Code.Formatting;
using MFractor.Configuration;
using MFractor.Work;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.Mvvm.BindingContextConnectors
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IBindingContextConnector))]
    public abstract class BindingContextConnector : Configurable, IBindingContextConnector
    {
        readonly Lazy<ICodeFormattingPolicyService> formattingPolicyService;
        readonly Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService;
        readonly Lazy<IProjectService> projectService;

        protected BindingContextConnector(Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                          Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService,
                                          Lazy<IProjectService> projectService)
        {
            this.formattingPolicyService = formattingPolicyService;
            this.xmlFormattingPolicyService = xmlFormattingPolicyService;
            this.projectService = projectService;
        }

        protected ICodeFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;

        protected IXmlFormattingPolicyService XmlFormattingPolicyService => xmlFormattingPolicyService.Value;

        protected IProjectService ProjectService => projectService.Value;

        public abstract bool IsAvailable(ProjectIdentifier projectIdentifier);
        public abstract IReadOnlyList<IWorkUnit> Connect(CreateProjectFileWorkUnit view, CreateProjectFileWorkUnit codeBehind, CreateProjectFileWorkUnit viewModel, string viewModelMetaType, string viewMetaType, ProjectIdentifier projectIdentifier);
    }
}