using System;
using System.Diagnostics;

namespace MFractor
{
    /// <summary>
    /// A line and column based location.
    /// </summary>
    [DebuggerDisplay("{Line},{Column}")]
    public class FileLocation
    {
        /// <summary>
        /// Creates a new empty <see cref="FileLocation"/>.
        /// </summary>
        public FileLocation()
        {
        }

        /// <summary>
        /// Creates a new <see cref="FileLocation"/> with the given <paramref name="line"/> and <paramref name="column"/>/
        /// </summary>
        /// <param name="line"></param>
        /// <param name="column"></param>
        public FileLocation(int line, int column)
        {
            Line = line;
            Column = column;
        }

        /// <summary>
        /// The line of this <see cref="FileLocation"/>.
        /// </summary>
        public int Line { get; set; }

        /// <summary>
        /// The column of this <see cref="FileLocation"/>.
        /// </summary>
        public int Column { get; set; }
    }
}
