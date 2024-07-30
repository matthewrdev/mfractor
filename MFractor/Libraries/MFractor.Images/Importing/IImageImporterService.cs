using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MFractor.Images.Models;
using MFractor.Progress;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Importing
{
    /// <summary>
    /// Defines an interface for the Image Importer features.
    /// </summary>
    public interface IImageImporterService
    {
        Task<bool> Import(ImportImageOperation operation, IProgressMonitor progressMonitor);

        Task<bool> Import(IEnumerable<ImportImageOperation> operations, IProgressMonitor progressMonitor);
    }
}
