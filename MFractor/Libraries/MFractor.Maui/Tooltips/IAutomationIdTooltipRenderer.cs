using System;
using MFractor.Maui.Data.Models;
using Microsoft.CodeAnalysis;

namespace MFractor.Maui.Tooltips
{
    public interface IAutomationIdTooltipRenderer
    {
        string CreateTooltip(AutomationIdDeclaration automationId, Project project);
    }
}