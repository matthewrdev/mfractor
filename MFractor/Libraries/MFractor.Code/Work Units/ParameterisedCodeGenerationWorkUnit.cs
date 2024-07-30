using System;
using System.Collections.Generic;
using MFractor.Code.CodeGeneration.Options;
using MFractor.Work;

namespace MFractor.Code.WorkUnits
{
    public delegate IReadOnlyList<IWorkUnit> ParameterisedCodeGenerationDelegate(ICodeGenerationOptionSet options);

    public class ParameterisedCodeGenerationWorkUnit : WorkUnit
    {
        public ParameterisedCodeGenerationWorkUnit(string title,
                                                   string confirmMessage,
                                                   string helpUrl,
                                                   ParameterisedCodeGenerationDelegate @delegate,
                                                   ICodeGenerationOptionSet options)
        {
            Title = title;
            ConfirmMessage = confirmMessage;
            HelpUrl = helpUrl; 
            Delegate = @delegate;
            Options = options;
        }

        public string Title { get; }

        public string ConfirmMessage { get; }

        public string HelpUrl { get; }

        public ParameterisedCodeGenerationDelegate Delegate { get; }

        public ICodeGenerationOptionSet Options { get; }
    }
}
