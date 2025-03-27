using System;
using System.Linq;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using SoundFilter.Resources;

namespace SoundFilter.Ui {
    public class SoundLog {
        private Plugin Plugin { get; }

        private string _search = string.Empty;

        internal SoundLog(Plugin plugin) {
            this.Plugin = plugin;
        }

        internal void Draw() {
            if (!this.Plugin.Config.ShowLog) {
                return;
            }

            ImGui.SetNextWindowSize(new Vector2(500, 450), ImGuiCond.FirstUseEver);

            if (!ImGui.Begin(Language.LogWindowTitle, ref this.Plugin.Config.ShowLog)) {
                ImGui.End();
                return;
            }

            if (ImGui.Checkbox(Language.LogEnableLogging, ref this.Plugin.Config.LogEnabled)) {
                this.Plugin.Config.Save();
            }

            ImGui.SameLine();

            if (ImGui.Checkbox(Language.LogLogFiltered, ref this.Plugin.Config.LogFiltered)) {
                this.Plugin.Config.Save();
            }

            ImGui.InputText(Language.LogSearch, ref this._search, 255);

            var entries = (int) this.Plugin.Config.LogEntries;
            if (ImGui.InputInt(Language.LogMaxRecentSounds, ref entries)) {
                this.Plugin.Config.LogEntries = (uint) Math.Min(10_000, Math.Max(0, entries));
                this.Plugin.Config.Save();
            }

            ImGui.Separator();

            if (ImGui.BeginChild("sounds")) {
                var i = 0;
                foreach (var recent in this.Plugin.Filter.Recent.Reverse()) {
                    if (!string.IsNullOrWhiteSpace(this._search) && !recent.ContainsIgnoreCase(this._search)) {
                        continue;
                    }

                    if (Util.IconButton(FontAwesomeIcon.Copy, $"copy-{recent}-{i}")) {
                        ImGui.SetClipboardText(recent);
                    }

                    ImGui.SameLine();
                    ImGui.TextUnformatted(recent);
                    i += 1;
                }

                ImGui.EndChild();
            }

            // god disabled this frame
            if (!this.Plugin.Config.ShowLog) {
                this.Plugin.Config.Save();
            }

            ImGui.End();
        }
    }
}
