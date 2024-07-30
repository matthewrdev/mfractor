using System;
using System.Collections.Generic;
using MFractor.iOS.Images;
using Microsoft.CodeAnalysis;

namespace MFractor.iOS.Services
{
    public interface IApplicationIconService
    {
        AssetCatalogue GetIconSet(string iconSetFilePath);
    }
}
