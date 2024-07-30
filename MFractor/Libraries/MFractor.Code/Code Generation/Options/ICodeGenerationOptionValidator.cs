using System;

namespace MFractor.Code.CodeGeneration.Options
{
    public interface ICodeGenerationOptionValidator
    {
        CodeGenerationOptionValidationResult Validate(ICodeGenerationOption option);
    }
}
