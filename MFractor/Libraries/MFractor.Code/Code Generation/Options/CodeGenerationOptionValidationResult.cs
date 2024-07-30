namespace MFractor.Code.CodeGeneration.Options
{
    public class CodeGenerationOptionValidationResult
    {
        public CodeGenerationOptionValidationResult(bool success, string errorMessage)
        {
            Success = success;
            ErrorMessage = errorMessage;
        }

        public bool Success { get; }

        public string ErrorMessage { get; }
    }
}
