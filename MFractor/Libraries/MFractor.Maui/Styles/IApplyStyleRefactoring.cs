using System;
using System.Collections.Generic;
using MFractor.Maui.XamlPlatforms;
using MFractor.Work;
using MFractor.Xml;

namespace MFractor.Maui.Styles
{
    /// <summary>
    /// Applies a given style to XAML element.
    /// </summary>
    public interface IApplyStyleRefactoring
    {
        /// <summary>
        /// Apply the given <paramref name="style"/> onto the <paramref name="syntax"/> in the given <paramref name="filePath"/>.
        /// </summary>
        IReadOnlyList<IWorkUnit> ApplyStyle(IXamlPlatform platform, XmlNode syntax, IStyle style, string filePath);

        /// <summary>
        /// Can the given <paramref name="style"/> be applied onto the <paramref name="syntax"/>.
        /// </summary>
        bool CanApplyStyle(XmlNode syntax, IStyle style);
    }
}
