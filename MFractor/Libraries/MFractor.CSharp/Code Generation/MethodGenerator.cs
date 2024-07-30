using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace MFractor.CSharp.CodeGeneration
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IMethodGenerator))]
    class MethodGenerator : MFractor.Code.CodeGeneration.CSharp.CSharpCodeGenerator, IMethodGenerator
    {
        public override string Identifier => "com.mfractor.code_gen.csharp.method_declaration";

        public override string Name => "Method Generator";

        public override string Documentation => "Generates a new method declaration";

        public MethodDeclarationSyntax GenerateMethodSyntax(MemberDeclaration memberDeclaration)
        {
            var typeSyntax = SyntaxFactory.ParseTypeName(memberDeclaration.NamedType.ToDisplayString());

            var name = SyntaxFactory.ParseToken(memberDeclaration.Name);

            var method = SyntaxFactory.MethodDeclaration(
                typeSyntax,
                    name);

            method = method.WithModifiers(memberDeclaration.Modifiers());

            var parameters = GenerateMethodParameters(memberDeclaration.Parameters);

            method = method.WithBody(
                SyntaxFactory.Block(
                SyntaxFactory.SingletonList<StatementSyntax>(
                    SyntaxFactory.ThrowStatement(
                        SyntaxFactory.ObjectCreationExpression(
                            SyntaxFactory.IdentifierName("NotImplementedException"))
                            .WithArgumentList(SyntaxFactory.ArgumentList ())))))
                           .WithParameterList (parameters);

            return method;
        }

        private ParameterListSyntax GenerateMethodParameters (List<MethodParameter> parameters)
        {
            var result = SyntaxFactory.ParameterList ();
            
            if (parameters == null || !parameters.Any())
            {
                return result;
            }

            foreach (var p in parameters)
            {
                var type = SyntaxFactory.ParseTypeName (p.Type.ToString ());
                ParameterSyntax param;
                param = SyntaxFactory.Parameter (
                                    SyntaxFactory.Identifier (p.Name))
                                                         .WithType (type);
                if (!string.IsNullOrEmpty (p.DefaultValue))
                {
                    var value = p.DefaultValue;
                    var typeString = p.Type.ToString ();
                    if (typeString.Equals ("System.String", StringComparison.OrdinalIgnoreCase)
                        || typeString.Equals ("string", StringComparison.OrdinalIgnoreCase)
                        && p.DefaultValue.Trim ().ToLower () != "null" )
                    {
                        value = $"\"{value}\"";
                    }
                    
                    var expression = SyntaxFactory.ParseExpression (value);
                    var defaultValue = SyntaxFactory.EqualsValueClause (expression);

                    param = param.WithDefault (defaultValue);
                }

                if (p.IsOutParameter)
                {
                    param = param.AddModifiers(SyntaxFactory.Token(SyntaxKind.OutKeyword));
                }

                if (p.IsRefParameter)
                {
                    param = param.AddModifiers(SyntaxFactory.Token(SyntaxKind.RefKeyword));
                }

                result = result.AddParameters (param);
            }

            return result;
        }
    }
}
