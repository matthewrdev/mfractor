using System;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Services
{
    public interface IConvertProjectToCSharp9Service
    {
        bool CanConvertProject(Project project);

        void Convert(Project project);
    }
}
