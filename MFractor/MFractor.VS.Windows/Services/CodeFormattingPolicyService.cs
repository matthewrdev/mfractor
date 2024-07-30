using System;
using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.Formatting;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;

namespace MFractor.VS.Windows.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeFormattingPolicyService))]
    class CodeFormattingPolicyService : ICodeFormattingPolicyService
    {
        class CSharpFormattingPolicy : ICSharpFormattingPolicy
        {
            public CSharpFormattingPolicy(OptionSet optionSet)
            {
                OptionSet = optionSet;
            }

            public OptionSet OptionSet { get; }
        }

        readonly Lazy<IWorkspaceService> workspaceService;
        IWorkspaceService WorkspaceService => workspaceService.Value;

        readonly Lazy<IProjectService> projectService;
        IProjectService ProjectService => projectService.Value;

        readonly Lazy<IXmlFormattingSettings> formattingSettings;
        IXmlFormattingSettings FormattingSettings => formattingSettings.Value;

        [ImportingConstructor]
        public CodeFormattingPolicyService(Lazy<IWorkspaceService> workspaceService,
                                       Lazy<IProjectService> projectService,
                                       Lazy<IXmlFormattingSettings> formattingSettings)
        {
            this.workspaceService = workspaceService;
            this.projectService = projectService;
            this.formattingSettings = formattingSettings;
        }

        public ICSharpFormattingPolicy GetFormattingPolicy()
        {
            return new CSharpFormattingPolicy(WorkspaceService.CurrentWorkspace.Options);
        }

        public ICSharpFormattingPolicy GetFormattingPolicy(Project project)
        {
            return GetFormattingPolicy();
        }

        public ICSharpFormattingPolicy GetFormattingPolicy(ProjectIdentifier projectIdentifier)
        {
            return GetFormattingPolicy();
        }

        public ICSharpFormattingPolicy GetFormattingPolicy(IFeatureContext context)
        {
            return GetFormattingPolicy();
        }
    }
}
