using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using MFractor.IOC;

namespace MFractor.Work
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IWorkUnitHandlerRepository))]
    class WorkUnitHandlerRepository : PartRepository<IWorkUnitHandler>, IWorkUnitHandlerRepository
    {
        readonly Logging.ILogger log = Logging.Logger.Create();

        readonly Lazy<IReadOnlyDictionary<Type, IWorkUnitHandler>> workUnitHandlers;
        public IReadOnlyDictionary<Type, IWorkUnitHandler> WorkUnitHandlers => workUnitHandlers.Value;

        [ImportingConstructor]
        public WorkUnitHandlerRepository(Lazy<IPartResolver> partResolver)
            : base(partResolver)
        {
            workUnitHandlers = new Lazy<IReadOnlyDictionary<Type, IWorkUnitHandler>>(() =>
            {
                var result = new Dictionary<Type, IWorkUnitHandler>();

                var count = 0;
                foreach (var WorkUnitHandler in Parts)
                {
                    try
                    {
                        if (result.ContainsKey(WorkUnitHandler.SupportedWorkUnitType))
                        {
                            var existing = result[WorkUnitHandler.SupportedWorkUnitType];
                            log?.Warning($"The {nameof(WorkUnitHandlerRepository)} already contains a work unit handler that can process {WorkUnitHandler.SupportedWorkUnitType} work units. The incoming element {WorkUnitHandler.ToString()} will replace {existing.ToString()}. Was this intended?");
                            Debugger.Break();
                        }

                        result[WorkUnitHandler.SupportedWorkUnitType] = WorkUnitHandler;

                        count++;
                    }
                    catch (Exception ex)
                    {
                        log?.Exception(ex);
                    }
                }

                if (count > 0)
                {
                    log?.Info($"Discovered and registered {count} {typeof(IWorkUnitHandler).Name}'s.");
                }

                return result;
            });
        }

        /// <summary>
        /// Does the workUnit engine have a <see cref="T:MFractor.IWorkUnitHandler"/> for a workUnit of <paramref name="type"/>?.
        /// </summary>
        /// <returns><c>true</c>, if workUnit handler for type was hased, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        public bool IsSupportedWorkUnit<TWorkUnit>() where TWorkUnit : IWorkUnit
        {
            return IsSupportedWorkUnit(typeof(TWorkUnit));
        }

        /// <summary>
        /// Is the given <paramref name="workUnit"/> supported by the workUnit engine?
        /// <para/>
        /// Aka, does the workUnit engine
        /// </summary>
        /// <returns><c>true</c>, if workUnit handler for type was hased, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        public bool IsSupportedWorkUnit(IWorkUnit workUnit)
        {
            if (workUnit is null)
            {
                return false;
            }

            return IsSupportedWorkUnit(workUnit.GetType());
        }

        /// <summary>
        /// Does the workUnit engine have a <see cref="T:MFractor.IWorkUnitHandler"/> for a workUnit of <paramref name="type"/>?.
        /// </summary>
        /// <returns><c>true</c>, if workUnit handler for type was hased, <c>false</c> otherwise.</returns>
        /// <param name="type">Type.</param>
        public bool IsSupportedWorkUnit(Type type)
        {
            if (type is null)
            {
                return false;
            }

            return WorkUnitHandlers.ContainsKey(type);
        }

        /// <summary>
        /// Gets the workUnit handler that can handle the provided workUnit type.
        /// </summary>
        /// <returns>The workUnit handler.</returns>
        /// <param name="type">Type.</param>
        public IWorkUnitHandler GetWorkUnitHandler(Type type)
        {
            if (type is null)
            {
                return null;
            }

            if (!WorkUnitHandlers.ContainsKey(type))
            {
                return null;
            }

            return WorkUnitHandlers[type];
        }

        /// <summary>
        /// Gets the workUnit handler that can handle the provided workUnit type.
        /// </summary>
        /// <returns>The workUnit handler.</returns>
        /// <param name="type">Type.</param>
        /// <typeparam name="TWorkUnitHandler">The 1st type parameter.</typeparam>
        public TWorkUnitHandler GetWorkUnitHandler<TWorkUnitHandler>(Type type) where TWorkUnitHandler : class, IWorkUnitHandler
        {
            return GetWorkUnitHandler(type) as TWorkUnitHandler;
        }

    }
}