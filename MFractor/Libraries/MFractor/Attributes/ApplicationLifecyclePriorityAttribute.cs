using System;
namespace MFractor.Attributes
{
    /// <summary>
    /// Use the <see cref="ApplicationLifecyclePriorityAttribute"/> to specify that an <see cref="IApplicationLifecycleHandler"/> implementation should have its <see cref="IApplicationLifecycleHandler.Startup"/> method invoked at a higher or lower priority than other lifecycle handlers. 
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class ApplicationLifecyclePriorityAttribute : Attribute
    {
        public ApplicationLifecyclePriorityAttribute()
        {
            Priority = uint.MaxValue / 2;
        }

        public ApplicationLifecyclePriorityAttribute(uint priority)
        {
            Priority = priority;
        }

        /// <summary>
        /// The startup execution priority of this application lifecycle handler.
        /// <para/>
        /// Higher values means higher priority
        /// </summary>
        public uint Priority { get; }
    }
}
