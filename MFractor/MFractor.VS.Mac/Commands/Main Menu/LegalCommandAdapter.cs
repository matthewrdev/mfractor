using System;
using MFractor.Commands.MainMenu.Legal;

namespace MFractor.VS.Mac.Commands.MainMenu
{
    class LegalCommandAdapter : IdeCommandAdapter<LegalCompositeCommand>
    {
        protected override bool RequiresActivation => false;
    }
}