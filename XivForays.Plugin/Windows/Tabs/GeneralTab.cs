using System.Linq;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Enemy;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

/// <summary>
/// Tab for FATE tracking configuration and status display
/// </summary>
public class GeneralTab(FateModule fateModule, EnemyLocationGatherer enemyLocationGatherer) : ITab
{
    /// <inheritdoc />
    public int Index => 1;
    
    /// <inheritdoc />
    public string TabTitle => "General";

    /// <inheritdoc />
    public void Draw(Configuration.Configuration configuration)
    {
        ImGui.Text("Crowdsource data");
        ImGui.SameLine();
        
        var canCrowdsourceData = configuration.CanCrowdsourceData;
        if (ImGui.Checkbox("##CrowdsourceData", ref canCrowdsourceData))
        {
            configuration.CanCrowdsourceData = canCrowdsourceData;
            configuration.Save();
            fateModule.LoadConfig(configuration);
            enemyLocationGatherer.LoadConfig(configuration);
        }

        if (!configuration.CanCrowdsourceData)
        {
            return;
        }

        var fateCount = fateModule.ActiveFates.Count();
        ImGui.Text($"Fates on map: {fateCount}##{fateCount}");
    }
}
