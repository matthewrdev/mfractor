using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace MFractor.Utilities
{
    /// <summary>
    /// Profiles a section of code by using the <see cref="IDisposable"/> pattern.
    /// <para/>
    /// To use the profiler, create a new profiler with <see cref="Profile(string, string)"/> inside a using statement.
    /// </summary>
    public class Profiler : IDisposable
    {
        class ProfilerItem
        {
            public int Callcount;
            public TimeSpan TotalDuration;
        }

        public bool Cancelled { get; set; }

        /// <summary>
        /// The message of this profiler.
        /// </summary>
        /// <value>The message.</value>
        public string Message { get; }

        Action<string, TimeSpan> doneHandler;

        Stopwatch watch;

        public Profiler Parent { get; }

        readonly Dictionary<string, ProfilerItem> items = new Dictionary<string, ProfilerItem>();

        /// <summary>
        /// Initializes a new instance of the <see cref="Profiler"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="onDone">On done.</param>
        public Profiler(string message, 
                        Action<string, TimeSpan> onDone = null)
        {
            Message = message;

            doneHandler = onDone ?? new Action<string, TimeSpan>((content, time) =>
            {
                var elapsed = Math.Round(time.TotalMilliseconds, 2);
                Console.WriteLine($"{content} took {elapsed}ms");

                if (items.Any())
                {
                    foreach (var item in items)
                    {
                        var average = (item.Value.TotalDuration.TotalMilliseconds / (double)item.Value.Callcount);
                        Console.WriteLine(item.Key);
                        Console.WriteLine("    Total duration: " + Math.Round(item.Value.TotalDuration.TotalMilliseconds, 2).ToString());
                        Console.WriteLine("    Total callcount: " + item.Value.Callcount.ToString());
                        Console.WriteLine("    Average: " +  Math.Round(average, 2));
                    }
                }
            });
            watch = Stopwatch.StartNew();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Profiler"/> class.
        /// </summary>
        /// <param name="message">Message.</param>
        /// <param name="parent">Parent.</param>
        /// <param name="record">If set to <c>true</c> record.</param>
        public Profiler(string message,
                        Profiler parent,
                        bool record)
        {
            Message = message;
            Parent = parent;

            if (record)
            {
                doneHandler = new Action<string, TimeSpan>((content, time) =>
                {
                    parent?.Record(message, time);
                });
            }

            watch = Stopwatch.StartNew();
        }

        void Record(string message, TimeSpan time)
        {
            if (!items.ContainsKey(message))
            {
                items.Add(message, new ProfilerItem());
            }

            items[message].Callcount += 1;
            items[message].TotalDuration += time;
        }

        /// <summary>
        /// Releases all resource used by the <see cref="Profiler"/> object.
        /// </summary>
        /// <remarks>Call <see cref="Dispose"/> when you are finished using the <see cref="MFractor.Utilities.Profiler"/>. The
        /// <see cref="Dispose"/> method leaves the <see cref="MFractor.Utilities.Profiler"/> in an unusable state.
        /// After calling <see cref="Dispose"/>, you must release all references to the
        /// <see cref="Profiler"/> so the garbage collector can reclaim the memory that the
        /// <see cref="Profiler"/> was occupying.</remarks>
        public void Dispose()
        {
            if (!Cancelled)
            {
                doneHandler?.Invoke(Message, watch.Elapsed);
            }

            watch = null;
            doneHandler = null;
        }

        /// <summary>
        /// Creates a new <see cref="Profiler"/> instance, automatically capturing the caller and file that it is profiling.
        /// </summary>
        /// <returns>The profile.</returns>
        /// <param name="context">Context.</param>
        /// <param name="file">File.</param>
        public static Profiler Profile([CallerMemberName] string context = "", [CallerFilePath] string file = "")
        {
            var fileName = Path.GetFileName(file);

            var profileContext = fileName + "|" + context;

            return new Profiler(profileContext);
        }
    }
}

