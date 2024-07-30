using System;
using MFractor.Maui.CodeGeneration.ValueConversion;

namespace MFractor.Views.ValueConverterWizard
{
    public class ValueConverterGenerationEventArgs : EventArgs
    {
        public ValueConverterGenerationEventArgs(ValueConverterGenerationOptions options)
        {
            Options = options ?? throw new ArgumentNullException(nameof(options));
        }

        public ValueConverterGenerationOptions Options { get; }
    }
}
