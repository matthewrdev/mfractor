using System;
using MFractor.Workspace;
using Microsoft.CodeAnalysis;

namespace MFractor.Android
{
    public interface IAndroidManifestResolver
    {
        IProjectFile ResolveAndroidManifest(Project project);
    }
}