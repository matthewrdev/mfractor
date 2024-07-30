using System;
using System.Linq;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Maui.Syntax
{
    /// <summary>
    /// Matt R: I wrote this but I really have no idea how this even works, use with caution.
    /// </summary>
    public class XamlExpressionParser
    {
        int index;
        int Start { get; }

        int readCount = 0;

        string Expression { get; }

        public XamlExpressionParser(string expression, int start)
        {
            if (start < 0)
            {
                throw new ArgumentException($"{nameof(start)} may not be less than zero");
            }

            index = 0;
            Start = start;
            Expression = expression ?? throw new ArgumentNullException(nameof(expression));
        }

        public XamlExpressionParser(string expression, TextSpan span)
            : this(expression, span.Start)
        {
        }

        bool CanAdvance()
        {
            _ = index + 1;
            if (index < Expression.Length)
            {
                return true;
            }

            return false;
        }

        void Advance()
        {
            if (!allowAdvance)
            {
                throw new Exception("Current character has not been read!");
            }

            index++;
            allowAdvance = false;
        }

        bool allowAdvance = true;

        char CurrentCharacter()
        {
            allowAdvance = true;

            if (index >= Expression.Length)
            {
                throw new IndexOutOfRangeException();
            }

            readCount++;
            if (readCount >= Expression.Length * 50)
            {
                throw new InvalidOperationException("Potentially stuck read loop, the read counter is now fifty times longer than the expression length");
            }

            return Expression[index];
        }

        char? PeekNextCharacter()
        {
            if (index + 1 >= Expression.Length)
            {
                return null;
            }

            return Expression[index + 1];
        }

        int Position => Start + index;

        int NextPosition => Position + 1;

        public XamlExpressionSyntaxNode Parse()
        {
            var expression = new ExpressionParserState(null, Position, XamlExpressionSyntaxKind.Expression);

            var trailing = string.Empty;

            // Read all leading trivia
            expression.FullSpan = TextSpan.FromBounds(Position, Position);
            var character = CurrentCharacter();
            var leading = character.ToString();

            while ((character != '{' || char.IsWhiteSpace(character))
                   && CanAdvance())
            {
                Advance();
                character = CurrentCharacter();
                leading += character;
            }

            Advance();

            expression.Leading = leading;
            expression.SetSpanStart(Position);

            ProcessExpression(expression, false);

            while (CanAdvance())
            {
                character = CurrentCharacter();
                trailing += character;
                Advance();
            }

            expression.Trailing += trailing;
            expression.SetFullSpanEnd(Position);

            var result = ExpressionFactory.CreateExpression(expression);

            return result;
        }

        internal void ProcessAssignmentRightHandExpression(ExpressionParserState assignment)
        {
            var kind = InferAssignmentKind();

            try
            {
                switch (kind)
                {
                    case XamlExpressionSyntaxKind.Expression:
                        {
                            var expression = assignment.CreateChild(Position, XamlExpressionSyntaxKind.Expression);

                            var character = CurrentCharacter();
                            var leading = character.ToString();

                            while ((character != '{' || char.IsWhiteSpace(character))
                                   && CanAdvance())
                            {
                                Advance();
                                character = CurrentCharacter();
                                leading += character;
                            }

                            Advance();

                            expression.Leading = leading;

                            ProcessExpression(expression, true);
                        }
                        break;
                    case XamlExpressionSyntaxKind.Value:
                        {
                            ProcessAssignmentValue(assignment);
                        }
                        break;
                    case XamlExpressionSyntaxKind.StringValue:
                        {
                            ProcessStringValue(assignment);
                        }
                        break;
                    default:

                        break;
                }
            }
            finally
            {
                assignment.SetFullSpanEnd(NextPosition);
            }
        }

        XamlExpressionSyntaxKind InferAssignmentKind()
        {
            var initialIndex = index;

            var kind = XamlExpressionSyntaxKind.Value;
            try
            {
                for (; CanAdvance(); Advance())
                {
                    var character = CurrentCharacter();

                    if (character == ',' || character == '}')
                    {
                        return kind;
                    }
                    else if (character == '{')
                    {
                        return XamlExpressionSyntaxKind.Expression;
                    }
                    else if (character == '\'')
                    {
                        return XamlExpressionSyntaxKind.StringValue;
                    }
                    else if (character == '.'
                             || character == ':'
                             || character == '=')
                    {
                        return XamlExpressionSyntaxKind.Error;
                    }
                    else if (char.IsWhiteSpace(character))
                    {
                        continue;
                    }
                    else
                    {
                        return XamlExpressionSyntaxKind.Value;
                    }
                }
            }
            finally
            {
                index = initialIndex;
            }

            return kind;
        }

        internal ExpressionParserState ProcessAssignmentValue(ExpressionParserState assignment)
        {
            var value = assignment.CreateChild(Position, XamlExpressionSyntaxKind.Value);
            var mode = ParserCharacterMode.Leading;

            for (; CanAdvance(); Advance())
            {
                var character = CurrentCharacter();

                switch (mode)
                {
                    case ParserCharacterMode.Leading:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                value.Leading += character;
                            }
                            else if (character == ',')
                            {
                                value.SetSpanEnd(NextPosition);
                                mode = ParserCharacterMode.Trailing;
                                goto case ParserCharacterMode.Trailing;
                            }
                            else
                            {
                                value.SetSpanStart(Position);
                                mode = ParserCharacterMode.Element;
                                goto case ParserCharacterMode.Element;
                            }
                        }
                        break;
                    case ParserCharacterMode.Element:
                        {
                            if (char.IsWhiteSpace(character)
                               || character == ',')
                            {
                                value.SetSpanEnd(NextPosition);
                                mode = ParserCharacterMode.Trailing;
                                goto case ParserCharacterMode.Trailing;
                            }
                            else
                            {
                                value.Content += character;
                            }
                        }
                        break;
                    case ParserCharacterMode.Trailing:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                value.Trailing += character;
                            }
                            else
                            {
                                value.SetFullSpanEnd(NextPosition);
                                return value;
                            }
                        }
                        break;
                }


                var next = PeekNextCharacter();
                if (next == null
                    || next == '}'
                    || next == ',')
                {
                    value.SetSpanEnd(NextPosition);
                    value.SetFullSpanEnd(NextPosition);
                    Advance();
                    break;
                }
            }

            return value;
        }

        internal ExpressionParserState ProcessStringValue(ExpressionParserState assignmentState)
        {
            var value = assignmentState.CreateChild(Position, XamlExpressionSyntaxKind.StringValue);

            var mode = ParserCharacterMode.Leading;

            for (; CanAdvance(); Advance())
            {
                var character = CurrentCharacter();
                switch (mode)
                {
                    case ParserCharacterMode.Leading:
                        {
                            if (character == '\'')
                            {
                                value.Leading += character;
                                value.SetSpanStart(Position);
                                mode = ParserCharacterMode.Element;
                            }
                            else if (character == ',')
                            {
                                value.SetSpanEnd(NextPosition);
                                value.SetFullSpanEnd(NextPosition);
                                value.Trailing += character;
                                return value;
                            }
                            else
                            {
                                value.Leading += character;
                            }
                        }
                        break;
                    case ParserCharacterMode.Element:
                        {
                            if (character == '\'')
                            {
                                if (!string.IsNullOrEmpty(value.Content) && value.Content.Last() == '\\')
                                {
                                    value.Content += character;
                                }
                                else
                                {
                                    value.SetSpanEnd(NextPosition);
                                    goto case ParserCharacterMode.Trailing;
                                }
                            }
                            else
                            {
                                value.Content += character;
                            }
                        }
                        break;
                    case ParserCharacterMode.Trailing:
                        {
                            if (character == '\'')
                            {
                                value.SetFullSpanEnd(NextPosition);
                                value.Trailing += character;
                                return value;
                            }
                            else if (char.IsWhiteSpace(character))
                            {
                                value.Trailing += character;
                            }
                            else if (character == ',')
                            {
                                value.SetFullSpanEnd(NextPosition);
                                return value;
                            }

                        }
                        break;
                }


                var next = PeekNextCharacter();

                if (mode == ParserCharacterMode.Trailing)
                {
                    if (next == null
                        || next == '}'
                        || next == ',')
                    {
                        value.SetSpanEnd(NextPosition);
                        value.SetFullSpanEnd(NextPosition);
                        Advance();
                        break;
                    }
                }

                if (next == '\'' && (string.IsNullOrEmpty(value.Content) || value.Content.Last() != '\\') && mode != ParserCharacterMode.Leading)
                {
                    value.SetSpanEnd(NextPosition);
                    mode = ParserCharacterMode.Trailing;
                }
            }

            return value;
        }

        internal void ProcessExpression(ExpressionParserState expressionState, bool isNested)
        {
            var symbolKind = InferNextSymbolKind();

            switch (symbolKind)
            {
                case XamlExpressionSyntaxKind.Symbol:
                    ProcessSymbolSyntax(expressionState);
                    break;
                case XamlExpressionSyntaxKind.TypeName:
                    ProcessType(expressionState);
                    break;
                case XamlExpressionSyntaxKind.Error:
                    ProcessErrorSyntax(expressionState);
                    break;
            }

            if (!CanAdvance())
            {
                expressionState.SetFullSpanEnd(NextPosition);
                return;
            }

            Advance();

            ProcessExpressionChildElements(expressionState);

            FinaliseExpressionTrailingCharacters(expressionState);

            if (!isNested && CanAdvance())
            {
                Advance();
            }
        }

        ExpressionParserState ProcessType(ExpressionParserState parent)
        {
            var symbol = parent.CreateChild(Position, XamlExpressionSyntaxKind.TypeName);

            var mode = ParserCharacterMode.Leading;

            for (; CanAdvance(); Advance())
            {
                var character = CurrentCharacter();
                switch (mode)
                {
                    case ParserCharacterMode.Leading:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                symbol.Leading += character;
                            }
                            else if (character == ',')
                            {
                                symbol.SetSpanStart(Position);
                                symbol.SetSpanEnd(NextPosition);
                                symbol.SetFullSpanEnd(NextPosition);
                                symbol.Trailing += character;
                                return symbol;
                            }
                            else
                            {
                                symbol.Content += character;
                                symbol.SetSpanStart(Position);
                                mode = ParserCharacterMode.Element;
                            }
                        }
                        break;
                    case ParserCharacterMode.Element:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                symbol.SetSpanEnd(NextPosition - 1);
                                mode = ParserCharacterMode.Trailing;
                                goto case ParserCharacterMode.Trailing;
                            }
                            else if (character == ',')
                            {
                                symbol.SetSpanEnd(NextPosition - 1);
                                symbol.SetFullSpanEnd(NextPosition);
                                symbol.Trailing += character;
                                return symbol;
                            }
                            else
                            {
                                symbol.Content += character;
                            }

                        }
                        break;
                    case ParserCharacterMode.Trailing:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                symbol.Trailing += character;

                                var nextChar = PeekNextCharacter(); // Is this the end of the expression?
                                if (nextChar != null
                                    && (!char.IsWhiteSpace(nextChar.Value) || nextChar != ','))
                                {
                                    symbol.SetFullSpanEnd(NextPosition);
                                    return symbol;
                                }

                            }
                            else if (character == ',')
                            {
                                symbol.SetFullSpanEnd(NextPosition);
                                symbol.Trailing += character;
                                return symbol;
                            }
                            else
                            {
                                symbol.SetFullSpanEnd(NextPosition);
                                return symbol;
                            }
                        }
                        break;
                }


                var next = PeekNextCharacter();
                if (next == null
                    || next == '}'
                    || next == '.') // Check if we are inside a member access expression, if so, the outer type should handle it.
                {
                    if (mode != ParserCharacterMode.Trailing)
                    {
                        symbol.SetSpanEnd(NextPosition);
                    }

                    symbol.SetFullSpanEnd(NextPosition);
                    break;
                }
            }

            return symbol;
        }

        internal ExpressionParserState ProcessSymbolNamespace(ExpressionParserState parent)
        {
            var @namespace = parent.CreateChild(Position, XamlExpressionSyntaxKind.Namespace);

            var mode = ParserCharacterMode.Leading;

            for (; CanAdvance(); Advance())
            {
                var character = CurrentCharacter();
                switch (mode)
                {
                    case ParserCharacterMode.Leading:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                @namespace.Leading += character;
                            }
                            else if (character == ',' || character == ':')
                            {
                                @namespace.SetSpanStart(Position);
                                @namespace.SetSpanEnd(NextPosition);
                                @namespace.Trailing += character;
                                @namespace.SetFullSpanEnd(NextPosition);
                                return @namespace;
                            }
                            else
                            {
                                @namespace.Content += character;
                                @namespace.SetSpanStart(Position);
                                mode = ParserCharacterMode.Element;
                            }
                        }
                        break;
                    case ParserCharacterMode.Element:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                @namespace.SetSpanEnd(NextPosition - 1);
                                @namespace.Trailing += character;
                                mode = ParserCharacterMode.Trailing;
                            }
                            else if (character == ',' || character == ':')
                            {
                                @namespace.SetSpanEnd(NextPosition - 1);
                                @namespace.Trailing += character;
                                @namespace.SetFullSpanEnd(NextPosition);
                                return @namespace;
                            }
                            else
                            {
                                @namespace.Content += character;
                            }
                        }
                        break;
                    case ParserCharacterMode.Trailing:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                @namespace.Trailing += character;
                            }
                            else if (character == ',' || character == ':')
                            {
                                @namespace.Trailing += character;
                                @namespace.SetFullSpanEnd(NextPosition);
                                return @namespace;
                            }
                            else
                            {
                                @namespace.Trailing += character;
                            }
                        }
                        break;
                }

                var next = PeekNextCharacter();
                if (next == null
                    || next == '}'
                    || next == '.') // Check if we are inside a member access expression, if so, the outer type should handle it.
                {
                    if (mode != ParserCharacterMode.Trailing)
                    {
                        @namespace.SetSpanEnd(NextPosition);
                    }

                    @namespace.SetFullSpanEnd(NextPosition);
                    break;
                }
            }

            return @namespace;
        }

        internal ExpressionParserState ProcessSymbolSyntax(ExpressionParserState parent)
        {
            var symbol = parent.CreateChild(Position, XamlExpressionSyntaxKind.Symbol);

            try
            {
                ProcessSymbolNamespace(symbol);

                Advance();

                ProcessType(symbol);

            }
            finally
            {
                symbol.SetSpanEnd(NextPosition);
                symbol.SetFullSpanEnd(NextPosition);
            }

            return symbol;
        }

        XamlExpressionSyntaxKind InferNextSymbolKind()
        {
            var initialIndex = index;
            var mode = ParserCharacterMode.Leading;
            var kind = XamlExpressionSyntaxKind.Error;
            try
            {
                // Read until the next ',' or '}'
                for (; CanAdvance(); Advance())
                {
                    var character = CurrentCharacter();

                    if (character == ',')
                    {
                        return kind;
                    }

                    switch (mode)
                    {
                        case ParserCharacterMode.Leading:
                            if (char.IsWhiteSpace(character))
                            {
                                continue;
                            }
                            else
                            {
                                mode = ParserCharacterMode.Element;
                                goto case ParserCharacterMode.Element;
                            }
                        case ParserCharacterMode.Element:
                            {
                                if (character == ',')
                                {
                                    return kind;
                                }
                                else if (character == ':')
                                {
                                    return XamlExpressionSyntaxKind.Symbol;
                                }
                                else if (character == '.'
                                         || character == '='
                                         || character == '{'
                                         || character == '}')
                                {
                                    if (kind != XamlExpressionSyntaxKind.Error)
                                    {
                                        return kind;
                                    }

                                    return XamlExpressionSyntaxKind.Error;
                                }
                                else if (char.IsWhiteSpace(character))
                                {
                                    mode = ParserCharacterMode.Trailing;
                                }
                                else
                                {
                                    kind = XamlExpressionSyntaxKind.TypeName;
                                }
                            }
                            break;
                        case ParserCharacterMode.Trailing:
                            {
                                if (character == '.'
                                    || character == ','
                                         || character == '='
                                         || character == '{'
                                         || character == '}')
                                {
                                    return kind;
                                }
                                else if (character == ':')
                                {
                                    return XamlExpressionSyntaxKind.Symbol;
                                }
                                else if (char.IsWhiteSpace(character))
                                {
                                    continue;
                                }
                                else
                                {
                                    return kind;
                                }
                            }
                    }

                    if (PeekNextCharacter() == '}' || PeekNextCharacter() == null)
                    {
                        break;
                    }
                }
            }
            finally
            {
                index = initialIndex;
            }

            return kind;
        }

        XamlExpressionSyntaxKind InferNextChildSyntaxKind()
        {
            var initialIndex = index;
            var mode = ParserCharacterMode.Leading;

            var content = string.Empty;

            var kind = XamlExpressionSyntaxKind.Error;
            try
            {
                // Read until the next ',' or '}'
                for (; CanAdvance(); Advance())
                {
                    var character = CurrentCharacter();

                    if (character == ',')
                    {
                        return kind;
                    }

                    switch (mode)
                    {
                        case ParserCharacterMode.Leading:
                            if (char.IsWhiteSpace(character))
                            {
                                continue;
                            }
                            else
                            {
                                mode = ParserCharacterMode.Element;
                                goto case ParserCharacterMode.Element;
                            }
                        case ParserCharacterMode.Element:
                            {
                                if (character == ',')
                                {
                                    return kind;
                                }
                                else if (character == ':')
                                {
                                    kind = XamlExpressionSyntaxKind.Symbol;
                                    // Likely a symbol, but keep reading to discover if it's a member access expression.
                                }
                                else if (character == '.')
                                {
                                    if (string.IsNullOrEmpty(content)) // Consider bindings that use a single '.'.
                                    {
                                        return XamlExpressionSyntaxKind.Content;
                                    }

                                    return XamlExpressionSyntaxKind.MemberAccessExpression;
                                }
                                else if (character == '=')
                                {
                                    return XamlExpressionSyntaxKind.Assignment;
                                }
                                else if (char.IsWhiteSpace(character))
                                {
                                    if (!string.IsNullOrEmpty(content))
                                    {
                                        return kind;
                                    }
                                }
                                else if (character == '{')
                                {
                                    return XamlExpressionSyntaxKind.Expression;
                                }
                                else if (character == '}' && string.IsNullOrEmpty(content))
                                {
                                    return XamlExpressionSyntaxKind.Error;
                                }
                                else
                                {
                                    if (kind == XamlExpressionSyntaxKind.Error)
                                    {
                                        kind = XamlExpressionSyntaxKind.Content;
                                    }
                                }

                                content += character;
                            }
                            break;
                    }

                    if (PeekNextCharacter() == '}' || PeekNextCharacter() == null)
                    {
                        break;
                    }
                }
            }
            finally
            {
                index = initialIndex;
            }

            return kind;
        }

        internal void ProcessExpressionChildElements(ExpressionParserState expressionState)
        {
            _ = Position;
            for (; CanAdvance(); Advance())
            {
                var nextKind = InferNextChildSyntaxKind();

                ExpressionParserState state = null;

                switch (nextKind)
                {
                    case XamlExpressionSyntaxKind.Assignment:
                        state = ProcessAssignmentSyntax(expressionState);
                        break;
                    case XamlExpressionSyntaxKind.Symbol:
                    case XamlExpressionSyntaxKind.TypeName:
                    case XamlExpressionSyntaxKind.MemberName:
                    case XamlExpressionSyntaxKind.MemberAccessExpression:
                        state = ProcessMemberAccessExpression(expressionState);
                        break;
                    case XamlExpressionSyntaxKind.Content:
                        state = ProcessContentSyntax(expressionState);
                        break;
                    default:
                        state = ProcessErrorSyntax(expressionState);
                        break;
                }

                if (PeekNextCharacter() == '}' || PeekNextCharacter() == null)
                {
                    break;
                }

                if (PeekNextCharacter() == ',')
                {
                    Advance();
                    state.Trailing += CurrentCharacter();
                    state.SetFullSpanEnd(Position);
                }
            }

            expressionState.Span = TextSpan.FromBounds(expressionState.Span.Start, Position);
        }

        ExpressionParserState ProcessErrorSyntax(ExpressionParserState expressionState)
        {
            var error = expressionState.CreateChild(Position, XamlExpressionSyntaxKind.Error);
            for (; CanAdvance(); Advance())
            {
                if (CurrentCharacter() == '}')
                {
                    break;
                }

                var character = CurrentCharacter();

                if (character == ',')
                {
                    error.SetSpanEnd(NextPosition);
                    error.SetFullSpanEnd(NextPosition);
                    error.Trailing = character.ToString();
                    return error;
                }
                else
                {
                    error.Content += character;
                }
            }

            return error;
        }

        ExpressionParserState ProcessContentSyntax(ExpressionParserState expressionState)
        {
            var content = expressionState.CreateChild(Position, XamlExpressionSyntaxKind.Content);

            var mode = ParserCharacterMode.Leading;

            for (; CanAdvance(); Advance())
            {
                if (CurrentCharacter() == '}')
                {
                    break;
                }

                var character = CurrentCharacter();
                var next = PeekNextCharacter();

                switch (mode)
                {
                    case ParserCharacterMode.Leading:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                content.Leading += character;
                            }
                            else
                            {
                                content.Content += character;
                                mode = ParserCharacterMode.Element;
                            }
                        }
                        break;
                    case ParserCharacterMode.Element:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                content.SetSpanEnd(NextPosition);
                                mode = ParserCharacterMode.Trailing;
                                goto case ParserCharacterMode.Trailing;
                            }
                            else if (character == ',')
                            {
                                content.Trailing = character.ToString();
                                content.SetSpanEnd(NextPosition);
                                content.SetFullSpanEnd(NextPosition);
                                return content;
                            }
                            else
                            {
                                content.Content += character;
                            }
                        }
                        break;
                    case ParserCharacterMode.Trailing:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                content.Trailing += character;
                            }
                            else if (character == ',')
                            {
                                content.Trailing += character;
                                content.SetFullSpanEnd(NextPosition);
                                return content;
                            }

                            if (next != null)
                            {
                                if (!char.IsWhiteSpace(next.Value) || next != ',')
                                {
                                    content.SetFullSpanEnd(NextPosition);
                                    return content;
                                }
                            }
                        }
                        break;
                }

                if (next == '}' || next == null)
                {
                    content.SetSpanEnd(NextPosition);
                    content.SetFullSpanEnd(NextPosition);
                    return content;
                }
            }

            return content;
        }

        ExpressionParserState ProcessMemberAccessExpression(ExpressionParserState parent)
        {
            var symbolKind = InferNextSymbolKind();

            var memberAccess = parent.CreateChild(Position, XamlExpressionSyntaxKind.MemberAccessExpression);

            ExpressionParserState symbol;
            switch (symbolKind)
            {
                case XamlExpressionSyntaxKind.Symbol:
                    symbol = ProcessSymbolSyntax(memberAccess);
                    break;
                case XamlExpressionSyntaxKind.TypeName:
                    symbol = ProcessType(memberAccess);
                    break;
                default:
                    symbol = ProcessErrorSyntax(memberAccess);
                    break;
            }

            if (!CanAdvance())
            {
                memberAccess.SetSpanEnd(NextPosition);
                memberAccess.SetFullSpanEnd(NextPosition);
                return memberAccess;
            }

            Advance();

            while (CanAdvance() && (CurrentCharacter() == '.' || char.IsWhiteSpace(CurrentCharacter())))
            {
                symbol.Trailing += CurrentCharacter();
                symbol.SetFullSpanEnd(NextPosition);
                Advance();
            }

            while (CanAdvance())
            {
                var member = ProcessMemberNameExpression(memberAccess);

                if (member == null || !member.Trailing.Contains("."))
                {
                    break;
                }

                Advance();
            }

            memberAccess.SetSpanEnd(NextPosition);
            memberAccess.SetFullSpanEnd(NextPosition);

            FinaliseStateTrailingCharacters(memberAccess);

            return memberAccess;
        }

        ExpressionParserState ProcessMemberNameExpression(ExpressionParserState parent)
        {
            var member = parent.CreateChild(Position, XamlExpressionSyntaxKind.MemberName);

            var mode = ParserCharacterMode.Leading;

            for (; CanAdvance(); Advance())
            {
                if (CurrentCharacter() == '}')
                {
                    break;
                }

                var character = CurrentCharacter();

                switch (mode)
                {
                    case ParserCharacterMode.Leading:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                member.Leading += character;
                            }
                            else if (character == ',' || character == '.')
                            {
                                member.Trailing = character.ToString();
                                member.SetSpanEnd(NextPosition);
                                member.SetFullSpanEnd(NextPosition);
                                return member;
                            }
                            else
                            {
                                member.SetSpanStart(Position);
                                mode = ParserCharacterMode.Element;
                                goto case ParserCharacterMode.Element;
                            }
                        }
                        break;
                    case ParserCharacterMode.Element:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                member.Trailing += character;
                                member.SetSpanEnd(NextPosition);
                                mode = ParserCharacterMode.Trailing;
                            }
                            else if (character == ',' || character == '.')
                            {
                                member.Trailing = character.ToString();
                                member.SetSpanEnd(NextPosition);
                                member.SetFullSpanEnd(NextPosition);
                                return member;
                            }
                            else
                            {
                                member.Content += character;
                            }
                        }
                        break;
                    case ParserCharacterMode.Trailing:
                        {
                            member.Trailing += character;
                            if (char.IsWhiteSpace(character))
                            {
                                mode = ParserCharacterMode.Trailing;
                            }
                            else if (character == ',' || character == '.')
                            {
                                member.SetFullSpanEnd(NextPosition);
                                return member;
                            }
                        }
                        break;
                }

                var next = PeekNextCharacter();
                if (next == '}' || next == null)
                {
                    member.SetSpanEnd(NextPosition);
                    member.SetFullSpanEnd(NextPosition);
                    return member;
                }

                if (next == ',' || next == '.')
                {
                    mode = ParserCharacterMode.Trailing;
                }
            }

            return member;
        }

        ExpressionParserState ProcessAssignmentSyntax(ExpressionParserState parent)
        {
            var assigment = parent.CreateChild(Position, XamlExpressionSyntaxKind.Assignment);

            ProcessAssignmentLeftHandExpression(assigment);

            Advance();

            ProcessAssignmentRightHandExpression(assigment);

            assigment.SetSpanEnd(NextPosition);

            FinaliseStateTrailingCharacters(assigment);

            return assigment;
        }

        ExpressionParserState ProcessAssignmentLeftHandExpression(ExpressionParserState assigment)
        {
            var property = assigment.CreateChild(Position, XamlExpressionSyntaxKind.Property);

            var mode = ParserCharacterMode.Leading;

            for (; CanAdvance(); Advance())
            {
                var character = CurrentCharacter();

                switch (mode)
                {
                    case ParserCharacterMode.Leading:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                property.Leading += character;
                            }
                            else if (character == '=')
                            {
                                property.Trailing = character.ToString();
                                property.SetSpanEnd(NextPosition);
                                property.SetFullSpanEnd(NextPosition);
                                return property;
                            }
                            else
                            {
                                property.SetSpanStart(Position);
                                mode = ParserCharacterMode.Element;
                                goto case ParserCharacterMode.Element;
                            }
                        }
                        break;
                    case ParserCharacterMode.Element:
                        {
                            if (char.IsWhiteSpace(character))
                            {
                                property.SetSpanEnd(NextPosition);
                                mode = ParserCharacterMode.Trailing;
                                goto case ParserCharacterMode.Trailing;
                            }
                            else if (character == ',' || character == '=')
                            {
                                property.Trailing = character.ToString();
                                property.SetSpanEnd(NextPosition);
                                property.SetFullSpanEnd(NextPosition);
                                return property;
                            }
                            else
                            {
                                property.Content += character;
                            }
                        }
                        break;
                    case ParserCharacterMode.Trailing:
                        {
                            property.Trailing += character;

                            if (character == ',' || character == '=')
                            {
                                property.SetFullSpanEnd(NextPosition);
                                return property;
                            }
                        }
                        break;
                }

                var next = PeekNextCharacter();
                if (next == '}' || next == null)
                {
                    property.SetSpanEnd(NextPosition);
                    property.SetFullSpanEnd(NextPosition);
                    return property;
                }

                if (next == '=')
                {
                    mode = ParserCharacterMode.Trailing;
                    property.SetSpanEnd(NextPosition);
                }
            }

            return property;
        }

        internal void FinaliseStateTrailingCharacters(ExpressionParserState state)
        {
            var next = PeekNextCharacter();
            if (!CanAdvance()
                || next == '}'
                || next == null)
            {
                state.Trailing = string.Empty;
                state.SetFullSpanEnd(NextPosition);
                return;
            }

            var trailing = string.Empty;

            while ((char.IsWhiteSpace(CurrentCharacter()) || CurrentCharacter() == ',')
                   && CanAdvance() && IsNextCharacterValidTrailingElement())
            {
                trailing += CurrentCharacter();

                next = PeekNextCharacter();

                if (IsNextCharacterValidTrailingElement())
                {
                    Advance();
                }
            }

            state.Trailing = trailing;
            state.SetFullSpanEnd(NextPosition);
        }

        bool IsNextCharacterValidTrailingElement()
        {
            var next = PeekNextCharacter();


            if (next != null && (char.IsWhiteSpace(next.Value) || next == ','))
            {
                return true;
            }

            return false;
        }

        internal void FinaliseExpressionTrailingCharacters(ExpressionParserState expression)
        {
            if (!CanAdvance())
            {
                expression.Trailing = string.Empty;
                expression.SetSpanEnd(NextPosition);
                expression.SetFullSpanEnd(NextPosition);
                return;
            }

            var trailing = string.Empty;
            var next = PeekNextCharacter();
            if (next == '}')
            {
                Advance();
                trailing += CurrentCharacter();
            }
            else if (CurrentCharacter() == '}')
            {
                trailing += CurrentCharacter();
            }

            expression.Trailing = trailing;
            expression.SetSpanEnd(Position);
            expression.SetFullSpanEnd(NextPosition);
        }
    }
}
