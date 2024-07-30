using System;
namespace MFractor
{
    /// <summary>
    /// Launches and shuts down the core MFractor engine.
    /// </summary>
    interface IBootstrapper
    {
        /// <summary>
        /// Starts MFractor.
        /// </summary>
        void Start();

        /// <summary>
        /// Runs MFractors shutdown routine.
        /// </summary>
        void Shutdown();
    }
}
