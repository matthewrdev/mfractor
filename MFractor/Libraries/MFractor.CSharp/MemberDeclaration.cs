using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.CSharp.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace MFractor.CSharp
{
    public class MemberDeclaration
    {
        public ITypeSymbol NamedType { get; }

        public string Name { get; }

        public MemberType MemberType { get; }

        public Accessibility Accessibility { get; }

        public List<MethodParameter> Parameters { get; }

        public bool IsOverride { get; set; } = false;

        public bool HasGetter { get; set; } = true;
        public bool HasSetter { get; set; } = true;

        public MemberDeclaration(ITypeSymbol namedType, 
                                 string name, 
                                 MemberType memberType, 
                                 Accessibility accessibility)
        {
            NamedType = namedType;
            Name = name;
            MemberType = memberType;
            Accessibility = accessibility;
        }

        public MemberDeclaration(ITypeSymbol namedType, 
                                 string name,
								 MemberType memberType,
								 Accessibility accessibility, 
                                 IEnumerable<MethodParameter> parameters)
        {
            NamedType = namedType;
            Name = name;
            MemberType = memberType;
            Accessibility = accessibility;
            Parameters = (parameters ?? Enumerable.Empty<MethodParameter>()).ToList();
        }

        public bool Matches(MemberDeclaration member)
        {
            if (member.MemberType != this.MemberType
                || member.Name != this.Name
                || member.NamedType != this.NamedType)
			{
				return false;
			}

            if (this.MemberType == MemberType.Method)
            {
                if (Parameters == null && member.Parameters != null)
                {
                    return false;
                }

				if (Parameters != null && member.Parameters == null)
				{
					return false;
				}

                if (Parameters.Count != member.Parameters.Count)
				{
					return false;
				}

                for (var i = 0; i < Parameters.Count; ++i)
                {
					var left = Parameters[i];
					var right = member.Parameters[i];

                    if (left.Type != right.Type
                        || left.Name != right.Name
                        || left.DefaultValue != right.DefaultValue
                        || left.IsOutParameter != right.IsOutParameter
                        || left.IsRefParameter != right.IsRefParameter)
                    {
                        return false;
                    }

                }

                return true;
            }

            return true;
        }

        public SyntaxTokenList Modifiers()
        {
            var modifiers = Accessibility.AsList() ?? new List<SyntaxToken>();

            if (IsOverride)
            {
                modifiers.Add(SyntaxFactory.Token(SyntaxKind.OverrideKeyword));
            }

            return SyntaxFactory.TokenList(modifiers.ToArray());
        }
    }
}
