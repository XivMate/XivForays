using Dalamud.Plugin.Services;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Extensions;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

/// <summary>
/// Tab for system configuration settings
/// </summary>
public class SystemTab(IPluginLog logger) : ITab
{
    /// <inheritdoc />
    public int Index => 999;

    /// <inheritdoc />
    public string TabTitle => "System";

    /// <inheritdoc />
    public void Draw(Configuration.Configuration configuration)
    {
        var sysConfig = configuration.SystemConfiguration;
        ImGui.SetNextItemWidth(300); // Optional: Set a specific width for the input box

        var apiUrl = sysConfig.ApiUrl.ToString();
        if (ImGuiHelper.InputText("Api URL", ref apiUrl, 512U))
        {
            logger.Warning("Setting API URL to {ApiUrl}", apiUrl.ToString());
            sysConfig.ApiUrl = apiUrl;
            configuration.Save();
        }

        var apiKey = sysConfig.ApiKey;
        if (ImGuiHelper.InputText("Api Key", ref apiKey))
        {
            sysConfig.ApiKey = apiKey;
            configuration.Save();
        }
    }
}
