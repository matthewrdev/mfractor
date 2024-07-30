using System;
using System.ComponentModel.Composition;
using MFractor.Configuration;

namespace MFractor.Ide
{
    /// <summary>
    /// The global settings for use throughout MFractor.
    /// </summary>
    [Export(typeof(IIdeFeatureSettings))]
    [PartCreationPolicy(CreationPolicy.Shared)]
    class IdeFeatureSettings : IIdeFeatureSettings
    {
        readonly Lazy<IUserOptions> userOptions;
        IUserOptions UserOptions => userOptions.Value;

        [ImportingConstructor]
        public IdeFeatureSettings(Lazy<IUserOptions> userOptions)
        {
            this.userOptions = userOptions;
        }

        /// <summary>
        /// The key for <see cref="UseXAMLIntelliSense"/>.
        /// </summary>
        public const string UseXAMLIntelliSenseKey = "com.mfractor.settings.csharp.use_xaml_intellisense";

        /// <summary>
        /// Should the IDE use MFractor's XAML IntelliSense?
        /// </summary>
        /// <value><c>true</c> if use XAMLI ntelli sense; otherwise, <c>false</c>.</value>
        public bool UseXAMLIntelliSense
        {
            get => UserOptions.Get(UseXAMLIntelliSenseKey, true);
            set => UserOptions.Set(UseXAMLIntelliSenseKey, value);
        }

        /// <summary>
        /// The key for <see cref="ShowProductTips"/>.
        /// </summary>
        public const string ShowProductTipsKey = "com.mfractor.settings.show_product_tips";

        /// <summary>
        /// Should MFractor show product tips once per week?
        /// </summary>
        /// <value><c>true</c> if show product tips; otherwise, <c>false</c>.</value>
        public bool ShowProductTips
        {
            get => UserOptions.Get(ShowProductTipsKey, true);
            set => UserOptions.Set(ShowProductTipsKey, value);
        }

        public const string AutomaticFrameworkConfigurationsKey = "com.mfractor.settings.csharp.automatic_framework_configurations";
        public bool AutomaticFrameworkConfigurations
        {
            get => UserOptions.Get(AutomaticFrameworkConfigurationsKey, true);
            set => UserOptions.Set(AutomaticFrameworkConfigurationsKey, value);
        }

        public const string ExperimentalFeaturesKey = "com.mfractor.settings.experimental_features";
        public bool ExperimentalFeatures
        {
#if DEBUG
            get => UserOptions.Get(ExperimentalFeaturesKey, true);
#else
            get => UserOptions.Get(ExperimentalFeaturesKey, false);
#endif
            set => UserOptions.Set(ExperimentalFeaturesKey, value);
        }


        public const string AllowColorAdornmentsKey = "com.mfractor.settings.xaml.color_adornments";
        public bool AllowColorAdornments
        {
            get => UserOptions.Get(AllowColorAdornmentsKey, true);
            set => UserOptions.Set(AllowColorAdornmentsKey, value);
        }


        public const string AllowGridAdornmentsKey = "com.mfractor.settings.xaml.grid_adornments";
        public bool AllowGridAdornments
        {
            get => UserOptions.Get(AllowGridAdornmentsKey, true);
            set => UserOptions.Set(AllowGridAdornmentsKey, value);
        }

        public const string AllowThicknessAdornmentsKey = "com.mfractor.settings.xaml.thickness_adornments";
        public bool AllowThicknessAdornments
        {
            get => UserOptions.Get(AllowThicknessAdornmentsKey, true);
            set => UserOptions.Set(AllowThicknessAdornmentsKey, value);
        }

        public const string EnableLocalisationAnalysisKey = "com.mfractor.settings.xaml.localisation_analysis";
        public bool EnableLocalisationAnalysis
        {
            get => UserOptions.Get(EnableLocalisationAnalysisKey, false);
            set => UserOptions.Set(EnableLocalisationAnalysisKey, value);
        }

        public const string EnableExtendedLoggingKey = "com.mfractor.settings.support.extended_logging";
        public bool EnableExtendedLogging {
            get => UserOptions.Get(EnableExtendedLoggingKey, false);
            set => UserOptions.Set(EnableExtendedLoggingKey, value);
        }
    }
}
