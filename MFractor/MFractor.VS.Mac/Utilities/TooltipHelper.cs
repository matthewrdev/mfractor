using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MFractor.Data;
using MFractor.Data.Repositories;
using MFractor.Maui;
using MFractor.Maui.Data.Models;
using MFractor.IOC;
using MFractor.Localisation.Data.Models;
using MFractor.Localisation.Data.Repositories;
using MFractor.Utilities;
using MFractor.Views.Branding;
using Microsoft.CodeAnalysis;
using MonoDevelop.Ide.CodeCompletion;

namespace MFractor.VS.Mac.Utilities
{
    public static class TooltipHelper
    {
        public static TooltipInformation AddMFractorBranding(this TooltipInformation tooltip)
        {
            if (string.IsNullOrEmpty(tooltip.FooterMarkup))
            {
                tooltip.FooterMarkup = BrandingHelper.PangoFormattedBrandingText;
            }
            else
            {
                tooltip.FooterMarkup += "\n\n" + BrandingHelper.PangoFormattedBrandingText;
            }

            return tooltip;
        }
    }
}

