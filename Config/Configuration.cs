using System;
using System.Collections.Generic;
using Dalamud.Configuration;
using Dalamud.Plugin;
using DotNet.Globbing;

namespace SoundFilter.Config {
    [Serializable]
    internal class Configuration : IPluginConfiguration {
        internal const int LatestVersion = 2;

        public int Version { get; set; } = LatestVersion;

        private IDalamudPluginInterface? Interface { get; set; }
        private Dictionary<string, Glob> CachedGlobs { get; } = new();

        public bool Enabled = true;
        public bool ShowLog;
        public bool LogEnabled = true;
        public bool LogFiltered = false;
        public uint LogEntries = 250;
        public List<CustomFilter> Filters { get; set; } = new();

        internal IReadOnlyDictionary<Glob, bool> Globs {
            get {
                var dictionary = new Dictionary<Glob, bool>();
                foreach (var filter in this.Filters) {
                    foreach (var globString in filter.Globs) {
                        if (this.CachedGlobs.TryGetValue(globString, out var cached)) {
                            dictionary[cached] = filter.Enabled;
                            continue;
                        }

                        var glob = Glob.Parse(globString);
                        this.CachedGlobs[globString] = glob;
                        dictionary[glob] = filter.Enabled;
                    }
                }

                return dictionary;
            }
        }

        internal void Initialise(IDalamudPluginInterface @interface) {
            this.Interface = @interface;
        }

        internal void Save() {
            this.Interface?.SavePluginConfig(this);
        }
    }
}
