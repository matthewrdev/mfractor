using System.Collections.Generic;
using MFractor.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Fonts
{
    /// <summary>
    /// An <see cref="IFont"/> that is within a <see cref="Project"/>.
    /// </summary>
    public interface IFontAsset : IFont
    {
        Project Project { get; }
    }
}