using System;
using Microsoft.CodeAnalysis.Text;

namespace MFractor.Text
{
    public interface ILine
    {
        /// <summary>
        /// The text content in this line.
        /// </summary>
        string Content { get; }

        /// <summary>
        /// The length of the <see cref="Content"/> in this line.
        /// </summary>
        int Length { get; }

        /// <summary>
        /// The index/position of this <see cref="ILine"/> in the <see cref="ILineCollection"/>.
        /// </summary>
        int Index { get; }

        /// <summary>
        /// The span (offset and length) of the <see cref="Content"/> in the <see cref="ILineCollection"/>.
        /// </summary>
        TextSpan Span { get; }
    }
}