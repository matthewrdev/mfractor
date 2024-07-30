using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using MFractor.Configuration;
using MFractor.Configuration.Attributes;
using MFractor.Progress;

namespace MFractor.Localisation.Translation
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    class GoogleTranslator : Configurable, ITranslator
    {
        public override string Identifier => "com.mfractor.translation.google_cloud_translation";

        public override string Name => "Google Cloud Translation";

        public override string Documentation => "Provides translation capabilities to MFractor through the Google Cloud Translation API.";

        [ExportProperty ("What is the API key for the Google Cloud Translation API?")]
        public string ApiKey { get; set; }

        public IEnumerable<string> Translate (IEnumerable<string> values, 
                                              CultureInfo source, 
                                              CultureInfo target,
                                              IProgressMonitor progressMonitor)
        {
            throw new NotImplementedException ();
        }

        public Task<IEnumerable<string>> TranslateAsync (IEnumerable<string> values, 
                                                         CultureInfo source, 
                                                         CultureInfo target,
                                                         IProgressMonitor progressMonitor)
        {
            throw new NotImplementedException ();
        }
    }
}
