﻿using System;
using System.Collections.Generic;
using System.Linq;
using MFractor.Work;

namespace MFractor.Work
{
    public class WorkUnitEventArgs : EventArgs
    {
        {
            if (workUnits is null)
            {
                throw new ArgumentNullException(nameof(workUnits));
            }

            WorkUnits = workUnits.ToList();
        }

        public IReadOnlyList<IWorkUnit> WorkUnits { get; }
    }
}