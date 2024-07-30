using System;
using MFractor.Configuration;

namespace MFractor.Maui.Configuration
{
    public interface IValueConverterTypeInfermentConfiguration : IConfigurable
    {
        string DefaultType { get; set; }

        bool TryInferUnknownTypes { get; set; }
    }
}
