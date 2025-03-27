using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface;
using ImGuiNET;
using SoundFilter.Config;
using SoundFilter.Resources;

namespace SoundFilter.Ui {
    internal class AddFilter {
        private Guid Id { get; } = Guid.NewGuid();
        private Plugin Plugin { get; }
        private CustomFilter? Filter { get; }

        private string _filterName = string.Empty;
        private string _newSoundPath = string.Empty;
        private readonly List<string> _soundPaths = new();

        internal AddFilter(Plugin plugin) {
            this.Plugin = plugin;
            this.Filter = null;
        }

        internal AddFilter(Plugin plugin, CustomFilter filter) {
            this.Plugin = plugin;
            this.Filter = filter;

            this._filterName = filter.Name;
            this._soundPaths.AddRange(filter.Globs);
        }

        internal bool Draw() {
            ImGui.TextUnformatted(Language.SettingsAddFilterName);

            ImGui.SetNextItemWidth(-1f);
            ImGui.InputText($"##sound-filter-name-{this.Id}", ref this._filterName, 255);

            ImGui.TextUnformatted(Language.SettingsAddPathToFilter);
            int? toRemove = null;
            for (var i = 0; i < this._soundPaths.Count; i++) {
                var path = this._soundPaths[i];
                SetNextItemWidth();
                if (ImGui.InputText($"##sound-path-edit-{i}-{this.Id}", ref path, 255)) {
                    this._soundPaths[i] = path;
                }

                ImGui.SameLine();

                if (Util.IconButton(FontAwesomeIcon.Trash, $"sound-path-delete-{i}-{this.Id}")) {
                    toRemove = i;
                }
            }

            if (toRemove != null) {
                this._soundPaths.RemoveAt(toRemove.Value);
            }

            SetNextItemWidth();
            ImGui.InputText($"##sound-path-{this.Id}", ref this._newSoundPath, 255);
            ImGui.SameLine();
            if (Util.IconButton(FontAwesomeIcon.Plus, "add") && !string.IsNullOrWhiteSpace(this._newSoundPath)) {
                this._soundPaths.Add(this._newSoundPath);
                this._newSoundPath = string.Empty;
            }

            if (Util.IconButton(FontAwesomeIcon.Save, $"save-filter-{this.Id}") && !string.IsNullOrWhiteSpace(this._filterName)) {
                if (!string.IsNullOrWhiteSpace(this._newSoundPath)) {
                    this._soundPaths.Add(this._newSoundPath);
                }

                if (this._soundPaths.Count(sound => !string.IsNullOrWhiteSpace(sound)) > 0) {
                    this.Save();

                    this._filterName = string.Empty;
                    this._newSoundPath = string.Empty;
                    this._soundPaths.Clear();

                    return true;
                }
            }


            if (this.Filter != null) {
                ImGui.SameLine();
                if (Util.IconButton(FontAwesomeIcon.Ban, $"cancel-filter-{this.Id}")) {
                    return true;
                }
            }

            return false;
        }

        private static void SetNextItemWidth() {
            ImGui.PushFont(UiBuilder.IconFont);
            ImGui.SetNextItemWidth(ImGui.GetContentRegionAvail().X - ImGui.CalcTextSize(FontAwesomeIcon.Ban.ToIconString()).X - ImGui.GetStyle().ItemSpacing.X * 2);
            ImGui.PopFont();
        }

        private void Save() {
            this._soundPaths.RemoveAll(string.IsNullOrWhiteSpace);

            if (this.Filter != null) {
                this.Filter.Name = this._filterName;
                this.Filter.Globs.Clear();
                this.Filter.Globs.AddRange(this._soundPaths);
            } else {
                this.Plugin.Config.Filters.Add(new CustomFilter {
                    Name = this._filterName,
                    Enabled = true,
                    Globs = this._soundPaths.ToList(),
                });
            }

            this.Plugin.Config.Save();
        }
    }
}
