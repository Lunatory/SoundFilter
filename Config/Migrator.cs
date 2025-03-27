using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SoundFilter.Config {
    internal static class Migrator {
        private static void WithEachObject(JToken old, Action<string, JObject> action) {
            foreach (var property in old.Children<JProperty>()) {
                if (property.Name == "$type") {
                    continue;
                }

                var layout = (JObject) property.Value;

                action(property.Name, layout);
            }
        }

        private static void MigrateV1(JObject old) {
            var filters = new List<CustomFilter>();

            WithEachObject(old["Filtered"]!, (glob, filter) => {
                var name = filter["Name"]!.Value<string>()!;
                var enabled = filter["Enabled"]!.Value<bool>();
                filters.Add(new CustomFilter {
                    Name = name,
                    Enabled = enabled,
                    Globs = { glob },
                });
            });

            old.Remove("Filtered");
            old["Filters"] = JArray.FromObject(filters);

            old["Version"] = 2;
        }

        public static Configuration LoadConfiguration(Plugin plugin) {
            var fileInfo = plugin.Interface.ConfigFile;
            var text = fileInfo.Exists
                ? File.ReadAllText(fileInfo.FullName)
                : null;

            if (text == null) {
                goto DefaultConfiguration;
            }

            var config = JsonConvert.DeserializeObject<JObject>(text)!;

            int GetVersion() {
                if (config.TryGetValue("Version", out var token)) {
                    return token.Value<int>();
                }

                return -1;
            }

            var version = GetVersion();
            if (version < 1) {
                goto DefaultConfiguration;
            }

            // run migrations until done
            while (version < Configuration.LatestVersion) {
                switch (version) {
                    case 1:
                        MigrateV1(config);
                        break;
                    default:
                        Plugin.Log.Warning($"Tried to migrate from an unknown version: {version}");
                        goto DefaultConfiguration;
                }

                version = GetVersion();
            }

            if (version == Configuration.LatestVersion) {
                return config.ToObject<Configuration>()!;
            }

            DefaultConfiguration:
            return plugin.Interface.GetPluginConfig() as Configuration ?? new Configuration();
        }
    }
}
