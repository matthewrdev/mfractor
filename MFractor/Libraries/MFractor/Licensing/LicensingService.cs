using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using MFractor.Configuration;
using MFractor.Heartbeat;
using MFractor.Licensing.Consumer;
using MFractor.Xml;

namespace MFractor.Licensing
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(ILicensingService))]
    [Export(typeof(IMutableLicensingService))]
    class LicensingService : ILicensingService, IMutableLicensingService
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        public event EventHandler<EventArgs> LicenseServiceStarted;

        LicenseConfig licenseConfig;

        LicenseConsumer licenseConsumer;

        ProductLicensingInfo licensingInfo;

        readonly Lazy<IUserOptions> userOptions;
        public IUserOptions UserOptions => userOptions.Value;

        readonly Lazy<IApplicationPaths> applicationPaths;
        public IApplicationPaths ApplicationPaths => applicationPaths.Value;

        readonly Lazy<ILicenseSigningInformation> licenseSigningInformation;
        public ILicenseSigningInformation LicenseSigningInformation => licenseSigningInformation.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        readonly Lazy<IPlatformService> platformService;
        public IPlatformService PlatformService => platformService.Value;

        readonly Lazy<IProductHeartbeat> heartbeat;
        public IProductHeartbeat Heartbeat => heartbeat.Value;

        readonly Lazy<IXmlSyntaxParser> xmlSyntaxParser;
        public IXmlSyntaxParser XmlSyntaxParser => xmlSyntaxParser.Value;

        readonly Lazy<IXmlSyntaxWriter> xmlSyntaxWriter;
        public IXmlSyntaxWriter XmlSyntaxWriter => xmlSyntaxWriter.Value;

        [ImportingConstructor]
        public LicensingService(Lazy<IUserOptions> userOptions,
                                Lazy<IApplicationPaths> applicationPaths,
                                Lazy<IProductInformation> productInformation,
                                Lazy<ILicenseSigningInformation> licenseSigningInformation,
                                Lazy<IPlatformService> platformService,
                                Lazy<IProductHeartbeat> heartbeat,
                                Lazy<IXmlSyntaxParser> xmlSyntaxParser,
                                Lazy<IXmlSyntaxWriter> xmlSyntaxWriter)
        {
            this.userOptions = userOptions;
            this.applicationPaths = applicationPaths;
            this.productInformation = productInformation;
            this.licenseSigningInformation = licenseSigningInformation;
            this.platformService = platformService;
            this.heartbeat = heartbeat;
            this.xmlSyntaxParser = xmlSyntaxParser;
            this.xmlSyntaxWriter = xmlSyntaxWriter;

            Setup(LicenseSigningInformation.ProductLicensingInfo);
        }

        public void Setup(ProductLicensingInfo licensingInfo)
        {
            this.licensingInfo = licensingInfo;

#if TRIAL
            if (File.Exists(ActivationConfigFilePath))
            {
                File.Delete(ActivationConfigFilePath);
            }
#endif

            licenseConsumer = new LicenseConsumer(new PublicKey(this.licensingInfo.PublicKey), new PublicKey(this.licensingInfo.LegacyKey));

            if (File.Exists(ActivationConfigFilePath))
            {
                licenseConfig = ParseConfigFile(ActivationConfigFilePath);
            }

            if (licenseConfig != null && licenseConfig.HasLicense)
            {
                licenseConsumer.Load(licenseConfig.License);
            }

            LicenseServiceStarted?.Invoke(this, new EventArgs());

            isPaid = licenseConsumer.IsLoaded && licenseConsumer.IsValid();
            Heartbeat.Heartbeat += LicenseHeartbeat_Heartbeat;

            HasStarted = true;
        }

        void LicenseHeartbeat_Heartbeat(object sender, ProductHeartbeatEventArgs args)
        {
            try
            {
                isPaid = licenseConsumer.IsValid();
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        bool isPaid;

        public string ActivationConfigFilePath => Path.Combine(ApplicationPaths.ApplicationDataPath, licenseConfigFileName);


#if TRIAL
        const string licenseConfigFileName = ".trial_license_config";
#else
        const string licenseConfigFileName = ".license_config";
#endif

        protected LicenseConfig Config
        {
            get
            {
                if (licenseConfig == null)
                {
                    if (File.Exists(ActivationConfigFilePath))
                    {
                        licenseConfig = ParseConfigFile(ActivationConfigFilePath);
                    }
                }

                return licenseConfig;
            }
        }

        LicenseConfig ParseConfigFile(string filePath)
        {
            var syntax = XmlSyntaxParser.ParseFile(filePath);

            if (syntax == null
                || syntax.Root == null
                || syntax.Root.Name.FullName != "LicenseConfig")
            {
                return default;
            }

            var name = syntax.Root.GetChildNode("Name")?.Value;
            var email = syntax.Root.GetChildNode("Email")?.Value;
            var license = syntax.Root.GetChildNode("License")?.Value;

            return new LicenseConfig()
            {
                Name = name,
                Email = email,
                License = license
            };
        }

        public bool HasActivation
        {
            get
            {
                if (licenseConfig == null)
                {
                    return false;
                }

                return licenseConfig.Activated;
            }
        }

        public string ActivationEmail
        {
            get
            {
                if (licenseConfig == null)
                {
                    return "";
                }

                return licenseConfig.Email;
            }
        }

        public string ActivationName
        {
            get
            {
                if (licenseConfig == null)
                {
                    return "";
                }

                return licenseConfig.Name;
            }
        }

        public event EventHandler<LicenseActivatedEventArgs> OnLicenseActivated;

        public event EventHandler<LicenseRemovedEventArgs> OnLicenseRemoved;

        public bool HasLicense => IsPaid || IsTrial;

        public bool IsPaid
        {
            get
            {
                return true;

                if (licenseConsumer == null)
                {
                    return false;
                }

                if (licenseConfig == null)
                {
                    return false;
                }

                if (licenseConfig.HasLicense == false)
                {
                    return false;
                }

                return isPaid;
            }
        }

        public bool IsTrial
        {
            get
            {
                if (licenseConsumer == null)
                {
                    return false;
                }

                if (licenseConfig == null)
                {
                    return false;
                }

                if (licenseConfig.HasLicense == false)
                {
                    return false;
                }

                return licenseConsumer.IsTrial;
            }
        }

        public LicenseDetails LicensingDetails
        {
            get
            {
                if (!licenseConsumer.IsLoaded)
                {
                    return new LicenseDetails("NA", ActivationEmail, false, false, false, false, null);
                }

                var name = licenseConsumer.Name;
                var email = licenseConsumer.Email;
                var professional = licenseConsumer.IsProfessional;
                var trial = licenseConsumer.IsTrial;
                var expiry = licenseConsumer.Expiry;
                var hasExpired = licenseConsumer.HasExpired(DateTime.UtcNow);

                return new LicenseDetails(name, email, professional, trial, HasActivation, hasExpired, expiry);
            }
        }

        public void Activate(LicensedUserInformation options)
        {
            var license = licenseConfig?.License;

            licenseConfig = new LicenseConfig
            {
                Email = options.Email,
                Name = options.Name,
                License = license
            };

            SerializeConfig(ActivationConfigFilePath, licenseConfig);

            this.OnLicenseActivated?.Invoke(this, new LicenseActivatedEventArgs(LicensingDetails));
        }

        void SerializeConfig(string filePath, LicenseConfig config)
        {
            var node = new XmlNode(nameof(LicenseConfig));
            if (config.HasName)
            {
                node.AddChildNode(new XmlNode(nameof(LicenseConfig.Name)) { Value = config.Name });
            }
            node.AddChildNode(new XmlNode(nameof(LicenseConfig.Email)) { Value = config.Email });
            node.AddChildNode(new XmlNode(nameof(LicenseConfig.License)) { Value = config.License });

            var content = XmlSyntaxWriter.WriteNode(node, string.Empty, DefaultXmlFormattingPolicy.Instance, true, true, false);

            File.WriteAllText(filePath, content);
        }

        public bool ImportLicense(FileInfo license, out IReadOnlyList<string> issues)
        {
            if (!File.Exists(license.FullName))
            {
                issues = new List<string>()
                {
                    $"MFractor could not import the license file \"{license.FullName}\". Are you sure that this file exists?"
                };
                return false;
            }

            var licenseContent = File.ReadAllText(license.FullName);

            return ImportLicense(licenseContent, out issues);
        }

        public bool ImportLicense(string licenseContent, out IReadOnlyList<string> issues)
        {
            issues = new List<string>();

            try
            {
                licenseConsumer.Load(licenseContent);
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
                issues = new List<string>()
                {
                    $"MFractor could not load the license provided; it appears to be either corrupted or an invalid MFractor license. Please contact matthew@mfractor.com for support."
                };
                return false;
            }

            var email = licenseConsumer.Email;

            var failures = licenseConsumer.Validate();

            if (failures.Any())
            {
                issues = failures.Select(f => f.Message + ". " + f.HowToResolve).ToList();
                return false;
            }

            if (licenseConsumer.HasExpired(DateTime.UtcNow))
            {
                issues = new List<string>()
                {
                    $"The license has expired. Please purchase a new MFractor license at www.mfractor.com/buy"
                };
                return false;
            }

            licenseConfig = new LicenseConfig()
            {
                Email = email,
                License = licenseContent,
            };

            if (email != ActivationEmail)
            {
                Activate(new LicensedUserInformation(email, licenseConsumer.Name, licenseConsumer.Company));
            }

            SerializeConfig(ActivationConfigFilePath, licenseConfig);

            UserOptions.Set(hasShownOneDayExpiryMessage, false);
            UserOptions.Set(hasShownSevenDayExpiryMessage, false);
            UserOptions.Set(hasShownFourteenDayExpiryMessage, false);

            isPaid = licenseConsumer.IsValid();

            try
            {
                OnLicenseActivated?.Invoke(this, new LicenseActivatedEventArgs(LicensingDetails));
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }

            return true;
        }

        const string hasShownFourteenDayExpiryMessage = "com.mfractor.licensing.expiry.fourteen_days";
        const string hasShownSevenDayExpiryMessage = "com.mfractor.licensing.expiry.seven_days";
        const string hasShownOneDayExpiryMessage = "com.mfractor.licensing.expiry.one_day";

        public bool RemoveActiveLicense(out IReadOnlyList<string> issues)
        {
            issues = new List<string>();
            if (Config == null)
            {
                issues = new List<string>()
                {
                    "This installation of MFractor is not activated"
                };
                return false;
            }

            var temp = licenseConfig;
            temp.License = null;

            SerializeConfig(ActivationConfigFilePath, temp);

            UserOptions.Set(hasShownOneDayExpiryMessage, false);
            UserOptions.Set(hasShownSevenDayExpiryMessage, false);
            UserOptions.Set(hasShownFourteenDayExpiryMessage, false);

            var details = LicensingDetails;

            licenseConfig = temp;
            licenseConsumer.Unload();
            OnLicenseRemoved?.Invoke(this, new LicenseRemovedEventArgs(details));

            Console.WriteLine("License is paid activation: " + (IsPaid ? "Yes" : "No"));

            return true;
        }

        public LicenseStatusMessage GetLicenseStatusMessage()
        {
            var details = LicensingDetails;

            if (details == null)
            {
                return null;
            }

            var isPaidOrTrial = details.IsPaid || details.IsTrial;
            if (!isPaidOrTrial)
            {
                return null;
            }


            var licenseKind = details.IsTrial ? "MFractor trial license" : "MFractor Professional license";

            if (details.HasExpired)
            {
                RemoveActiveLicense(out var issues);
                return new LicenseStatusMessage("License Expired",
                                                $"Your {licenseKind} has expired and you are now using MFractor Lite. To continue using MFractor Professional, please renew or purchase a new license at www.mfractor.com/buy",
                                                LicenseStatusMessageKind.LicenseExpired);
            }

            var now = DateTime.UtcNow;

            var expiryTimeSpan = licenseConsumer.Expiry.Value - now;

            if (expiryTimeSpan.Days == 14 && !UserOptions.Get(hasShownFourteenDayExpiryMessage, false))
            {
                UserOptions.Set(hasShownFourteenDayExpiryMessage, true);
                return new LicenseStatusMessage("MFractor License Expiring Soon",
                                                $"Your {licenseKind} will expire in 14 days. To continue using MFractor Professional, please renew or purchase a newlicense at www.mfractor.com/buy",
                                                LicenseStatusMessageKind.LicenseExpiresOneFortnight);
            }


            if (expiryTimeSpan.Days == 7 && !UserOptions.Get(hasShownSevenDayExpiryMessage, false))
            {
                UserOptions.Set(hasShownSevenDayExpiryMessage, true);
                return new LicenseStatusMessage("MFractor License Expiring Soon",
                                                $"Your {licenseKind} will expire in 7 days. To continue using MFractor Professional, please renew or purchase a new license at www.mfractor.com/buy",
                                                LicenseStatusMessageKind.LicenseExpiresOneWeek);
            }

            if (expiryTimeSpan.Days == 1 && !UserOptions.Get(hasShownOneDayExpiryMessage, false))
            {
                UserOptions.Set(hasShownOneDayExpiryMessage, true);
                return new LicenseStatusMessage("MFractor License Expiring Tomorrow",
                                                $"Your {licenseKind} will expire tomorrow. To continue using MFractor Professional, please renew or purchase a new license at www.mfractor.com/buy",
                                                LicenseStatusMessageKind.LicenseExpiresTomorrow);
            }

            return null;
        }

        public bool HasStarted { get; private set; }

        public bool IsUsingLegacyLicense => false;
    }
}
