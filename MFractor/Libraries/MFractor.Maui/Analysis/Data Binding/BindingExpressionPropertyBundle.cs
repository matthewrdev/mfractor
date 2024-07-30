using MFractor.Maui.Syntax.Expressions;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Analysis.DataBinding
{
    /// <summary>
    /// A bundle for transferring the result of a binding expression analysis routine.
    /// </summary>
    public class BindingExpressionPropertyBundle
    {
        /// <summary>
        /// The expression.
        /// </summary>
        public BindingExpression Expression;

        /// <summary>
        /// The property suggestion.
        /// </summary>
        public IPropertySymbol PropertySuggestion;

        public string Property { get; internal set; }
    }
}

