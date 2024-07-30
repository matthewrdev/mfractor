using System;
using System.Collections;
using System.Collections.Generic;
using MFractor.Localisation.Data.Models;
using MFractor.Work;
using Microsoft.CodeAnalysis;

namespace MFractor.Localisation
{
    public interface ILocalisationNavigationService
    {
        IReadOnlyList<IWorkUnit> Navigate(ResXLocalisationDefinition localisationDefinition, Project project);
    }
}