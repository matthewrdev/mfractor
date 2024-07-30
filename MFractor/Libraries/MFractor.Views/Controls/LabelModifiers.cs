using System;
using Xwt;
using Xwt.Drawing;

namespace MFractor.Views.Controls
{
    /// <summary>
    /// Borrows the Modifier concept from SwiftUI, to allow declaratively setting
    /// control properties.
    /// </summary>
    public static class LabelModifiers
    {
        /// <summary>
        /// Sets the Label style as a font style.
        /// </summary>
        /// <param name="label"></param>
        /// <returns>The object itself to allow for fluent chaining.</returns>
        public static Label SetTitleFont(this Label label)
        {
            label.HeightRequest = 30;
            label.Font = Font.SystemFont
                .WithSize(20)
                .WithWeight(FontWeight.Bold);

            return label;
        }

        /// <summary>
        /// Sets the text of the label.
        /// </summary>
        /// <param name="label"></param>
        /// <param name="text">The text to set.</param>
        /// <returns>The object itself to allow for fluent chaining.</returns>
        public static Label WithText(this Label label, string text)
        {
            label.Text = text;
            return label;
        }
    }
}
