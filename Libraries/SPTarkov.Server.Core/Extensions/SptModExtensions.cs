using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SPTarkov.Server.Core.Models.Spt.Mod;

namespace SPTarkov.Server.Core.Extensions
{
    public static class SptModExtensions
    {
        public static string GetModPath(this SptMod sptMod)
        {
            var relativeModPath = Path.GetRelativePath(Directory.GetCurrentDirectory(), sptMod.Directory).Replace('\\', '/');

            return relativeModPath;
        }
    }
}
