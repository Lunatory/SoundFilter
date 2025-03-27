using System;
using System.Linq;
using Dalamud.Game.Command;
using SoundFilter.Resources;

namespace SoundFilter {
    internal class Commands : IDisposable {
        private const string Name = "/soundfilter";

        private Plugin Plugin { get; }

        public Commands(Plugin plugin) {
            this.Plugin = plugin;

            this.Plugin.CommandManager.AddHandler(Name, new CommandInfo(this.OnCommand) {
                HelpMessage = $"Toggle the {Plugin.Name} config",
            });
        }

        public void Dispose() {
            this.Plugin.CommandManager.RemoveHandler(Name);
        }

        private void OnCommand(string command, string args) {
            if (string.IsNullOrWhiteSpace(args)) {
                this.Plugin.Ui.Settings.Toggle();
                return;
            }

            var chat = this.Plugin.ChatGui;

            var split = args.Split(' ');
            if (split.Length < 1) {
                chat.PrintError($"[{Plugin.Name}] {Language.CommandNotEnoughArguments}");
                chat.PrintError($"[{Plugin.Name}] /soundfilter log");
                chat.PrintError($"[{Plugin.Name}] /soundfilter <enable|disable|toggle> [filter name]");
                return;
            }

            if (split[0] == "log") {
                this.Plugin.Config.ShowLog ^= true;
                this.Plugin.Config.Save();
                return;
            }

            var filterName = split.Length > 1 ? string.Join(" ", split.Skip(1)) : null;
            var filter = filterName == null ? null : this.Plugin.Config.Filters.FirstOrDefault(filter => filter.Name == filterName);
            if (filterName != null && filter == null) {
                chat.PrintError($"[{Plugin.Name}] {Language.CommandNoSuchFilter}");
                return;
            }

            bool? enabled = split[0] switch {
                "enable" => true,
                "disable" => false,
                "toggle" when filter == null => !this.Plugin.Config.Enabled,
                "toggle" => !filter.Enabled,
                _ => null,
            };
            if (enabled == null) {
                chat.PrintError($"[{Plugin.Name}] {Language.CommandInvalidSubcommand}");
                return;
            }

            if (filter != null) {
                filter.Enabled = enabled.Value;
            } else {
                switch (this.Plugin.Config.Enabled) {
                    case true when !enabled.Value:
                        this.Plugin.Filter.Disable();
                        break;
                    case false when enabled.Value:
                        this.Plugin.Filter.Enable();
                        break;
                }

                this.Plugin.Config.Enabled = enabled.Value;
            }

            this.Plugin.Config.Save();
        }
    }
}
