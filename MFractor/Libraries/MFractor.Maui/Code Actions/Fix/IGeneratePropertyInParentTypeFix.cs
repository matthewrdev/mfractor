using System.Collections.Generic;
using MFractor.Code.CodeActions;
using MFractor.Xml;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.Maui.CodeActions.Fix
{
    public interface IGeneratePropertyIntoParentTypeFix : IFixCodeAction
    {
        bool TryGetClassSyntax(XmlAttribute element, IXamlFeatureContext context, out INamedTypeSymbol targetSymbol, out ClassDeclarationSyntax classSyntax);
        IEnumerable<MemberDeclarationSyntax> GeneratePropertySyntax(XmlAttribute element, IXamlFeatureContext context);
        IEnumerable<MemberDeclarationSyntax> GenerateBindablePropertySyntax(XmlAttribute element, IXamlFeatureContext context);
    }
}
