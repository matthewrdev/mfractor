using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Utilities
{
    public static class MemberDeclarationHelper
	{
        public static MemberDeclaration AsMemberDeclarationModel(this ISymbol symbol)
		{
            if (symbol is IMethodSymbol)
            {
                return (symbol as IMethodSymbol).AsMemberDeclarationModel();
            }
            else if (symbol is IFieldSymbol)
			{
				return (symbol as IFieldSymbol).AsMemberDeclarationModel();
			}
            else if (symbol is IPropertySymbol)
			{
				return (symbol as IPropertySymbol).AsMemberDeclarationModel();
            } 
            else if (symbol is IEventSymbol)
            {
                return (symbol as IEventSymbol).AsMemberDeclarationModel();
            }

            return null;
		}

        public static MemberDeclaration AsMemberDeclarationModel(this IMethodSymbol method)
        {
            var isInterface = method.ContainingType.TypeKind == TypeKind.Interface;

            var parameters = method.Parameters.Select(p => 
            {
                var modifiers = p.CustomModifiers;
                var refType = p.RefKind;

                string defaultValue = null;
                if (p.HasExplicitDefaultValue)
                {
                    if (p.ExplicitDefaultValue != null)
                    {
                        defaultValue = p.ExplicitDefaultValue.ToString ();
                    } 
                    else
                    {
                        if (p.DeclaringSyntaxReferences.Any ())
                        {
                            var syntax = p.DeclaringSyntaxReferences.First ().GetSyntax ();

                            var code = syntax.ToString ();

                            var split = code.Split ('=');

                            if (split.Length >= 2)
                            {
                                defaultValue = split [1];
                            }
                        }

                        if (string.IsNullOrEmpty (defaultValue)) {
                            defaultValue = "null";
                        }
                    }
                }

                var isOutParam = p.RefKind == RefKind.Out;
                var isRefParam = p.RefKind == RefKind.Ref;

                return new MethodParameter(p.Type, p.Name, defaultValue)
                {
                    IsOutParameter = isOutParam,
                    IsRefParameter = isRefParam,
                };
            }).ToList();

            return new MemberDeclaration(method.ReturnType, method.Name, MemberType.Method, method.DeclaredAccessibility, parameters)
            {
                IsOverride = method.IsAbstract && !isInterface
            };
        }

        public static MemberDeclaration AsMemberDeclarationModel(this IFieldSymbol field)
		{
            return new MemberDeclaration(field.Type, field.Name, MemberType.Field, field.DeclaredAccessibility);
		}

        public static MemberDeclaration AsMemberDeclarationModel(this IPropertySymbol property)
        {
            var isInterface = property.ContainingType.TypeKind == TypeKind.Interface;

            return new MemberDeclaration(property.Type, property.Name, MemberType.Property, property.DeclaredAccessibility)
			{
                IsOverride = property.IsAbstract && !isInterface,
                HasGetter = property.GetMethod != null,
                HasSetter = property.SetMethod != null,
			};
		}

        public static MemberDeclaration AsMemberDeclarationModel(this IEventSymbol eventSymbol)
        {
            var isInterface = eventSymbol.ContainingType.TypeKind == TypeKind.Interface;

            return new MemberDeclaration(eventSymbol.Type, eventSymbol.Name, MemberType.EventHandler, eventSymbol.DeclaredAccessibility)
            {
                IsOverride = eventSymbol.IsAbstract && !isInterface
            };
        }
    }
}
