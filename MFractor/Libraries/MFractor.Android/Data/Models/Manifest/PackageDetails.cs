﻿using System;
using MFractor.Data.Models;
using MFractor.Workspace.Data.Models;

namespace MFractor.Android.Data.Models.Manifest
{
    public class PackageDetails : ProjectFileOwnedEntity
    {
        public string PackageName { get; set; }
    }
}