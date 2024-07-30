using System;
using System.Collections.Generic;
using MFractor.Maui.Utilities;
using MFractor.Workspace.Data.Models;
using Microsoft.CodeAnalysis.Text;


namespace MFractor.Maui.Data.Models
{
    public class ThicknessUsage : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The formatted version of the thickness usage in the format of 'Left,Top,Right,Bottom'
        /// <para/>
        /// Use <see cref="FormattedValue"/> when attempting to match dupicate thickness values.
        /// </summary>
        public string FormattedValue => ThicknessHelper.ToFormattedValueString(Left, Right, Top, Bottom);

        /// <summary>
        /// The literal value of this thickness usages.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The file offset for the start of the value section of the thickness usage.
        /// </summary>
        /// <value>The name start.</value>
        public int ValueStart { get; set; }

        /// <summary>
        /// The file offset for the end of the name section of the thickness usage.
        /// </summary>
        /// <value>The name end.</value>
        public int ValueEnd { get; set; }

        /// <summary>
        /// The span of the thicknesses value area.
        /// </summary>
        /// <value>The span.</value>
        public TextSpan ValueSpan
        {
            get => TextSpan.FromBounds(ValueStart, ValueEnd);
            set
            {
                ValueStart = value.Start;
                ValueEnd = value.End;
            }
        }
        /// <summary>
        /// The left value of this thickness.
        /// </summary>
        public double Left { get; set; }

        /// <summary>
        /// The right value of this thickness.
        /// </summary>
        public double Right { get; set; }

        /// <summary>
        /// The top value of this thickness.
        /// </summary>
        public double Top { get; set; }

        /// <summary>
        /// The bottom value of this thickness.
        /// </summary>
        public double Bottom { get; set; }

        /// <summary>
        /// An alias for <see cref="Left"/>, <see cref="Right"/>, <see cref="Top"/> and <see cref="Bottom"/> using the <see cref="ThicknessHelper"/>.
        /// <para/>
        /// If the provided value is valid (non null and has 1,2or 4 values), the <see cref="FormattedValue"/> will also be initialised.
        /// </summary>
        
        public IReadOnlyList<double> Thickness
        {
            get => new List<double>() { Left, Right, Top, Bottom };
            set
            {
                if (ThicknessHelper.ProcessThickness(value, out var left, out var right, out var top, out var bottom))
                {
                    Left = left;
                    Right = right;
                    Top = top;
                    Bottom = bottom;
                }
            }
        }

        /// <summary>
        /// Compares this thickness value against the <paramref name="left"/>, <paramref name="right"/>, <paramref name="top"/> and <paramref name="bottom"/>.
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="top"></param>
        /// <param name="bottom"></param>
        /// <returns></returns>
        public bool IsMatch(double left, double right, double top, double bottom)
        {
            return Math.Abs(Left - left) < Double.Epsilon
                  && Math.Abs(Right - right) < Double.Epsilon
                  && Math.Abs(Top - top) < Double.Epsilon
                  && Math.Abs(Bottom - bottom) < Double.Epsilon;
        }

        public override string ToString()
        {
            return ThicknessHelper.ToFormattedValueString(Left, Right, Top, Bottom);
        }
    }
}