using System;
using MFractor.Localisation.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation.Tooltips
{
    public interface ILocalisationTooltipRenderer
    {
        string CreateLocalisationTooltip(ResXLocalisationDefinition definition, Project project);
        string CreateLocalisationTooltip(ILocalisationDeclarationCollection localisations);
    }
}