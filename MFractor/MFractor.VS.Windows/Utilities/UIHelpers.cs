using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using MediaColor = System.Windows.Media.Color;
using DrawingColor = System.Drawing.Color;
using EnvDTE80;
using Microsoft.VisualStudio.Shell;
using EnvDTE;

namespace MFractor.VS.Windows.Utilities
{
    /// <summary>
    /// Provide helper and extension methods for dealing with UI.
    /// </summary>
    public static class UIHelpers
    {
        const string DTE_PROPERTIES_CATEGORY_FONTS_AND_COLORS = "FontsAndColors";
        const string DTE_PROPERTIES_PAGE_TEXT_EDITOR = "TextEditor";
        const string DTE_PROPERTY_FONT_FAMILY = "FontFamily";
        const string DTE_PROPERTY_FONT_SIZE = "FontSize";
        const string DTE_PROPERTY_FONTS_AND_COLORS_ITEMS = "FontsAndColorsItems";
        const string DISPLAY_ITEM_PLAIN_TEXT = "Plain Text";

        /// <summary>
        /// Get an instance of the DTE class.
        /// </summary>
        public static DTE2 DTE { get; } = ServiceProvider.GlobalProvider.GetService(typeof(DTE)) as DTE2;

        static Properties GetTextEditorFontProperties() => DTE.Properties[DTE_PROPERTIES_CATEGORY_FONTS_AND_COLORS, DTE_PROPERTIES_PAGE_TEXT_EDITOR];


        /// <summary>
        /// Converts this <c>System.Drawing.Color</c> instance to a <c>System.Windows.Media.Color</c> instance.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static MediaColor ToMediaColor(this DrawingColor color) => MediaColor.FromArgb(color.A, color.R, color.G, color.B);

        /// <summary>
        /// Create a <c>SolidColorBrush</c> from the current Color instace.
        /// </summary>
        /// <param name="color"></param>
        /// <returns></returns>
        public static System.Windows.Media.SolidColorBrush ToBrush(this MediaColor color) => new System.Windows.Media.SolidColorBrush(color);

        /// <summary>
        /// Gets the current Visual Studio Text Editor Font Family.
        /// </summary>
        /// <returns></returns>
        public static string GetTextEditorFontFamilyName()
        {
            var fontProperties = GetTextEditorFontProperties();
            var fontFamily = fontProperties.Item(DTE_PROPERTY_FONT_FAMILY).Value.ToString();
            return fontFamily;
        }

        /// <summary>
        /// Gets the current Visual Studio Text Editor Font size.
        /// </summary>
        /// <returns></returns>
        public static int GetTextEditorFontSize()
        {
            var fontProperties = GetTextEditorFontProperties();
            var fontSize = Convert.ToInt32(fontProperties.Item(DTE_PROPERTY_FONT_SIZE).Value);
            return fontSize;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public static DrawingColor GetTextEditorPlainTextColor()
        {
            var fontProperties = GetTextEditorFontProperties();
            var fontsAndColorsItems = fontProperties.Item(DTE_PROPERTY_FONTS_AND_COLORS_ITEMS).Object as EnvDTE.FontsAndColorsItems;

            if (fontsAndColorsItems == null)
            {
                return DrawingColor.Black;
            }

            var colorableItems = fontsAndColorsItems.Item(DISPLAY_ITEM_PLAIN_TEXT) as EnvDTE.ColorableItems;
            var oleColor = Convert.ToInt32(colorableItems.Foreground);
            var foregroundColor = System.Drawing.ColorTranslator.FromOle(oleColor);
            return foregroundColor;
        }
    }
}
