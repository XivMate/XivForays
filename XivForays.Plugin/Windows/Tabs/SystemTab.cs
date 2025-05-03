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

        // Define a set of characters to trim from the end
        char[] charsToTrim = new char[] {
            '\0', // Null character
            ' ',  // Space
            '\t', // Tab
            '\n', // Newline
            '\r', // Carriage return
            '\u0001', '\u0002', '\u0003', '\u0004', '\u0005', '\u0006', '\u0007',
            '\u0008', '\u000b', '\u000c', '\u000e', '\u000f', '\u0010', '\u0011',
            '\u0012', '\u0013', '\u0014', '\u0015', '\u0016', '\u0017', '\u0018',
            '\u0019', '\u001a', '\u001b', '\u001c', '\u001d', '\u001e', '\u001f',
            '\u007f' // DEL character
        };


        var apiUrl = sysConfig.ApiUrl ?? "";
        if (ImGuiHelper.InputText("Api URL", ref apiUrl, 512U))
        {
            // Trim the defined set of potentially problematic trailing characters
            var cleanedApiUrl = apiUrl.TrimEnd(charsToTrim);

            logger.Warning("Setting API URL to {ApiUrl}", cleanedApiUrl);
            sysConfig.ApiUrl = cleanedApiUrl;
            configuration.Save();
        }

        var apiKey = sysConfig.ApiKey ?? "";
        if (ImGuiHelper.InputText("Api Key", ref apiKey))
        {
            // Trim the same set of characters for the API Key as well
            var cleanedApiKey = apiKey.TrimEnd(charsToTrim);
            sysConfig.ApiKey = cleanedApiKey;
            configuration.Save();
        }
    }
}
