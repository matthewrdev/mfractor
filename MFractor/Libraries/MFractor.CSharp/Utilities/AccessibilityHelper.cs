using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MFractor.CSharp.Utilities
{
    public static class AccessibilityHelper
    {
        public static SyntaxTokenList AsSyntaxTokenList(this Accessibility accesibility)
        {
            var list = accesibility.AsList();

            if (list == null || !list.Any())
            {
                return default;
            }

			return SyntaxFactory.TokenList(list.ToArray());
        }

        public static List<SyntaxToken> AsList(this Accessibility accesibility)
        {
            switch (accesibility)
            {
                case Accessibility.NotApplicable:
                    return null;
                case Accessibility.Private:
                    return new List<SyntaxToken>() { SyntaxFactory.Token(SyntaxKind.PrivateKeyword) };
                case Accessibility.ProtectedAndInternal:
                    return new List<SyntaxToken>() { SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.InternalKeyword) };
                case Accessibility.Protected:
                    return new List<SyntaxToken>() { SyntaxFactory.Token(SyntaxKind.ProtectedKeyword) };
                case Accessibility.Internal:
                    return new List<SyntaxToken>() { SyntaxFactory.Token(SyntaxKind.InternalKeyword) };
                case Accessibility.ProtectedOrInternal:
                    return new List<SyntaxToken>() { SyntaxFactory.Token(SyntaxKind.ProtectedKeyword), SyntaxFactory.Token(SyntaxKind.InternalKeyword) };
                case Accessibility.Public:
                    return new List<SyntaxToken>() { SyntaxFactory.Token(SyntaxKind.PublicKeyword) };
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}
