using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

namespace XivMate.DataGathering.Forays.Dalamud.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly IEnumerable<ITab> tabs;
    private Configuration.Configuration Configuration;

    public ConfigWindow(Plugin plugin, IEnumerable<ITab> tabs, IPluginLog log) : base("XivForays Settings")
    {
        this.tabs = tabs;
        log.Info($"Config Window has {tabs?.Count()} tabs");
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(600, 250);
        SizeCondition = ImGuiCond.Always;

        Configuration = plugin.Configuration;
    }

    public void Dispose() { }

    public override void Draw()
    {
        ImGui.BeginTabBar("#tabs");
        foreach (var tab in tabs)
            tab.DrawTab(Configuration);
        ImGui.EndTabBar();
    }
}
