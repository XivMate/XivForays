using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

namespace XivMate.DataGathering.Forays.Dalamud.Windows;

public class ApiKeyWindow : Window, IDisposable
{
    private readonly IPluginLog log;
    private Configuration.Configuration Configuration;

    public ApiKeyWindow(Plugin plugin, IPluginLog log) :
        base("XivForays API Key")
    {
        this.log = log;
        Configuration = plugin.Configuration;
        Size = new Vector2(600, 250);
    }

    public void Dispose() { }

    public override void Draw()
    {
        log.Debug("Drawing ApiKeyWindow");
        ImGui.Text("Api Key");
        var key = Configuration.SystemConfiguration.ApiKey;
        if (ImGui.InputText("##ApiKey", ref key, 100))
        {
            Configuration.SystemConfiguration.ApiKey = key.Trim();
        }
        if (ImGui.Button("Save"))
        {
            Configuration.SystemConfiguration.ApiKey = key;
            log.Information($"Saving API Key: {Configuration.SystemConfiguration.ApiKey}");
            Configuration.Save();
            Toggle();
        }
    }
}
