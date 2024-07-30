using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using MFractor.Configuration;
using MFractor.Ide;
using MFractor.Images;
using MFractor.Images.Optimisation;
using MFractor.Images.Settings;
using MFractor.IOC;
using MFractor.Utilities;
using Xwt;

namespace MFractor.Views.Settings
{
    public class SettingsWidget : VBox, IOptionsWidget
    {
        [Import]
        protected IUserOptions UserOptions { get; set; }

        [Import]
        protected IIdeFeatureSettings FeatureSettings { get; set; }

        [Import]
        protected IImageFeatureSettings ImageFeatureSettings { get; set; }

        [Import]
        protected IImageOptimisationService ImageOptimisationService { get; set; }

        readonly Logging.ILogger log = Logging.Logger.Create();

        public Widget Widget => this;

        public string Title => "General";

        CheckBox useXamlIntelliSenseCheckbox;

        CheckBox allowColorAdornmentsCheckbox;

        CheckBox allowGridAdornmentsCheckbox;

        CheckBox allowThicknessAdornmentsCheckbox;

        CheckBox automaticFrameworkConfigs;

        CheckBox showProductTips;

        CheckBox experimentalFeatures;

        CheckBox extendedLogging;

        ComboBox minimumAndroidDensitySelection;

        ComboBox defaultIOSResourceType;

        TextEntry tinyPngApiKey;

        public SettingsWidget()
        {
            Resolver.ComposeParts(this);

            BuildIntellisenseOptions();
        }

        void BuildIntellisenseOptions()
        {
            var label = new Label();
            label.Markup = "<b>MFractor Settings</b>";

            PackStart(label);

            BuildGeneralPreferences();

            PackStart(new HSeparator());

            BuildImageToolingPreferences();

            PackStart(new HSeparator());

            BuildFeatureFlagPreferences();

            PackStart(new HSeparator());

            BuildOtherPreferences();
        }

        void BuildGeneralPreferences()
        {
            showProductTips = new CheckBox
            {
                Label = "Show Product Tips",
                Active = FeatureSettings.ShowProductTips,
                TooltipText = "Should MFractor show it's product tip dialog each week?"
            };

            PackStart(showProductTips);

            automaticFrameworkConfigs = new CheckBox
            {
                Label = "Automatic Framework Configurations",
                Active = FeatureSettings.AutomaticFrameworkConfigurations,
                TooltipText = "MFractor includes in-built configurations for popular frameworks that it automaticallys load when that framework is within your project. This let's you work using the best-practices that are recommended by the framework authors.\n\nShould MFractor detect and apply these framework configurations automatically?\n\nPlease close and re-open any open solutions for this change to take affect."
            };

            PackStart(automaticFrameworkConfigs);
        }

        void BuildFeatureFlagPreferences()
        {
            var intelliSenseLabel = new Label
            {
                Markup = "<b>Feature Flags</b>"
            };

            PackStart(intelliSenseLabel);

            useXamlIntelliSenseCheckbox = new CheckBox
            {
                Label = "Use MFractors XAML IntelliSense",
                TooltipText = "Should Visual Studio Mac use MFractors XAML IntelliSense or use the default XAML editor?.\n\nPlease close any active XAML files for this change to take affect.",
                Active = FeatureSettings.UseXAMLIntelliSense,
            };

            PackStart(useXamlIntelliSenseCheckbox);


            allowColorAdornmentsCheckbox = new CheckBox
            {
                Label = "Enable XAML Color Adornments",
                Active = FeatureSettings.AllowColorAdornments,
                TooltipText = "Should MFractor allow color adornments in XAML? If you are seeing any performance loss in the XAML editor, uncheck this item."
            };

            PackStart(allowColorAdornmentsCheckbox);

            allowGridAdornmentsCheckbox = new CheckBox
            {
                Label = "Enable XAML Grid Adornments",
                Active = FeatureSettings.AllowGridAdornments,
                TooltipText = "Should MFractor allow grid index adornments in XAML? If you are seeing performance loss in the XAML editor, uncheck this item."
            };

            PackStart(allowGridAdornmentsCheckbox);

            allowThicknessAdornmentsCheckbox = new CheckBox
            {
                Label = "Enable XAML Thickness Adornments",
                Active = FeatureSettings.AllowGridAdornments,
                TooltipText = "Should MFractor allow Thickness value orientation adornments in XAML? If you are seeing performance loss in the XAML editor, uncheck this item."
            };

            PackStart(allowThicknessAdornmentsCheckbox);

            experimentalFeatures = new CheckBox
            {
                Label = "Enable Experimental Features",
                Active = FeatureSettings.ExperimentalFeatures,
                TooltipText = "Allow access to MFractors experimental features such as the scaffolder?"
            };

            PackStart(experimentalFeatures);
        }

