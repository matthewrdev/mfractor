using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis.Text;
using System;

namespace MFractor.Maui.Syntax
{
    public abstract class XamlExpressionSyntaxNode
    {
        public XamlExpressionSyntaxNode Parent { get; internal set; }

        public bool IsRoot => Parent == null;

        public bool IsLeaf => !Children.Any();
        
        public XamlExpressionSyntaxKind SyntaxKind { get; }

        public TextSpan FullSpan
        {
            get;
            internal set;
        }

        public TextSpan Span
        {
            get;
            internal set;
        }

        public ImmutableList<XamlExpressionSyntaxNode> Children { get; private set; } = Enumerable.Empty<XamlExpressionSyntaxNode>().ToImmutableList();

        public ImmutableList<char> LeadingCharacters { get; private set; } = Enumerable.Empty<char>().ToImmutableList();

        public string Leading => string.Join("", LeadingCharacters);

        public ImmutableList<char> TrailingCharacters { get; private set; } = Enumerable.Empty<char>().ToImmutableList();

        public string Trailing => string.Join("", TrailingCharacters);

        public XamlExpressionSyntaxNode(XamlExpressionSyntaxKind syntaxKind)
        {
            SyntaxKind = syntaxKind;
        }

        internal void AddChild(XamlExpressionSyntaxNode syntaxNode)
        {
            if (syntaxNode == null)
            {
                return;
            }

            syntaxNode.Parent = this;
            Children = Children.Add(syntaxNode);
        }

        internal void AddLeading(char character)
        {
            LeadingCharacters = LeadingCharacters.Add(character);
        }

        internal void SetLeading(string leading)
        {
            if (string.IsNullOrEmpty(leading))
            {
                LeadingCharacters = Enumerable.Empty<char>().ToImmutableList();
            }
            else
            {
                LeadingCharacters = leading.ToImmutableList();
            }
        }

        public char? GetCharacter(int offset)
        {
            if (offset < 0 || offset >= this.ToString().Length)
            {
                return null;
            }

            return this.ToString()[offset];
        }

        public char? GetPreceedingCharacter(int offset)
        {
            var adjusted = (offset - FullSpan.Start) - 1;

            return GetCharacter(adjusted);
        }

        public  XamlExpressionSyntaxNode PreceedingChild(int offset)
        {
            var preceedingOffset = offset - 1;

            return IntersectingChild(preceedingOffset);
        }

        public  XamlExpressionSyntaxNode IntersectingChild(int offset)
		{
            if (!FullSpan.Contains(offset))
            {
                return null;
            }
            
            if (IsLeaf)
            {
                return this;
            }

            var intesection = Children.Where(c => c.FullSpan.Contains(offset)).FirstOrDefault();

            if (intesection == null)
            {
                return this;
            }

            return intesection.IntersectingChild(offset);
		}

		internal void AddTrailing(char character)
        {
            TrailingCharacters = TrailingCharacters.Add(character);
        }

        internal void SetTrailing(string trailing)
        {
            if (string.IsNullOrEmpty(trailing))
            {
                TrailingCharacters = Enumerable.Empty<char>().ToImmutableList();
            }
            else
            {
                TrailingCharacters = trailing.ToImmutableList();
            }
        }

		public override string ToString()
		{
            return Leading + string.Join("", Children.Select(c => c.ToString())) + Trailing;
		}

        public TSyntax GetChild<TSyntax>() where TSyntax : XamlExpressionSyntaxNode
        {
            return Children.OfType<TSyntax>().FirstOrDefault();
        }

        public TSyntax GetChild<TSyntax>(Func<TSyntax, bool> predicate) where TSyntax : XamlExpressionSyntaxNode
        {
            return Children.OfType<TSyntax>().FirstOrDefault(predicate);
        }

        public int Index
        {
            get
            {
                if (Parent == null)
                {
                    throw new InvalidOperationException("This node does not have a parent!");
                }

                return Parent.Children.IndexOf(this);
            }
        }
	}
}
