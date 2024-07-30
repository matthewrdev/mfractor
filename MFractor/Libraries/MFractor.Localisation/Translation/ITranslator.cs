using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Configuration;
using MFractor.Progress;

namespace MFractor.Localisation.Translation
{
    /// <summary>
    /// Exposes a translation source into the translation service.
    /// </summary>
    [InheritedExport]
    public interface ITranslator : IConfigurable
    {
        IEnumerable<string> Translate(IEnumerable<string> values,
                                      CultureInfo source,
                                      CultureInfo target,
                                      IProgressMonitor progressMonitor);

        Task<IEnumerable<string>> TranslateAsync(IEnumerable<string> values,
                                                 CultureInfo source,
                                                 CultureInfo target,
                                                 IProgressMonitor progressMonitor);
    }
}
