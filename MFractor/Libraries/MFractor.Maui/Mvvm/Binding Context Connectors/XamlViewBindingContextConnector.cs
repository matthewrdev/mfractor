using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Code;
using MFractor.Code.Formatting;
using MFractor.Maui.CodeGeneration.Xaml;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;

namespace MFractor.Maui.Mvvm.BindingContextConnectors
{
    class XamlViewBindingContextConnector : BindingContextConnector
    {
        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        public override string Identifier => "com.mfractor.binding_context_connector.xaml_view";

        public override string Name => "Initialise BindingContext In XAML";

        public override string Documentation => "Initialise the View's binding context using the ViewModel in XAML.";

        [Import]
        public IXamlNamespaceImportGenerator XamlNamespaceImportGenerator
        {
            get;
            set;
        }

        [ImportingConstructor]
        public XamlViewBindingContextConnector(Lazy<ICodeFormattingPolicyService> formattingPolicyService,
                                               Lazy<IXmlFormattingPolicyService> xmlFormattingPolicyService,
                                               Lazy<IProjectService> projectService,
                                               Lazy<IXmlSyntaxWriter> xmlSyntaxWriter,
                                               Lazy<IXmlSyntaxParser> xmlSyntaxParser)
            : base(formattingPolicyService, xmlFormattingPolicyService, projectService)
        {
            this.xmlSyntaxWriter = xmlSyntaxWriter;
            this.xmlSyntaxParser = xmlSyntaxParser;
        }

        public override IReadOnlyList<IWorkUnit> Connect(CreateProjectFileWorkUnit view, CreateProjectFileWorkUnit codeBehind, CreateProjectFileWorkUnit viewModel, string viewModelMetaType, string viewMetaType, ProjectIdentifier projectIdentifier)
        {
            var xaml = XmlSyntaxParser.ParseText(view.FileContent);

            var root = xaml.Root;

            var viewModelSymbolParts = viewModelMetaType.Split('.');

            if (viewModelSymbolParts.Length > 1)
            {
                var viewModelNamespace = string.Join(".", viewModelSymbolParts.Take(viewModelSymbolParts.Length - 1));

                var viewModelName = viewModelSymbolParts.Last();

                var importAttr = XamlNamespaceImportGenerator.GenerateXmlnsImportAttibute("viewModels", viewModelNamespace);

                root.AddAttribute(importAttr);

                var name = root.Name.FullName;

                var bindingContextInitialiser = new XmlNode(name + ".BindingContext");

                var viewModelInitialiser = new XmlNode("viewModels:" + viewModelName);
                viewModelInitialiser.AddAttribute("x:Name", "ViewModel");
                viewModelInitialiser.IsSelfClosing = true;

                bindingContextInitialiser.AddChildNode(viewModelInitialiser);
                root.AddChildNode(bindingContextInitialiser);

                var policy = XmlFormattingPolicyService.GetXmlFormattingPolicy(projectIdentifier);

                view.FileContent = XmlSyntaxWriter.WriteNode(root, string.Empty, policy, true, true, true);
            }

            return new List<IWorkUnit>()
            {
                view, codeBehind, viewModel,
            };
        }

        public override bool IsAvailable(ProjectIdentifier projectIdentifier)
        {
            return true;
        }
    }
}
