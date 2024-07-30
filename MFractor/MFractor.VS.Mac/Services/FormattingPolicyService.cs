using System.ComponentModel.Composition;
using MFractor.Code;
using MFractor.Code.Formatting;
using MFractor.VS.Mac.Utilities;
using MFractor.Workspace;
using MFractor.Workspace.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Options;
using MonoDevelop.Ide;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ICodeFormattingPolicyService))]
    class FormattingPolicyService : ICodeFormattingPolicyService
    {
        class CSharpFormattingPolicy : ICSharpFormattingPolicy
        {
            public CSharpFormattingPolicy(OptionSet optionSet)
            {
                OptionSet = optionSet;
            }

            public OptionSet OptionSet { get; }
        }

        readonly IWorkspaceService workspaceService;

        [ImportingConstructor]
        public FormattingPolicyService(IWorkspaceService workspaceService)
        {
            this.workspaceService = workspaceService;
        }

        public ICSharpFormattingPolicy GetFormattingPolicy()
        {
            return new CSharpFormattingPolicy(workspaceService.CurrentWorkspace.Options);
        }

        public ICSharpFormattingPolicy GetFormattingPolicy(Project project)
        {
            return GetFormattingPolicy(project.GetIdentifier());
        }

        public ICSharpFormattingPolicy GetFormattingPolicy(ProjectIdentifier projectIdentifier)
        {
            var ideProject = projectIdentifier.ToIdeProject();

            var defaultOptions = workspaceService.CurrentWorkspace.Options;

            if (ideProject == null)
            {
                return new CSharpFormattingPolicy(defaultOptions);
            }

            var policyParent = ideProject?.Policies;
            var types = IdeServices.DesktopService.GetMimeTypeInheritanceChain("text/x-csharp");
            var codePolicy = policyParent != null ? policyParent.Get<MonoDevelop.CSharp.Formatting.CSharpFormattingPolicy>(types) : MonoDevelop.Projects.Policies.PolicyService.GetDefaultPolicy<MonoDevelop.CSharp.Formatting.CSharpFormattingPolicy>(types);
            var textPolicy = policyParent != null ? policyParent.Get<MonoDevelop.Ide.Gui.Content.TextStylePolicy>(types) : MonoDevelop.Projects.Policies.PolicyService.GetDefaultPolicy<MonoDevelop.Ide.Gui.Content.TextStylePolicy>(types);

            return new CSharpFormattingPolicy(codePolicy.CreateOptions(textPolicy));
        }

        public ICSharpFormattingPolicy GetFormattingPolicy(IFeatureContext context)
        {
            return GetFormattingPolicy(context.Project);
        }
    }
}
