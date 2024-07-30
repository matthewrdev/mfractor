namespace MFractor.Maui
{
    /// <summary>
    /// The result of a type inference operation.
    /// </summary>
    public class ValueConverterTypeInferance
    {
        public ValueConverterTypeInferance(bool inferenceSuccess,
                                           string inputType,
                                           string outputType,
                                           string parameterType = null)
        {
            if (string.IsNullOrEmpty(inputType))
            {
                inputType = "object";
            }

            if (string.IsNullOrEmpty(outputType))
            {
                outputType = "object";
            }

            InferenceSuccess = inferenceSuccess;

            InputType = inputType;
            OutputType = outputType;
            ParameterType = parameterType;
        }

        /// <summary>
        /// Did the value converter type inference succeed?
        /// </summary>
        public bool InferenceSuccess { get; } = false;

        /// <summary>
        /// The fully qualified type for the value converter input.
        /// </summary>
        public string InputType { get; }

        /// <summary>
        /// The fully qualified type for the value converter output.
        /// </summary>
        public string OutputType { get; }

        /// <summary>
        /// The fully qualified type for the value converter parameter.
        /// </summary>
        public string ParameterType { get; }

        /// <summary>
        /// The default result of a value converter inference.
        /// </summary>
        public static readonly ValueConverterTypeInferance Default = new ValueConverterTypeInferance(false, "object", "object");
    }
}