        void BuildOtherPreferences()
        {
            var intelliSenseLabel = new Label
            {
                Markup = "<b>Other</b>"
            };
            PackStart(intelliSenseLabel);

            extendedLogging = new CheckBox
            {
                Label = "Enable Extended Logging (Support only)",
                Active = FeatureSettings.EnableExtendedLogging,
                TooltipText = "You should only enable this option as requested by MFractor support staff. When enabled it will log aditional messages that will help support on finding hard to track issues.",
            };
            PackStart(extendedLogging);
        }

        void BuildImageToolingPreferences()
        {
            var imageToolsLabel = new Label
            {
                Markup = "<b>Image Tools</b>"
            };

            PackStart(imageToolsLabel);

            var androidDensityContainer = new HBox();

            androidDensityContainer.PackStart(new Label()
            {
                Text = "Minimum Android Density:",
                TooltipText = "When the image importer creates new variants of an imported image, what is the lowest density that it should create?\n\nFor example, on modern Android devices it is very rare that ldpi images will be used. You can tell MFractor to exclude creating this image size by specifying mdpi or higher."
            });

            minimumAndroidDensitySelection = new ComboBox();

            foreach (var value in EnumHelper.GetDisplayValues<AndroidImageDensities>())
            {
                var displayValue = value.Item1.ToString();

                minimumAndroidDensitySelection.Items.Add(value, displayValue);
            }

            minimumAndroidDensitySelection.SelectedIndex = (int)ImageFeatureSettings.MinimumAndroidDensity;

            androidDensityContainer.PackStart(minimumAndroidDensitySelection, true);

            PackStart(androidDensityContainer);


            var iosDefaultImageTypeContainer = new HBox();

            iosDefaultImageTypeContainer.PackStart(new Label()
            {
                Text = "iOS Default Image Asset Type:",
                TooltipText = "When the image importer imports a new image asset into an iOS project, you can define if it should use Asset Catalogs or Bundle Resources (deprecated)."
            });

            defaultIOSResourceType = new ComboBox();

            defaultIOSResourceType.Items.Add(ImageResourceType.AssetCatalog, EnumHelper.GetEnumDescription(ImageResourceType.AssetCatalog));
            defaultIOSResourceType.Items.Add(ImageResourceType.BundleResource, EnumHelper.GetEnumDescription(ImageResourceType.BundleResource));

            defaultIOSResourceType.SelectedIndex = defaultIOSResourceType.Items.IndexOf(ImageFeatureSettings.DefaultIOSResourceType);

            iosDefaultImageTypeContainer.PackStart(defaultIOSResourceType, true);

            PackStart(iosDefaultImageTypeContainer);

            var tinyPngApiKeyContainer = new HBox();
            var tinyPngApiKeylabel = new Label("TinyPNG API Key:");

            tinyPngApiKey = new TextEntry()
            {
                Text = ImageFeatureSettings.TinyPNGApiKey,
                PlaceholderText = "Enter your API key for TinyPNG here.",
                TooltipText = "MFractor's Image Manager includes support for shrinking your mobile image assets using TinyPNG.\n\nImage shrinking reduces the size of image assets by applying image quantisation. This may significantly reduce the final size of the image with very little to no visual loss.",
            };

            tinyPngApiKeyContainer.PackStart(tinyPngApiKeylabel);
            tinyPngApiKeyContainer.PackStart(tinyPngApiKey, true, true);

            if (string.IsNullOrEmpty(tinyPngApiKey.Text))
            {
                var actionButton = new Button("Get API Key");
                actionButton.Clicked += (sender, e) =>
                {
                    Process.Start("https://tinypng.com/developers");
                };
                tinyPngApiKeyContainer.PackEnd(actionButton);
            }

            PackStart(tinyPngApiKeyContainer);
        }

        public void ApplyChanges()
        {
            try
            {
                using (UserOptions.StartTransaction())
                {
                    FeatureSettings.UseXAMLIntelliSense = useXamlIntelliSenseCheckbox.Active;

                    ImageFeatureSettings.MinimumAndroidDensity = (AndroidImageDensities)minimumAndroidDensitySelection.SelectedIndex;

                    FeatureSettings.AutomaticFrameworkConfigurations = automaticFrameworkConfigs.Active;

                    FeatureSettings.ExperimentalFeatures = experimentalFeatures.Active;

                    FeatureSettings.ShowProductTips = showProductTips.Active;

                    FeatureSettings.AllowColorAdornments = allowColorAdornmentsCheckbox.Active;

                    FeatureSettings.AllowGridAdornments = allowGridAdornmentsCheckbox.Active;

                    FeatureSettings.AllowThicknessAdornments = allowThicknessAdornmentsCheckbox.Active;

                    ImageFeatureSettings.TinyPNGApiKey = tinyPngApiKey.Text;

                    ImageFeatureSettings.DefaultIOSResourceType = (ImageResourceType)defaultIOSResourceType.SelectedItem;

                    ImageOptimisationService.SetApiKey(tinyPngApiKey.Text);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
