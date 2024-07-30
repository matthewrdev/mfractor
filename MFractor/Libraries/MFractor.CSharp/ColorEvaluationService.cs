using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis;

namespace MFractor.CSharp.Services
{
    [PartCreationPolicy(CreationPolicy.Shared)]
    [Export(typeof(IColorEvaluationService))]
    class ColorEvaluationService : IColorEvaluationService
    {
        // Color.Red (Named color)
        // Color.FromRgb (int/byte = 256, double/float = 1.0)
        // Color.FromRgba (int/byte = 256, double/float = 1.0)
        // Color.FromHsla
        // Color.FromHsv
        // Color.FromHsva
        // Color.FromUint
        // Color.Rgb
        // Color.Argb
        // new Color(...) (int/byte = 256, double/float = 1.0), (inspect arg names for values/ordering)
        // new UIColor(...) (int/byte = 256, double/float = 1.0), (inspect arg names for values/ordering)
        // new CGColor(...) (int/byte = 256, double/float = 1.0), (inspect arg names for values/ordering)
        // new CIColor(...) (int/byte = 256, double/float = 1.0), (inspect arg names for values/ordering)

        public ColorEvalautionResult Evaluate(Project project, string filePath, int position)
        {
            throw new NotImplementedException();
        }

        public ColorEvalautionResult Evaluate(SyntaxNode syntaxNode)
        {
            throw new NotImplementedException();
        }
    }
}