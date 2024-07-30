using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFractor.Code.Formatting;
using MFractor.Code.WorkUnits;
using MFractor.CSharp.Models;
using MFractor.Utilities;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using MFractor.Workspace.WorkUnits;
using MFractor.Xml;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IConvertProjectToCSharp9Service))]
    public class ConvertProjectToCSharp9Service : IConvertProjectToCSharp9Service
    {
        const string compatClassContent = @"namespace System.Runtime.CompilerServices
{
    public class IsExternalInit { }
}";

        readonly Lazy<IXmlSyntaxParser> syntaxParser;
        readonly Lazy<IXmlSyntaxWriter> syntaxWriter;
        readonly Lazy<IXmlFormattingPolicyService> formattingPolicyService;
        readonly Lazy<IWorkEngine> workEngine;

        IXmlSyntaxParser XmlSyntaxParser => syntaxParser.Value;
        IXmlSyntaxWriter XmlSyntaxWriter => syntaxWriter.Value;
        IXmlFormattingPolicyService FormattingPolicyService => formattingPolicyService.Value;
        IWorkEngine WorkEngine => workEngine.Value;

        [ImportingConstructor]
        public ConvertProjectToCSharp9Service(
            Lazy<IXmlSyntaxParser> syntaxParser,
            Lazy<IXmlSyntaxWriter> syntaxWriter,
            Lazy<IXmlFormattingPolicyService> formattingPolicyService,
            Lazy<IWorkEngine> workEngine)
        {
            this.syntaxParser = syntaxParser;
            this.syntaxWriter = syntaxWriter;
            this.formattingPolicyService = formattingPolicyService;
            this.workEngine = workEngine;
        }

        public bool CanConvertProject(Project project)
        {
            var projectSyntax = XmlSyntaxParser.ParseFile(project.FilePath);
            if (!projectSyntax.IsDotNetProject())
            {
                return false;
            }

            if (!(project.IsMobileProject() || projectSyntax.IsNetStandardProject()))
            {
                return false;
            }

            var langVersion = GetLangVersionFromSyntaxTree(projectSyntax);
            if (langVersion is not null && langVersion.Value.In(CSharpVersion.V9.GetValue(), CSharpVersion.Preview.GetValue()))
            {
                return false;
            }

            return true;
        }

        public void Convert(Project project)
        {
            var projectSyntax = XmlSyntaxParser.ParseFile(project.FilePath);
            if (!projectSyntax.IsDotNetProject())
            {
                return;
            }

            AddSupportClassIfNeeded(project, projectSyntax);
            UpdateProjectSettings(project, projectSyntax);
        }

        void AddSupportClassIfNeeded(Project project, XmlSyntaxTree projectSyntaxTree)
        {
            if (projectSyntaxTree.IsNetStandardProject())
            {
                var fileWorkUnit = new CreateProjectFileWorkUnit(compatClassContent, "Properties\\CSharp9Support.cs", project) { ShouldOpen = false };
                Task.Run(async () => await WorkEngine.ApplyAsync(fileWorkUnit));        // This should be synchronously to avoid clashing with updating the csproj
            }
        }

        void UpdateProjectSettings(Project project, XmlSyntaxTree projectSyntaxTree)
        {
            var langVer = GetLangVersionFromSyntaxTree(projectSyntaxTree);
            if (langVer is not null)
            {
                UpdateLangVersionProperty(project, projectSyntaxTree);
            }
            else
            {
                AddLangVersionProperty(project, projectSyntaxTree);
            }
        }

        void UpdateLangVersionProperty(Project project, XmlSyntaxTree projectSyntaxTree)
        {
            var workUnits = new List<IWorkUnit>();
            var langVerNodes = GetLangVersionNodesFromSyntaxTree(projectSyntaxTree);

            foreach (var langVerNode in langVerNodes)
            {
                var newLangVer = langVerNode.Clone(true, true);
                newLangVer.Value = CSharpVersion.V9.GetValue();

                var defaultPolicy = FormattingPolicyService.GetXmlFormattingPolicy();
                var workUnit = new ReplaceXmlSyntaxWorkUnit
                {
                    New = newLangVer,
                    Existing = langVerNode,
                    FilePath = project.FilePath,
                    ReplaceChildren = true,
                    GenerateClosingTags = true,
                    FormattingPolicy = defaultPolicy,
                };

                workUnits.Add(workUnit);
            }

            WorkEngine.ApplyAsync(workUnits).ConfigureAwait(false);
        }

        void AddLangVersionProperty(Project project, XmlSyntaxTree projectSyntaxTree)
        {
            var firstPropertyGroup = projectSyntaxTree.Root.Children
                .Where(c => c.Name.FullName == "PropertyGroup")
                .FirstOrDefault();

            if (firstPropertyGroup is not null)
            {
                var versionValue = CSharpVersion.V9.GetValue();
                var langVerElement = new LangVersionProperty(versionValue).GetNode();

                // Get the last element of the group that will be the position
                var lastElement = firstPropertyGroup.Children.Last();
                var insertPosition = lastElement.ClosingTagSpan.End;
                var content = XmlSyntaxWriter.WriteNode(langVerElement, GetIdent(2, true), FormattingPolicyService.GetXmlFormattingPolicy(), true, true, true);
                var insertionWorkUnit = new InsertTextWorkUnit(content, insertPosition, project.FilePath);
                WorkEngine.ApplyAsync(insertionWorkUnit).ConfigureAwait(false);
            }
        }

        IEnumerable<XmlNode> GetLangVersionNodesFromSyntaxTree(XmlSyntaxTree syntaxTree) =>
            syntaxTree.Root.Children
                .Where(c => c.Name.FullName == "PropertyGroup" &&
                            c.Children.Any(ch => ch.Name.FullName == LangVersionProperty.ElementName))
                .Select(c => c.Children.First(ch => ch.Name.FullName == LangVersionProperty.ElementName));

        XmlNode GetLangVersionNodeFromSyntaxTree(XmlSyntaxTree syntaxTree) =>
            GetLangVersionNodesFromSyntaxTree(syntaxTree)
                .FirstOrDefault(c => c.Name.FullName == LangVersionProperty.ElementName);

        LangVersionProperty GetLangVersionFromSyntaxTree(XmlSyntaxTree syntaxTree) =>
            GetLangVersionNodeFromSyntaxTree(syntaxTree).ToLangVersion();

        string GetIdent(int level = 1, bool appendLine = false)
        {
            const int identSpaceCount = 2;
            var builder = new StringBuilder();

            if (appendLine)
            {
                builder.Append(Environment.NewLine);
            }
            for (var i = 0; i < level; i++)
            {
                builder.Append(" ".PadLeft(identSpaceCount));
            }

            return builder.ToString();
        }
    }
}
