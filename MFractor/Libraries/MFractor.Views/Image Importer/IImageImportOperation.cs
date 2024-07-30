using System;
using Microsoft.CodeAnalysis;
using MFractor.Images;
using System.Collections.Generic;

namespace MFractor.Views.ImageImporter
{
    public interface IImageImportOperationProjectState
    {
        bool IsSelected { get; }

        Project Project { get; }

        UnifiedImageDensityKind Density { get; }

        ImageResourceType ImageResourceType { get; }
    }

    public interface IImageImportOperation
    {
        string SourceFile { get; }

        string AssetName { get; }

        bool ResizeImage { get; }

        ImageSize NewSize { get; }

        List<IImageImportOperationProjectState> ProjectStates { get; }
    }
}
