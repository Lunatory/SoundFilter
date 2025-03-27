using System;
using System.Collections.Generic;

namespace SoundFilter.Config {
    [Serializable]
    internal class CustomFilter {
        public string Name = "Unnamed filter";
        public bool Enabled = true;
        public List<string> Globs { get; set; } = new();
    }
}
