using System;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration;
using MFractor.Xml;

namespace MFractor.VS.Window.Services
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
            return CreateDefaultXmlFormattingPolicy();
        }

        public IXmlFormattingPolicy GetXmlFormattingPolicy(ProjectIdentifier projectIdentifier)
        {
            return CreateDefaultXmlFormattingPolicy();
        }


        IXmlFormattingPolicy CreateDefaultXmlFormattingPolicy()
        {
            // TODO: Locate the XML formatting policy
            // VSMac has built-in policies. Need to check if VSWin has similar concept.
            var defaultPolicy = DefaultXmlFormattingPolicy.Instance;
            var defaultPolicyWithSettings = new OverloadableXmlFormattingPolicy(defaultPolicy);

            defaultPolicyWithSettings.AppendSpaceBeforeSlashOnSelfClosingTag = xmlFormattingSettings.AppendSpaceBeforeSlashOnSelfClosingTag;
            defaultPolicyWithSettings.AlignAttributesToFirstAttribute = xmlFormattingSettings.AlignAttributesToFirstAttribute;
            defaultPolicyWithSettings.AttributesIndentString = xmlFormattingSettings.AttributesIndentString;
            defaultPolicyWithSettings.ContentIndentString = xmlFormattingSettings.ContentIndentString;
            defaultPolicyWithSettings.FirstAttributeOnNewLine = xmlFormattingSettings.FirstAttributeOnNewLine;
            defaultPolicyWithSettings.NewLineChars = xmlFormattingSettings.NewLineChars;

            return defaultPolicyWithSettings;
        }
    }
}
