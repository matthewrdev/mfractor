using System;
using System.Collections.Generic;

namespace MFractor.Code.Scaffolding
{
    /// <summary>
    /// The input string provided to the scaffolder.
    /// <para/>
    /// The scaffolder input is broken down into the core components that a <see cref="IScaffolder"/> may need.
    /// </summary>
    public interface IScaffoldingInput
    {
        /// <summary>
        /// The raw input that the user provided to the scaffolder.
        /// <para/>
        /// This includes the path, file name and extension in a single string.
        /// <para/>
        /// Note that any separators for the path may not be valid for the current platform.
        /// <para/>
        /// The <see cref="RawInput"/> should never be null.
        /// </summary>
        string RawInput { get; }

        /// <summary>
        /// The folder path relative to the project root provided into the scaffolder.
        /// <para/>
        /// The <see cref="VirtualFolderPath"/> should never be null.
        /// </summary>
        IReadOnlyList<string> VirtualFolderPath { get; }

        /// <summary>
        /// The folder path that the user provided to the scaffolder.
        /// </summary>
        string FolderPath { get; }

        /// <summary>
        /// The name (including file extension) that the user provided to the scaffolder.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The file extension (including the ".") that the user provided to the scaffolder.
        /// <para/>
        /// May be empty but will never be null.
        /// </summary>
        string Extension { get; }

        /// <summary>
        /// The <see cref="Name"/> without any file extension.
        /// </summary>
        string NameNoExtension { get; }

        /// <summary>
        /// If the user provided any input to the scaffolder.
        /// </summary>
        bool HasInput { get; }
    }
}
