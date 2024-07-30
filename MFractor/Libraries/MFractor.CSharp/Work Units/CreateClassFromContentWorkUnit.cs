using System;
using System.Collections.Generic;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.WorkUnits
{
    public class CreateClassFromContentWorkUnit : WorkUnit
    {
        public CreateClassFromContentWorkUnit(string defaultName,
                                              Project defaultProject,
                                              string folderPath,
                                              string content)
        {
            DefaultClassName = defaultName;
            Project = defaultProject;
            FolderPath = folderPath;
            Content = content;
        }

        public string DefaultClassName { get; }

        public Project Project { get; }

        public string FolderPath { get; }

        public string Content { get; }
    }
}
