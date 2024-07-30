using System;

namespace MFractor.Code.CodeGeneration
{
    public interface ICodeGeneratorParameter
    {
        string Name { get; }

        string Description { get; }

        string Value { get; }

        Type ParameterType { get; }
    }
}
