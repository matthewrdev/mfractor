﻿// Copyright (c) Microsoft.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Language.Xml
{
    public abstract partial class SyntaxNode
    {
        private IEnumerable<SyntaxNode> DescendantNodesImpl(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia, bool includeSelf)
        {
            return descendIntoTrivia
                ? DescendantNodesAndTokensImpl(span, descendIntoChildren, true, includeSelf).Where(e => e.IsNode)
                : DescendantNodesOnly(span, descendIntoChildren, includeSelf);
        }

        private IEnumerable<SyntaxNode> DescendantNodesAndTokensImpl(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool descendIntoTrivia, bool includeSelf)
        {
            return descendIntoTrivia
                ? DescendantNodesAndTokensIntoTrivia(span, descendIntoChildren, includeSelf)
                : DescendantNodesAndTokensOnly(span, descendIntoChildren, includeSelf);
        }

        private IEnumerable<SyntaxTrivia> DescendantTriviaImpl(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren = null, bool descendIntoTrivia = false)
        {
            return descendIntoTrivia
                ? DescendantTriviaIntoTrivia(span, descendIntoChildren)
                : DescendantTriviaOnly(span, descendIntoChildren);
        }

        private static bool IsInSpan(TextSpan span, TextSpan childSpan)
        {
            return span.OverlapsWith(childSpan)
                // special case for zero-width tokens (OverlapsWith never returns true for these)
                || (childSpan.Length == 0 && span.IntersectsWith(childSpan));
        }

        private struct ChildSyntaxListEnumeratorStack : IDisposable
        {
            private static readonly ObjectPool<ChildSyntaxList.Enumerator[]> s_stackPool = new ObjectPool<ChildSyntaxList.Enumerator[]>(() => new ChildSyntaxList.Enumerator[16]);

            private ChildSyntaxList.Enumerator[] _stack;
            private int _stackPtr;

            public ChildSyntaxListEnumeratorStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
            {
                if (descendIntoChildren == null || descendIntoChildren(startingNode))
                {
                    _stack = s_stackPool.Allocate();
                    _stackPtr = 0;
                    _stack[0].InitializeFrom(startingNode);
                }
                else
                {
                    _stack = null;
                    _stackPtr = -1;
                }
            }

            public bool IsNotEmpty { get { return _stackPtr >= 0; } }

            public bool TryGetNextInSpan(TextSpan span, out SyntaxNode value)
            {
                while (_stack[_stackPtr].TryMoveNextAndGetCurrent(out value))
                {
                    if (IsInSpan(span, value.FullSpan))
                    {
                        return true;
                    }
                }

                _stackPtr--;
                return false;
            }

            public SyntaxNode TryGetNextAsNodeInSpan(TextSpan span)
            {
                SyntaxNode nodeValue;
                while ((nodeValue = _stack[_stackPtr].TryMoveNextAndGetCurrentAsNode()) != null)
                {
                    if (IsInSpan(span, nodeValue.FullSpan))
                    {
                        return nodeValue;
                    }
                }

                _stackPtr--;
                return null;
            }

            public void PushChildren(SyntaxNode node)
            {
                if (++_stackPtr >= _stack.Length)
                {
                    // Geometric growth
                    Array.Resize(ref _stack, checked(_stackPtr * 2));
                }

                _stack[_stackPtr].InitializeFrom(node);
            }

            public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
            {
                if (descendIntoChildren == null || descendIntoChildren(node))
                {
                    PushChildren(node);
                }
            }

            public void Dispose()
            {
                // Return only reasonably-sized stacks to the pool.
                if (_stack?.Length < 256)
                {
                    Array.Clear(_stack, 0, _stack.Length);
                    s_stackPool.Free(_stack);
                }
            }
        }

        private struct TriviaListEnumeratorStack : IDisposable
        {
            private static readonly ObjectPool<SyntaxTriviaList.Enumerator[]> s_stackPool = new ObjectPool<SyntaxTriviaList.Enumerator[]>(() => new SyntaxTriviaList.Enumerator[16]);

            private SyntaxTriviaList.Enumerator[] _stack;
            private int _stackPtr;

            public bool TryGetNext(out SyntaxTrivia value)
            {
                if (_stack[_stackPtr].TryMoveNextAndGetCurrent(out value))
                {
                    return true;
                }

                _stackPtr--;
                return false;
            }

            public void PushLeadingTrivia(SyntaxToken token)
            {
                Grow();
                _stack[_stackPtr].InitializeFromLeadingTrivia(token);
            }

            public void PushTrailingTrivia(SyntaxToken token)
            {
                Grow();
                _stack[_stackPtr].InitializeFromTrailingTrivia(token);
            }

            private void Grow()
            {
                if (_stack == null)
                {
                    _stack = s_stackPool.Allocate();
                    _stackPtr = -1;
                }

                if (++_stackPtr >= _stack.Length)
                {
                    // Geometric growth
                    Array.Resize(ref _stack, checked(_stackPtr * 2));
                }
            }

            public void Dispose()
            {
                // Return only reasonably-sized stacks to the pool.
                if (_stack?.Length < 256)
                {
                    Array.Clear(_stack, 0, _stack.Length);
                    s_stackPool.Free(_stack);
                }
            }
        }

        private struct TwoEnumeratorListStack : IDisposable
        {
            public enum Which : byte
            {
                Node,
                Trivia
            }

            private ChildSyntaxListEnumeratorStack _nodeStack;
            private TriviaListEnumeratorStack _triviaStack;
            private readonly ArrayBuilder<Which> _discriminatorStack;

            public TwoEnumeratorListStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
            {
                _nodeStack = new ChildSyntaxListEnumeratorStack(startingNode, descendIntoChildren);
                _triviaStack = new TriviaListEnumeratorStack();
                if (_nodeStack.IsNotEmpty)
                {
                    _discriminatorStack = ArrayBuilder<Which>.GetInstance ();
                    _discriminatorStack.Add(Which.Node);
                }
                else
                {
                    _discriminatorStack = null;
                }
            }

            public bool IsNotEmpty { get { return _discriminatorStack?.Count > 0; } }

            public Which PeekNext()
            {
                return _discriminatorStack[_discriminatorStack.Count - 1];
            }

            public bool TryGetNextInSpan(TextSpan span, out SyntaxNode value)
            {
                if (_nodeStack.TryGetNextInSpan(span, out value))
                {
                    return true;
                }

                _discriminatorStack.RemoveAt(_discriminatorStack.Count - 1);
                return false;
            }

            public bool TryGetNext(out SyntaxTrivia value)
            {
                if (_triviaStack.TryGetNext(out value))
                {
                    return true;
                }

                _discriminatorStack.RemoveAt(_discriminatorStack.Count - 1);
                return false;
            }

            public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
            {
                if (descendIntoChildren == null || descendIntoChildren(node))
                {
                    _nodeStack.PushChildren(node);
                    _discriminatorStack.Add(Which.Node);
                }
            }

            public void PushLeadingTrivia(SyntaxToken token)
            {
                _triviaStack.PushLeadingTrivia(token);
                _discriminatorStack.Add(Which.Trivia);
            }

            public void PushTrailingTrivia(SyntaxToken token)
            {
                _triviaStack.PushTrailingTrivia(token);
                _discriminatorStack.Add(Which.Trivia);
            }

            public void Dispose()
            {
                _nodeStack.Dispose();
                _triviaStack.Dispose();
                _discriminatorStack?.Free ();
            }
        }

        private struct ThreeEnumeratorListStack : IDisposable
        {
            public enum Which : byte
            {
                Node,
                Trivia,
                Token
            }

            private ChildSyntaxListEnumeratorStack _nodeStack;
            private TriviaListEnumeratorStack _triviaStack;
            private readonly ArrayBuilder<SyntaxNode> _tokenStack;
            private readonly ArrayBuilder<Which> _discriminatorStack;

            public ThreeEnumeratorListStack(SyntaxNode startingNode, Func<SyntaxNode, bool> descendIntoChildren)
            {
                _nodeStack = new ChildSyntaxListEnumeratorStack(startingNode, descendIntoChildren);
                _triviaStack = new TriviaListEnumeratorStack();
                if (_nodeStack.IsNotEmpty)
                {
                    _tokenStack = ArrayBuilder<SyntaxNode>.GetInstance ();
                    _discriminatorStack = ArrayBuilder<Which>.GetInstance ();

                    _discriminatorStack.Add(Which.Node);
                }
                else
                {
                    _tokenStack = null;
                    _discriminatorStack = null;
                }
            }

            public bool IsNotEmpty { get { return _discriminatorStack?.Count > 0; } }

            public Which PeekNext()
            {
                return _discriminatorStack[_discriminatorStack.Count - 1];
            }

            public bool TryGetNextInSpan(TextSpan span, out SyntaxNode value)
            {
                if (_nodeStack.TryGetNextInSpan(span, out value))
                {
                    return true;
                }

                _discriminatorStack.RemoveAt(_discriminatorStack.Count - 1);
                return false;
            }

            public bool TryGetNext(out SyntaxTrivia value)
            {
                if (_triviaStack.TryGetNext(out value))
                {
                    return true;
                }

                _discriminatorStack.RemoveAt(_discriminatorStack.Count - 1);
                return false;
            }

            public SyntaxNode PopToken()
            {
                _discriminatorStack.RemoveAt(_discriminatorStack.Count - 1);
                var result = _tokenStack[_tokenStack.Count - 1];
                _tokenStack.RemoveAt(_tokenStack.Count - 1);
                return result;
            }

            public void PushChildren(SyntaxNode node, Func<SyntaxNode, bool> descendIntoChildren)
            {
                if (descendIntoChildren == null || descendIntoChildren(node))
                {
                    _nodeStack.PushChildren(node);
                    _discriminatorStack.Add(Which.Node);
                }
            }

            public void PushLeadingTrivia(SyntaxToken token)
            {
                _triviaStack.PushLeadingTrivia(token);
                _discriminatorStack.Add(Which.Trivia);
            }

            public void PushTrailingTrivia(SyntaxToken token)
            {
                _triviaStack.PushTrailingTrivia(token);
                _discriminatorStack.Add(Which.Trivia);
            }

            public void PushToken(SyntaxNode value)
            {
                _tokenStack.Add(value);
                _discriminatorStack.Add(Which.Token);
            }

            public void Dispose()
            {
                _nodeStack.Dispose();
                _triviaStack.Dispose();
                _tokenStack?.Free ();
                _discriminatorStack?.Free ();
            }
        }

        private IEnumerable<SyntaxNode> DescendantNodesOnly(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
        {
            if (includeSelf && IsInSpan(span, this.FullSpan))
            {
                yield return this;
            }

            using (var stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren))
            {
                while (stack.IsNotEmpty)
                {
                    SyntaxNode nodeValue = stack.TryGetNextAsNodeInSpan(span);
                    if (nodeValue != null)
                    {
                        // PERF: Push before yield return so that "nodeValue" is 'dead' after the yield
                        // and therefore doesn't need to be stored in the iterator state machine. This
                        // saves a field.
                        stack.PushChildren(nodeValue, descendIntoChildren);

                        yield return nodeValue;
                    }
                }
            }
        }

        private IEnumerable<SyntaxNode> DescendantNodesAndTokensOnly(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
        {
            if (includeSelf && IsInSpan(span, this.FullSpan))
            {
                yield return this;
            }

            using (var stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren))
            {
                while (stack.IsNotEmpty)
                {
                    SyntaxNode value;
                    if (stack.TryGetNextInSpan(span, out value))
                    {
                        // PERF: Push before yield return so that "value" is 'dead' after the yield
                        // and therefore doesn't need to be stored in the iterator state machine. This
                        // saves a field.
                        var nodeValue = value;
                        if (nodeValue != null)
                        {
                            stack.PushChildren(nodeValue, descendIntoChildren);
                        }

                        yield return value;
                    }
                }
            }
        }

        private IEnumerable<SyntaxNode> DescendantNodesAndTokensIntoTrivia(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren, bool includeSelf)
        {
            if (includeSelf && IsInSpan(span, this.FullSpan))
            {
                yield return this;
            }

            using (var stack = new ThreeEnumeratorListStack(this, descendIntoChildren))
            {
                while (stack.IsNotEmpty)
                {
                    switch (stack.PeekNext())
                    {
                        case ThreeEnumeratorListStack.Which.Node:
                            SyntaxNode value;
                            if (stack.TryGetNextInSpan(span, out value))
                            {
                                // PERF: The following code has an unusual structure (note the 'break' out of
                                // the case statement from inside an if body) in order to convince the compiler
                                // that it can save a field in the iterator machinery.
                                if (value.IsNode)
                                {
                                    // parent nodes come before children (prefix document order)
                                    stack.PushChildren(value, descendIntoChildren);
                                }

                                // PERF: Yield here (rather than inside the if bodies above) so that it's
                                // obvious to the compiler that 'value' is not used beyond this point and,
                                // therefore, doesn't need to be kept in a field.
                                yield return value;
                            }

                            break;

                        /*case ThreeEnumeratorListStack.Which.Trivia:
                            // yield structure nodes and enumerate their children
                            SyntaxTrivia trivia;
                            if (stack.TryGetNext(out trivia))
                            {
                                if (trivia.HasStructure && IsInSpan(span, trivia.FullSpan))
                                {
                                    var structureNode = trivia.GetStructure();

                                    // parent nodes come before children (prefix document order)

                                    // PERF: Push before yield return so that "structureNode" is 'dead' after the yield
                                    // and therefore doesn't need to be stored in the iterator state machine. This
                                    // saves a field.
                                    stack.PushChildren(structureNode, descendIntoChildren);

                                    yield return structureNode;
                                }
                            }
                            break;*/

                        case ThreeEnumeratorListStack.Which.Token:
                            yield return stack.PopToken();
                            break;
                    }
                }
            }
        }

        private IEnumerable<SyntaxTrivia> DescendantTriviaOnly(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren)
        {
            using (var stack = new ChildSyntaxListEnumeratorStack(this, descendIntoChildren))
            {
                while (stack.IsNotEmpty)
                {
                    SyntaxNode value;
                    if (stack.TryGetNextInSpan(span, out value))
                    {
                        if (value.IsNode)
                        {
                            var nodeValue = value;

                            stack.PushChildren(nodeValue, descendIntoChildren);
                        }
                        else if (value.IsToken)
                        {
                            var token = (SyntaxToken)value;

                            foreach (var trivia in token.GetLeadingTrivia())
                            {
                                if (IsInSpan(span, trivia.FullSpan))
                                {
                                    yield return trivia;
                                }
                            }

                            foreach (var trivia in token.GetTrailingTrivia())
                            {
                                if (IsInSpan(span, trivia.FullSpan))
                                {
                                    yield return trivia;
                                }
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<SyntaxTrivia> DescendantTriviaIntoTrivia(TextSpan span, Func<SyntaxNode, bool> descendIntoChildren)
        {
            using (var stack = new TwoEnumeratorListStack(this, descendIntoChildren))
            {
                while (stack.IsNotEmpty)
                {
                    switch (stack.PeekNext())
                    {
                        case TwoEnumeratorListStack.Which.Node:
                            SyntaxNode value;
                            if (stack.TryGetNextInSpan(span, out value))
                            {
                                if (value.IsNode)
                                {
                                    var nodeValue = value;
                                    stack.PushChildren(nodeValue, descendIntoChildren);
                                }
                                else if (value.IsToken)
                                {
                                    var token = (SyntaxToken)value;

                                    if (token.HasTrailingTrivia)
                                    {
                                        stack.PushTrailingTrivia(token);
                                    }

                                    if (token.HasLeadingTrivia)
                                    {
                                        stack.PushLeadingTrivia(token);
                                    }
                                }
                            }

                            break;

                        case TwoEnumeratorListStack.Which.Trivia:
                            // yield structure nodes and enumerate their children
                            SyntaxTrivia trivia;
                            if (stack.TryGetNext(out trivia))
                            {
                                // PERF: Push before yield return so that "trivia" is 'dead' after the yield
                                // and therefore doesn't need to be stored in the iterator state machine. This
                                // saves a field.
                                /*if (trivia.HasStructure)
                                {
                                    var structureNode = trivia.GetStructure();
                                    stack.PushChildren(structureNode, descendIntoChildren);
                                }*/

                                if (IsInSpan(span, trivia.FullSpan))
                                {
                                    yield return trivia;
                                }
                            }

                            break;
                    }
                }
            }
        }
    }
}
