using Dalamud.Game;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using SoundFilter.Config;
using SoundFilter.Resources;
using SoundFilter.Ui;

namespace SoundFilter {
    // ReSharper disable once ClassNeverInstantiated.Global
    internal class Plugin : IDalamudPlugin {
        public static string Name => "Sound Filter";

        [PluginService]
        internal static IPluginLog Log { get; private set; } = null!;

        [PluginService]
        internal IDalamudPluginInterface Interface { get; init; } = null!;

        [PluginService]
        internal IChatGui ChatGui { get; init; } = null!;

        [PluginService]
        internal ICommandManager CommandManager { get; init; } = null!;

        [PluginService]
        internal IFramework Framework { get; init; } = null!;

        [PluginService]
        internal ISigScanner SigScanner { get; init; } = null!;

        [PluginService]
        internal IGameInteropProvider GameInteropProvider { get; init; } = null!;

        internal Configuration Config { get; }
        internal Filter Filter { get; }
        internal PluginUi Ui { get; }
        private Commands Commands { get; }

        public Plugin() {
            this.Config = Migrator.LoadConfiguration(this);
            this.Config.Initialise(this.Interface);

            this.Filter = new Filter(this);
            if (this.Config.Enabled) {
                this.Filter.Enable();
            }

            this.Ui = new PluginUi(this);
            this.Commands = new Commands(this);

            if (this.Interface.Reason != PluginLoadReason.Installer) {
                return;
            }

            var message = string.Format(Language.LoadWarning, Name);
            this.ChatGui.Print(new XivChatEntry {
                Name = Name,
                Message = new SeString(
                    new UIForegroundPayload(502),
                    new TextPayload($"[{Name}] {message}"),
                    new UIForegroundPayload(0)
                ),
            });
        }

        public void Dispose() {
            this.Commands.Dispose();
            this.Ui.Dispose();
            this.Filter.Dispose();
        }
    }
}
