using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration;
using MFractor.VS.Mac.Utilities;
using MFractor.Xml;
using MonoDevelop.Ide;
using MonoDevelop.Projects.Policies;

namespace MFractor.VS.Mac.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IXmlFormattingPolicyService))]
    class XmlFormattingPolicyService : IXmlFormattingPolicyService
    {
        readonly IUserOptions userOptions;
        readonly IXmlFormattingSettings xmlFormattingSettings;

        [ImportingConstructor]
        public XmlFormattingPolicyService(IUserOptions userOptions,
                                          IXmlFormattingSettings xmlFormattingSettings)
        {
            this.userOptions = userOptions;
            this.xmlFormattingSettings = xmlFormattingSettings;
        }


        public IXmlFormattingPolicy GetXmlFormattingPolicy()
        {
            return new DefaultXmlFormattingPolicy();
        }

        public IXmlFormattingPolicy GetXmlFormattingPolicy(ProjectIdentifier projectIdentifier)
        {
            var ideProject = projectIdentifier?.ToIdeProject();

            if (ideProject == null)
            {
                return new DefaultXmlFormattingPolicy();
            }

            var policyParent = ideProject.Policies as PolicyContainer;
            if (policyParent == null)
            {
                policyParent = PolicyService.DefaultPolicies;
            }

            var mimeTypeInheritanceChain = IdeServices.DesktopService.GetMimeTypeInheritanceChain("text/xml").ToList();

            return new IdeXmlFormattingPolicy(policyParent.Get<MonoDevelop.Xml.Formatting.XmlFormattingPolicy>(mimeTypeInheritanceChain), userOptions, xmlFormattingSettings);
        }
    }
}
