using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using MFractor.Attributes;
using MFractor.Utilities;

namespace MFractor
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IBootstrapper))]
    class Bootstrapper : IBootstrapper
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        bool isBootstrapped;

        readonly Lazy<IApplicationLifecycleHandlerRepository> applicationLifecycleHandlers;
        public IApplicationLifecycleHandlerRepository ApplicationLifecycleHandlers => applicationLifecycleHandlers.Value;

        readonly Lazy<IProductInformation> productInformation;
        public IProductInformation ProductInformation => productInformation.Value;

        [ImportingConstructor]
        public Bootstrapper(Lazy<IApplicationLifecycleHandlerRepository> applicationLifecycleHandlers,
                            Lazy<IProductInformation> productInformation)
        {
            this.applicationLifecycleHandlers = applicationLifecycleHandlers;
            this.productInformation = productInformation;
        }

        public void Start()
        {
            if (isBootstrapped)
            {
                throw new InvalidOperationException("The bootstrapper has already been started.");
            }

            using (Profiler.Profile())
            {
                try
                {
                    var productInfo = ProductInformation;

                    log?.Info($"Starting MFractor for {productInfo.ProductVariant}...");

                    var handlers = new List<IApplicationLifecycleHandler>();
                    using (Profiler.Profile($"Resolve {typeof(IApplicationLifecycleHandler).Name} implementations"))
                    {
                        handlers.AddRange(ApplicationLifecycleHandlers.ApplicationLifecycleHandlers);
                    }

                    log?.Info($"Discovered {handlers.Count} lifecycle handlers");

                    var prioritiesHandlers = handlers.Where(h => AttributeHelper.HasAttribute<ApplicationLifecyclePriorityAttribute>(h.GetType()))
                                                      .OrderByDescending(h => AttributeHelper.GetAttribute<ApplicationLifecyclePriorityAttribute>(h.GetType()).Priority)
                                                      .ToList();

                    using (var ph = Profiler.Profile("Starting priority lifecycle handlers"))
                    {
                        foreach (var priorityHandler in prioritiesHandlers)
                        {
                            handlers.Remove(priorityHandler);

                            log?.Info("Starting: " + priorityHandler.GetType().Name);
                            try
                            {
                                using (Profiler.Profile(priorityHandler.GetType().Name + ".Startup"))
                                {
                                    priorityHandler.Startup();
                                }
                            }
                            catch (Exception ex)
                            {
                                log?.Exception(ex);
                            }
                        }
                    }

                    using (var ph = Profiler.Profile("Starting standard lifecycle handlers"))
                    {
                        foreach (var handler in handlers)
                        {
                            log?.Info("Starting: " + handler.GetType().Name);
                            try
                            {
                                using (Profiler.Profile(handler.GetType().Name + ".Startup"))
                                {
                                    handler.Startup();
                                }
                            }
                            catch (Exception ex)
                            {
                                log?.Exception(ex);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    log?.Exception(ex);
                }
                finally
                {
                    isBootstrapped = true;
                }
            }
        }

        public void Shutdown()
        {
            try
            {
                foreach (var handler in ApplicationLifecycleHandlers.Parts)
                {
                    log?.Info("Stopping: " + handler.GetType().Name);
                    try
                    {
                        handler.Shutdown();
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }
            }
            catch (Exception ex)
            {
                log?.Exception(ex);
            }
        }
    }
}
