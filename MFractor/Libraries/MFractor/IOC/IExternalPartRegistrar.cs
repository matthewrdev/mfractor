using System;


namespace MFractor.IOC
{
    /// <summary>
    /// When MFractor is intergrated into a standalone product 
    /// </summary>
    public interface IExternalPartRegistrar
    {
        void RegisterMultiInstance<RegisterType, RegisterImplementation>() where RegisterImplementation : RegisterType, new();

        void RegisterMultiInstance<RegisterType>(RegisterType instance);

        void RegisterMultiInstance<RegisterType, RegisterImplementation>(Func<RegisterImplementation> factory) where RegisterImplementation : RegisterType;

        void RegisterSingleton<RegisterType, RegisterImplementation>() where RegisterImplementation : RegisterType, new();

        void RegisterSingleton<RegisterType>(RegisterType instance);

        void RegisterSingleton<RegisterType>(Func<RegisterType> factory);
    }
}
