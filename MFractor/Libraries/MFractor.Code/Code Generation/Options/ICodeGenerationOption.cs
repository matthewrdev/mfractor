namespace MFractor.Code.CodeGeneration.Options
{
    public interface ICodeGenerationOption
    {
        string Label { get; }

        string Tooltip { get; }

        string Identifier { get; }

        object Value { get; set; }

        ICodeGenerationOptionValidator Validator { get; }
    }
}