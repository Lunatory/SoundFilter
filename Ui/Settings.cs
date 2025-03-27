using System;
using System.Numerics;
using Dalamud.Interface;
using ImGuiNET;
using SoundFilter.Resources;

namespace SoundFilter.Ui {
    public class Settings : IDisposable {
        private Plugin Plugin { get; }
        private AddFilter AddFilter { get; }
        private AddFilter? EditFilter { get; set; }

        private bool _showWindow;
        private int _dragging = -1;

        internal Settings(Plugin plugin) {
            this.Plugin = plugin;
            this.AddFilter = new AddFilter(plugin);

            this.Plugin.Interface.UiBuilder.OpenConfigUi += this.Toggle;
        }

        public void Dispose() {
            this.Plugin.Interface.UiBuilder.OpenConfigUi -= this.Toggle;
        }

        internal void Toggle() {
            this._showWindow = !this._showWindow;
        }

        internal void Draw() {
            if (!this._showWindow) {
                return;
            }

            if (this.EditFilter != null) {
                ImGui.SetNextWindowSize(new Vector2(ImGui.GetWindowSize().X, -1));
                if (ImGui.BeginPopupModal($"{Language.SettingsEditFilter}###edit-filter-modal")) {
                    if (this.EditFilter.Draw()) {
                        this.EditFilter = null;
                    }

                    ImGui.EndPopup();
                }

                ImGui.OpenPopup("###edit-filter-modal");
            }

            ImGui.SetNextWindowSize(new Vector2(500, 450), ImGuiCond.FirstUseEver);

            var windowTitle = string.Format(Language.SettingsWindowTitle, Plugin.Name);
            if (!ImGui.Begin($"{windowTitle}###soundfilter-settings", ref this._showWindow)) {
                ImGui.End();
                return;
            }

            var shouldSave = false;
            if (ImGui.Checkbox(Language.SettingsEnableSoundFilter, ref this.Plugin.Config.Enabled)) {
                if (this.Plugin.Config.Enabled) {
                    this.Plugin.Filter.Enable();
                } else {
                    this.Plugin.Filter.Disable();
                }

                shouldSave = true;
            }

            shouldSave |= ImGui.Checkbox(Language.SettingsShowSoundLogWindow, ref this.Plugin.Config.ShowLog);

            ImGui.Separator();

            if (ImGui.CollapsingHeader(Language.SettingsAddFilter)) {
                this.AddFilter.Draw();
            }

            ImGui.Separator();

            if (ImGui.BeginChild("filtered-sounds")) {
                int? toRemove = null;
                (int src, int dst)? drag = null;
                for (var i = 0; i < this.Plugin.Config.Filters.Count; i++) {
                    var filter = this.Plugin.Config.Filters[i];

                    if (Util.IconButton(FontAwesomeIcon.Trash, $"delete-filter-{i}")) {
                        toRemove = i;
                        shouldSave = true;
                    }

                    ImGui.SameLine();
                    if (Util.IconButton(FontAwesomeIcon.PencilAlt, $"edit-filter-{i}")) {
                        this.EditFilter = new AddFilter(this.Plugin, filter);
                    }

                    ImGui.SameLine();

                    if (Util.IconButton(FontAwesomeIcon.Copy, $"copy-filter-{i}")) {
                        ImGui.SetClipboardText(string.Join("\n", filter.Globs));
                    }

                    ImGui.SameLine();

                    shouldSave |= ImGui.Checkbox($"{filter.Name}##{i}", ref filter.Enabled);

                    if (ImGui.IsItemActive() || this._dragging == i) {
                        this._dragging = i;
                        var step = 0;
                        if (ImGui.GetIO().MouseDelta.Y < 0 && ImGui.GetMousePos().Y < ImGui.GetItemRectMin().Y) {
                            step = -1;
                        }

                        if (ImGui.GetIO().MouseDelta.Y > 0 && ImGui.GetMousePos().Y > ImGui.GetItemRectMax().Y) {
                            step = 1;
                        }

                        if (step != 0) {
                            drag = (i, i + step);
                        }
                    }

                    if (!ImGui.IsItemHovered()) {
                        continue;
                    }

                    ImGui.BeginTooltip();
                    foreach (var glob in filter.Globs) {
                        ImGui.TextUnformatted(glob);
                    }

                    ImGui.EndTooltip();
                }

                if (!ImGui.IsMouseDown(ImGuiMouseButton.Left) && this._dragging != -1) {
                    this._dragging = -1;
                    this.Plugin.Config.Save();
                }

                if (drag != null && drag.Value.dst < this.Plugin.Config.Filters.Count && drag.Value.dst >= 0) {
                    this._dragging = drag.Value.dst;
                    // ReSharper disable once SwapViaDeconstruction
                    var temp = this.Plugin.Config.Filters[drag.Value.src];
                    this.Plugin.Config.Filters[drag.Value.src] = this.Plugin.Config.Filters[drag.Value.dst];
                    this.Plugin.Config.Filters[drag.Value.dst] = temp;
                }

                if (toRemove != null) {
                    this.Plugin.Config.Filters.RemoveAt(toRemove.Value);
                }

                ImGui.EndChild();
            }

            if (shouldSave) {
                this.Plugin.Config.Save();
            }

            ImGui.End();
        }
    }
}
