using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Data.Models;
using MFractor.Maui.Utilities;
using MFractor.Workspace.Data.Models;

namespace MFractor.Maui.Data.Models
{
    /// <summary>
    /// A Thickness static resource declaration.
    /// </summary>
    public class ThicknessDefinition : ProjectFileOwnedEntity
    {
        /// <summary>
        /// The primary key of the <see cref="StaticResourceDefinition"/> that declares this this thicknesses.
        /// </summary>
        /// <value>The static resource identifier.</value>
        public int StaticResourceKey { get; set; }

        /// <summary>
        /// The formatted version of the thickness usage in the format of 'Left,Top,Right,Bottom'
        /// <para/>
        /// Use <see cref="FormattedValue"/> when attempting to match dupicate thickness values.
        /// </summary>
        public string FormattedValue => ThicknessHelper.ToFormattedValueString(Left, Right, Top, Bottom);

        /// <summary>
        /// The name of this thickness resource.
        /// </summary>
        public string Name { get; set; }

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
    }
}
