﻿using System;
using MFractor.Commands.MainMenu;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class RecoverLicenseCommandAdapter : IdeCommandAdapter<RecoverLicenseCommand>
    {
        protected override bool RequiresActivation => false;
    }
}
