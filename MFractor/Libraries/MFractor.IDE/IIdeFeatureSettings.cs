namespace MFractor.Ide
{
    public interface IIdeFeatureSettings
    {
        bool ShowProductTips { get; set; }

        bool AutomaticFrameworkConfigurations { get; set; }

        bool UseXAMLIntelliSense { get; set; }

        bool AllowColorAdornments { get; set; }

        bool AllowGridAdornments { get; set; }

        bool AllowThicknessAdornments { get; set; }

        bool ExperimentalFeatures { get; set; }

        bool EnableLocalisationAnalysis { get; set; }

        bool EnableExtendedLogging { get; set; }
    }
}