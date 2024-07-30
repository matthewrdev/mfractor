using MFractor.Maui.Syntax;
using NUnit.Framework;
using System;

namespace MFractor.Maui.Tests
{
    [TestFixture()]
    public class XamlExpressionParserTests
    {
        [Test()]
        public void CanParseXamlExpression()
        {
            //                         000000000011111111112222222
            //                         012345678901234567901234567
            const string Expression = "{Binding MyPropertyBinding}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse() as ExpressionSyntax;

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());

            Assert.AreEqual(syntax.ToString().Length, syntax.FullSpan.Length);

            Assert.IsNotNull(syntax);
            Assert.IsNotEmpty(syntax.Children);
            Assert.AreEqual(2, syntax.Children.Count);

            Assert.AreEqual("{", syntax.Leading);
            Assert.AreEqual("}", syntax.Trailing);

            Assert.AreEqual(0, syntax.FullSpan.Start);
            Assert.AreEqual(27, syntax.FullSpan.End);

            Assert.AreEqual(1, syntax.Span.Start);
            Assert.AreEqual(26, syntax.Span.End);

            var name = syntax.Children[0] as TypeNameSyntax;
            var content = syntax.Children[1] as ContentSyntax;

            Assert.IsNotNull(name);
            Assert.AreEqual("Binding", name.Name);

            Assert.AreEqual(string.Empty, name.Leading);
            Assert.AreEqual(name.Trailing, " ");

            Assert.AreEqual(1, name.FullSpan.Start);
            var capture = Expression.Substring(name.FullSpan.Start, name.Leading.Length + name.Name.Length + name.Trailing.Length);
            Assert.AreEqual(9, name.FullSpan.Start + name.Leading.Length + name.Name.Length + name.Trailing.Length);

            Assert.AreEqual(1, name.Span.Start);
            Assert.AreEqual(8, name.Span.Start + name.Name.Length);

            Assert.AreEqual(name.ToString().Length, name.FullSpan.Length);

            Assert.IsNotNull(content);
            Assert.AreEqual("MyPropertyBinding", content.Content);

            Assert.AreEqual(string.Empty, content.Leading);
            Assert.AreEqual(string.Empty, content.Trailing);

            Assert.AreEqual(9, content.FullSpan.Start);
            Assert.AreEqual(26, content.FullSpan.Start + content.Leading.Length + content.Content.Length + content.Trailing.Length);

            Assert.AreEqual(9, content.Span.Start);
            Assert.AreEqual(26, content.Span.Start + content.Content.Length);
        }

        [Test()]
        public void CanParseDotBindingExpressions()
        {
            //                         000000000011111111112222222223333
            //                         012345678901234567901234567890123
            const string Expression = "{Binding .}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test()]
        public void CanParseNamespacedTypeXamlExpression()
        {
            //                         000000000011111111112222222223333
            //                         012345678901234567901234567890123
            const string Expression = "{x:Static MyContent}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test()]
        public void CanParsePropertyAssignment()
        {
            //                         000000000011111111112222222223333
            //                         012345678901234567901234567890123
            const string Expression = "{Binding Path=Test123}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test()]
        public void CanParseMemberAccessExpression()
        {
            //                         000000000011111111112222222223333
            //                         012345678901234567901234567890123
            const string Expression = "{Binding local:MyClass.MyMember}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test()]
        public void CanParseUnnamepsacedMemberAccessExpression()
        {
            //                         000000000011111111112222222223333
            //                         012345678901234567901234567890123
            const string Expression = "{Binding MyClass.MyMember}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test()]
        public void CanParseStringValueExpression()
        {
            //                         00000000001111111111222222222333333333344444444
            //                         01234567890123456790123456789 01234 5678901234567
            const string Expression = "{Binding StringFormat= 'ABDX \\'Test\\' {addsd} '}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test()]
        public void CanParseNestedExpressionInValue()
        {
            //                         000000000011111111112222222223333
            //                         012345678901234567901234567890123
            const string Expression = "{Binding Converter={StaticResource myConverter} } ";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test()]
        public void CanParseMultiComponentExpression()
        {
            //                         000000000011111111112222222223333333333444444444455555555556666666666777777777788888888889999999999
            //                         01234567890123456790123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890
            const string Expression = "{Binding Value, StringFormat='Testing {a}', Path={x:Reference myValue}, Converter={StaticResource myConverter}}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());

            Assert.AreEqual(5, syntax.Children.Count);

            var type = syntax.Children[0] as TypeNameSyntax;
            var value = syntax.Children[1] as ContentSyntax;
            var stringFormat = syntax.Children[2] as AssignmentSyntax;
            var path = syntax.Children[3] as AssignmentSyntax;
            var converter = syntax.Children[4] as AssignmentSyntax;

            Assert.IsNotNull(type);
            Assert.IsNotNull(value);
            Assert.IsNotNull(stringFormat);
            Assert.IsNotNull(path);
            Assert.IsNotNull(converter);
        }

        [Test]
        public void CanParsePartialExpression()
        {
            const string Expression = "{Binding Val";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }


        [Test]
        public void CanParsePartialNestedExpression()
        {
            const string Expression = "{Binding Source={StaticResour";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }


        [Test]
        public void CanParsePartialStringValueExpression()
        {
            const string Expression = "{Binding StringFormat='Testing {";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test]
        public void CanParseExpressionWithExcessiveWhiteSpace()
        {
            const string Expression = "{ Binding StringFormat = 'Testing' } ";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();
            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }


        [Test]
        public void CanParseSyntacticallyIncorrectExpressions()
        {
            const string Expression = "{Binding BindingValue StringValue='Testing'}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();
            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());

            Assert.AreEqual(3, syntax.Children.Count);
        }

        [Test]
        public void CanParseExpressionsWithEmptySyntaxElement()
        {
            const string Expression = "{Binding BindingValue, }";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();
            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test]
        public void CanParseEmptyStringValueExpression()
        {
            const string Expression = "{Binding StringValue=''}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }

        [Test]
        public void CanParseEmptyNestedExpression()
        {
            const string Expression = "{Binding Source={ }}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            // Verify round trip.
            Assert.AreEqual(Expression, syntax.ToString());
        }


        [Test()]
        public void CanParseSymbolExpressionWithExcessiveWhitespace()
        {
            //                         000000000011111111112222222223333
            //                         012345678901234567901234567890123
            const string Expression = "{local : CustomExtension Test}";

            var parser = new MFractor.Maui.Syntax.XamlExpressionParser(Expression, 0);

            var syntax = parser.Parse();

            Assert.AreEqual(Expression, syntax.ToString());

            Assert.AreEqual(2, syntax.Children.Count);

            var symbol = syntax.Children[0] as SymbolSyntax;
            var content = syntax.Children[1] as ContentSyntax;

            Assert.IsNotNull(symbol);
            Assert.IsNotNull(content);

            Assert.AreEqual("local : CustomExtension ", symbol.ToString());
            Assert.AreEqual("Test", content.ToString());

            Assert.AreEqual(2, symbol.Children.Count);

            var ns = symbol.Children[0] as NamespaceSyntax;
            var type = symbol.Children[1] as TypeNameSyntax;

            Assert.IsNotNull(ns);
            Assert.IsNotNull(type);

            Assert.AreEqual("local :", ns.ToString());
            Assert.AreEqual(" CustomExtension ", type.ToString());
        }
    }
}
