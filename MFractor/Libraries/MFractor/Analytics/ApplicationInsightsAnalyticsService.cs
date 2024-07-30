using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MFractor.Configuration;
using MFractor.Heartbeat;
using MFractor.Licensing;
using MFractor.Licensing.MachineIdentification;
using MFractor.Utilities;
using Newtonsoft.Json;

namespace MFractor.Analytics
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IAnalyticsService))]
    class AnalyticsService : IAnalyticsService, IApplicationLifecycleHandler
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IMutableLicensingService> licensingService;
        public IMutableLicensingService LicensingService => licensingService.Value;

        readonly Lazy<IProductHeartbeat> heartbeat;
        public IProductHeartbeat Heartbeat => heartbeat.Value;

        [ImportingConstructor]
        public AnalyticsService(Lazy<IMutableLicensingService> licensingService,
                                Lazy<IProductHeartbeat> heartbeat)
        {
            this.licensingService = licensingService;
            this.heartbeat = heartbeat;
        }

        public void Startup()
        {
            LicensingService.OnLicenseRemoved += LicensingService_OnLicenseRemoved;
            LicensingService.OnLicenseActivated += LicensingService_OnLicenseActivated;

            Heartbeat.Heartbeat += OnHeartbeat;
        }

        TimeSpan elapsed = new TimeSpan(0, 30, 0);

        void OnHeartbeat(object sender, ProductHeartbeatEventArgs e)
        {
            try
            {
                elapsed += e.Elapsed;

                if (elapsed.TotalMinutes > 20)
                {
                    elapsed = new TimeSpan(0, 0, 0);
                    Track("User Active", default);
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }

        void LicensingService_OnLicenseActivated(object sender, LicenseActivatedEventArgs e)
        {
            var props = new Dictionary<string, string>
            {
                ["email"] = e.Details.Email
            };

            if (e.Details.HasName)
            {
                props["name"] = e.Details.Name;
            }

            if (e.Details.IsTrial)
            {
                Track("Trial Activation", props);
            }
            else if (e.Details.IsPaid)
            {
                Track("Professional Activation", props);
            }
        }

        void LicensingService_OnLicenseRemoved(object sender, LicenseRemovedEventArgs e)
        {
            var traits = new Dictionary<string, string>()
            {
                { "User", e.Details.Email }
            };

            Track("License Deactivated", traits);

            if (e.Details.IsTrial)
            {
                Track("Trial License Expired", null);
            }
            else if (e.Details.IsPaid)
            {
                Track("Professional License Expired", null);
            }
        }

        public void Shutdown()
        {
        }

        public void Track(IAnalyticsFeature feature)
        {
            Track(feature, null);
        }

        public void Track(IAnalyticsFeature feature, IReadOnlyDictionary<string, string> traits)
        {
            if (feature is null)
            {
                return;
            }

            Track(feature.AnalyticsEvent, traits);
        }

        public void Track(string eventName)
        {
            Track(eventName, null);
        }

        public void Track(string eventName, IReadOnlyDictionary<string, string> traits)
        {
            if (string.IsNullOrEmpty(eventName))
            {
                return;
            }

            if (eventName != "User Active")
            {
                var eventContent = new UserEvent()
                {
                    Event = eventName,
                    Traits = traits ?? new Dictionary<string, string>(),
                };

                log?.Log("User Event", JsonConvert.SerializeObject(eventContent), Logging.LogLevel.Information);
            }
        }

        class UserEvent
        {
            public string Event { get; set; }

            public IReadOnlyDictionary<string, string> Traits { get; set; }
        }

        public void Track(Exception ex)
        {
            log?.Exception(ex);
        }

        public void Track(Enum @enum)
        {
            if (@enum == null)
            {
                return;
            }

            var @event = @enum.ToString().SeparateUpperLettersBySpace();

            Track(@event);
        }
    }
}