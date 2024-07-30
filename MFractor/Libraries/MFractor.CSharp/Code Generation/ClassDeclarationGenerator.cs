using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Configuration.Attributes;
using MFractor.CSharp.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IClassDeclarationGenerator))]
    class ClassDeclarationGenerator : MFractor.Code.CodeGeneration.CSharp.CSharpCodeGenerator, IClassDeclarationGenerator
    {
        public override string Documentation => "Generates a new class declaration implementing all provided members, base class constructors, base class abstract invocations and interfaces";

        public override string Identifier => "com.mfractor.code_gen.csharp.class_declaration";

        public override string Name => "Create Class Declaration";

        [ExportProperty("Should the base class constructors be automatically created?")]
        public bool ImplementBaseConstructors { get; set; }

        [Import]
        public IMemberFieldGenerator MemberFieldGenerator { get; set; }

        [Import("Default")]
        public IInstancePropertyGenerator InstancePropertyGenerator { get; set; }

        [Import]
        public IMethodGenerator MethodGenerator { get; set; }

        [Import]
        public IBaseConstructorGenerator BaseConstructorGenerator { get; set; }

        [Import]
        public IEventHandlerDeclarationGenerator EventHandlerDeclarationGenerator { get; set; }

        public IEnumerable<MemberDeclarationSyntax> GenerateMembers(IEnumerable<MemberDeclaration> members, 
                                                             IEnumerable<MemberDeclaration> membersToExclude,
                                                             INamedTypeSymbol baseType)
		{
            var result = new List<MemberDeclarationSyntax>();

            var baseMembers = new List<MemberDeclaration>();

            if (baseType != null)
            {
                var symbolMembers = baseType.GetMembers();

                baseMembers = symbolMembers.Where(m => m.IsStatic == false && m.IsAbstract == false)
                                           .Where(m => !m.DeclaredAccessibility.HasFlag(Accessibility.Private))
	                                       .Select(m => m.AsMemberDeclarationModel())
	                                       .Where(m => m != null).ToList();
            }

            // TODO: 

            if (members != null && members.Any())
            {
                foreach (var member in members)
                {
                    if (membersToExclude != null
                        && membersToExclude.Contains(member))
                    {
                        continue;
                    }

                    if (baseMembers.Any(m => m.Matches(member)))
                    {
                        continue;
                    }


                    switch (member.MemberType)
                    {
                        case MemberType.Method:
                            result.Add(MethodGenerator.GenerateMethodSyntax(member));
                            break;
                        case MemberType.Field:
                            result.Add(MemberFieldGenerator.GenerateSyntax(member.NamedType, member.Name, null));
                            break;
                        case MemberType.Property:
                            result.AddRange(InstancePropertyGenerator.GenerateSyntax(member.NamedType, member.Accessibility, member.Name, null));
                            break;
                        case MemberType.EventHandler:
                            result.AddRange(EventHandlerDeclarationGenerator.GenerateSyntax(member.Name, member.NamedType.ToString()));
                            break;
                        default:
                            break;
                    }
                }
            }

            return result;
        }

        public IEnumerable<MemberDeclarationSyntax> GenerateMembers(IEnumerable<ISymbol> members, IEnumerable<MemberDeclaration> membersToExclude, out List<MemberDeclaration> excludedMembers)
		{
            var result = new List<MemberDeclarationSyntax>();
            excludedMembers = new List<MemberDeclaration>();

			foreach (var member in members)
			{

				if (member is IMethodSymbol method)
				{
					var model = method.AsMemberDeclarationModel();

                    if (method.AssociatedSymbol != null)
                    {
                        continue; // This is a getter, setter, add or remove body.
                    }

					if (membersToExclude != null)
					{
						var match = membersToExclude.FirstOrDefault(m => m.Matches(model));
						if (match != null)
						{
							excludedMembers.Add(match);
						}
					}

					var methodSyntax = MethodGenerator.GenerateMethodSyntax(model);

					result.Add(methodSyntax);
				}
				else if (member is IPropertySymbol prop)
				{
					var model = prop.AsMemberDeclarationModel();

					if (membersToExclude != null)
					{
						var match = membersToExclude.FirstOrDefault(m => m.Matches(model));
						if (match != null)
						{
							excludedMembers.Add(match);
						}
					}

					var propertyDeclaration = InstancePropertyGenerator.GenerateSyntax(model);

					result.AddRange(propertyDeclaration);
                } else if (member is IEventSymbol eventSymbol)
                {
                    var model = eventSymbol.AsMemberDeclarationModel();

                    if (membersToExclude != null)
                    {
                        var match = membersToExclude.FirstOrDefault(m => m.Matches(model));
                        if (match != null)
                        {
                            excludedMembers.Add(match);
                        }
                    }

                    var eventHandlerType = model.NamedType.ToString();

                    var eventDeclaration = EventHandlerDeclarationGenerator.GenerateSyntax(model.Name, eventHandlerType);

                    result.AddRange(eventDeclaration);
                }
			}

            return result;
        }

        public ClassDeclarationSyntax GenerateSyntax(string className,
                                                     string baseTypeName,
                                                     INamedTypeSymbol baseTypeSymbol,
                                                     IEnumerable<MemberDeclaration> members)
        {
            var classDeclaration = SyntaxFactory.ClassDeclaration(className)
                               .WithModifiers(
                                   SyntaxFactory.TokenList(
                                       SyntaxFactory.Token(SyntaxKind.PublicKeyword)))
                            .NormalizeWhitespace();

            var baseTypes = new List<BaseTypeSyntax>();
            var classMembers = new List<MemberDeclarationSyntax>();
            var skipMembers = new List<MemberDeclaration>();

            if (!string.IsNullOrEmpty(baseTypeName))
            {
                var baseTypeSyntax = SyntaxFactory.ParseTypeName(baseTypeName);

                baseTypes.Add(SyntaxFactory.SimpleBaseType(baseTypeSyntax));
            }

            if (baseTypeSymbol != null)
            {
                if (baseTypeSymbol.IsAbstract)
                {
                    var abstractMembers = baseTypeSymbol.GetMembers().Where(m => m.IsAbstract).ToList();

                    classMembers.AddRange(GenerateMembers(abstractMembers, members, out skipMembers));
                }

                var constructors = baseTypeSymbol.Constructors;

                if (constructors.Any())
                {
                    foreach (var c in constructors)
                    {
                        if (c.Parameters.Any())
                        {
                            classMembers.Add(BaseConstructorGenerator.GenerateSyntax(c, className));
                        }
                    }
                }
            }

            var newMembers = GenerateMembers(members, skipMembers, baseTypeSymbol);

            if (newMembers != null && newMembers.Any())
            {
                classMembers.AddRange(newMembers);
            }

            if (classMembers != null && classMembers.Any())
            {
                classDeclaration = classDeclaration.WithMembers(SyntaxFactory.List(classMembers));
            }

            if (baseTypes != null && baseTypes.Any())
            {
                var baseList = SyntaxFactory.BaseList(SyntaxFactory.SeparatedList(baseTypes));

                classDeclaration = classDeclaration.WithBaseList(baseList).NormalizeWhitespace();
            }

            return classDeclaration;
        }

        public ClassDeclarationSyntax GenerateSyntax(string className,
                                                     INamedTypeSymbol baseType,
                                                     IEnumerable<MemberDeclaration> members)
        {
            return GenerateSyntax(className, baseType.ToString(), baseType, members);
        }

        public ClassDeclarationSyntax GenerateSyntax(string className, 
                                                     string baseType, 
                                                     IEnumerable<MemberDeclaration> members)
        {
            return GenerateSyntax(className, baseType, null, members);
        }
    }
}
