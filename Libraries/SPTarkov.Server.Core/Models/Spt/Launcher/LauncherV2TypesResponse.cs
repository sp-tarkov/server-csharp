﻿using SPTarkov.Server.Core.Models.Utils;

namespace SPTarkov.Server.Core.Models.Spt.Launcher;

public class LauncherV2TypesResponse : IRequestData
{
    public required Dictionary<string, string> Response { get; set; }
}
