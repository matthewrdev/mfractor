using System;
using MFractor.Configuration;

namespace MFractor.Maui.Configuration
{
    public interface IBehaviorsConfiguration : IConfigurable
    {
        string BehaviorsFolder { get; set; }

        string BehaviorsNamespace { get; set; }
    }
}
