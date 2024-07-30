using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MFractor.Logging;
using MFractor.Progress;
using MFractor.Workspace.WorkUnits;
using Microsoft.CodeAnalysis;

namespace MFractor.Images.Importing
{
    /// <summary>
    /// Shared Importing Utility methods.
    /// </summary>
    static class ImportUtils
    {
        public static Task<bool> CreateImageFiles(Project project, IReadOnlyList<CreateProjectFileWorkUnit> workUnits, IProgressMonitor progressMonitor, ILogger log = null)
        {
            try
            {
                progressMonitor.SetProgress("Importing images into " + project.Name, 0.0);

                for (var i = 0; i < workUnits.Count; ++i)
                {
                    var workUnit = workUnits[i];

                    progressMonitor.SetProgress("Adding " + workUnit.FilePath, i, workUnits.Count);
                    var filePath = workUnit.FilePath;

                    var file = new System.IO.FileInfo(filePath);
                    file.Directory.Create(); // If the directory already exists, this method does nothing.

                    if (workUnit.IsBinary)
                    {
                        if (workUnit.WriteContentAction != null)
                        {
                            using (var result = File.OpenWrite(filePath))
                            {
                                workUnit.WriteContentAction(result);
                            }
                        }
                    }
                    else
                    {
                        var fileContent = workUnit.FileContent;

                        if (workUnit.WriteContentAction != null)
                        {
                            using (var stream = new MemoryStream())
                            {
                                workUnit.WriteContentAction(stream);
                                stream.Seek(0, SeekOrigin.Begin);
                                using (var result = new StreamReader(stream))
                                {
                                    fileContent = result.ReadToEnd();
                                }
                            }
                        }

                        File.WriteAllText(filePath, fileContent);
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                return Task.FromResult(false);
            }

            return Task.FromResult(true);
        }
    }
}
