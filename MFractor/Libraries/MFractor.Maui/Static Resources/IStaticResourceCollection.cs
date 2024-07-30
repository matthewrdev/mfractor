using System;
using System.Collections.Generic;
using MFractor.Maui.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.StaticResources
{
    public interface IStaticResourceCollection : IEnumerable<StaticResourceDefinition>
    {
        /// <summary>
        /// The file that this static resource collection is for.
        /// </summary>
        string FilePath { get; }

        /// <summary>
        /// The project that this static resource collection is for.
        /// </summary>
        Project Project { get; }

        /// <summary>
        /// The projects that provide the static resources available for <see cref="FilePath"/>.
        /// </summary>
        IReadOnlyList<Project> Projects { get; }

        /// <summary>
        /// The files that provide the static resources available for <see cref="FilePath"/>.
        /// </summary>
        IReadOnlyList<string> SourceFiles { get; }

        IReadOnlyList<StaticResourceDefinition> Find(Func<Project, StaticResourceDefinition, bool> predicate);

        IReadOnlyList<StaticResourceDefinition> GetStyleStaticResourceDefinitions();

        Project GetProjectFor(StaticResourceDefinition definition);
    }
}