using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Extensions;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

/// <summary>
/// Tab for system configuration settings
/// </summary>
public class SystemTab : ITab
{
    /// <inheritdoc />
    public int Index => 999;

    /// <inheritdoc />
    public string TabTitle => "System";

    /// <inheritdoc />
    public void Draw(Configuration.Configuration configuration)
    {
        var sysConfig = configuration.SystemConfiguration;

        var apiUrl = sysConfig.ApiUrl;
        if (ImGuiHelper.InputText("Api URL:", ref apiUrl))
        {
            sysConfig.ApiUrl = apiUrl;
            configuration.Save();
        }

        var apiKey = sysConfig.ApiKey;
        if (ImGuiHelper.InputText("Api Key:", ref apiKey))
        {
            sysConfig.ApiKey = apiKey;
            configuration.Save();
        }
    }
}
