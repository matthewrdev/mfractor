using System;
using System.Diagnostics;

namespace MFractor.Concurrency
{
    [DebuggerDisplay("{value}")]
    public class ConcurrentValue<TValue>
    {
        private readonly object valueLock = new object();
        private TValue value;

        public ConcurrentValue()
        {
        }

        public ConcurrentValue(TValue value)
        {
            this.value = value;
        }

        public void Set(TValue value)
        {
            lock (valueLock)
            {
                this.value = value;
            }
        }

        /// <summary>
        /// Mutates the inner value by reference.
        /// <para/>
        /// Use when <typeparamref name="TValue"/> is mutable/reference data type.
        /// </summary>
        /// <param name="mutator"></param>
        public void Mutate(Action<TValue> mutator)
        {
            if (mutator is null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            lock (valueLock)
            {
                mutator(value);
            }
        }

        /// <summary>
        /// Mutates the inner value by returning a new value via the <paramref name="mutator"/> and re-assigning it.
        /// <para/>
        /// Use when <typeparamref name="TValue"/> is an immutable data type such as <see cref="String"/>.
        /// </summary>
        /// <param name="mutator"></param>
        public void Mutate(Func<TValue, TValue> mutator)
        {
            if (mutator is null)
            {
                throw new ArgumentNullException(nameof(mutator));
            }

            lock (valueLock)
            {
                value = mutator(value);
            }
        }

        public TValue Get()
        {
            lock (valueLock)
            {
                return value;
            }
        }

        public TResult Get<TResult>(Func<TValue, TResult> predicate)
        {
            if (predicate is null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            lock (valueLock)
            {
                return predicate(value);
            }
        }
    }
}
