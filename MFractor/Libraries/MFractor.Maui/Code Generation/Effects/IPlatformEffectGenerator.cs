using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration;
using MFractor.CodeSnippets;
using MFractor.Code.Documents;
using MFractor.Work;
using MFractor.Work.WorkUnits;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Options;
using CompilationWorkspace = Microsoft.CodeAnalysis.Workspace;

namespace MFractor.Maui.CodeGeneration.CSharp
{
    public interface IPlatformEffectGenerator : ICodeGenerator
    {
        string IOSEffectsFolder { get; set; }

        string AndroidEffectsFolder { get; set; }

        string EffectsFolder { get; set; }

        string ResolutionGroupName { get; set; }

        ICodeSnippet IOSEffectSnippet { get; set; }

        ICodeSnippet AndroidEffectSnippet { get; set; }

        ICodeSnippet EffectSnippet { get; set; }

        CompilationUnitSyntax GenerateEffectSyntax(string name, string effectNamespace);

        CompilationUnitSyntax GenerateiOSEffectSyntax(string name, string effectNamespace);

        CompilationUnitSyntax GenerateAndroidEffectSyntax(string name, string effectNamespace);

        IReadOnlyList<IWorkUnit> Generate(string effectName,
                                        string effectXamlNamespace,
                                        IParsedXmlDocument document,
                                        Project project,
                                        CompilationWorkspace workspace,
                                        OptionSet formattingOptions,
                                        string projectNamespace,
                                        bool shouldCreateCommonEffect,
                                        ReplaceTextWorkUnit insertEffectNameWorkUnit = null);
    }
}
