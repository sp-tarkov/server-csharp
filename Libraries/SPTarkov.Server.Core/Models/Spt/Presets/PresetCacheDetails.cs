namespace SPTarkov.Server.Core.Models.Spt.Presets
{
    public record PresetCacheDetails
    {
        // Preset Ids related to the tpl
        public HashSet<string> PresetIds
        {
            get;
            set;
        }

        // Id of the default preset for this tpl
        public string? DefaultId
        {
            get;
            set;
        }
    }
}
