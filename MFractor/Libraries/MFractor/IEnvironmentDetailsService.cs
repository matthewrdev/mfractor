using System;
using System.Collections.Generic;

namespace MFractor
{
    public interface IEnvironmentDetailsService
    {
        IReadOnlyList<string> Details { get; }
    }
}
