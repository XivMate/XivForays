using System;
using System.Collections.Generic;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;
using XivMate.DataGathering.Forays.Dalamud.Models;
using XivMate.DataGathering.Forays.Dalamud.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

public class DebugTab(FateModule fateModule, EnemyTrackingService enemyTrackingService) : ITab
{
    private bool Debug;

    private Dictionary<ulong, EnemyPosition>? enemies;

    /// <inheritdoc />
    public int Index => 3;

    /// <inheritdoc />
    public string TabTitle => "Debug";

    /// <inheritdoc />
    public void Draw(Configuration.Configuration configuration)
    {
        ImGui.Text("Show Debug Info");
        ImGui.SameLine();

        if (ImGui.Checkbox("##CrowdsourceData", ref Debug)) { }

        if (Debug)
        {
            enemies = enemyTrackingService.UpdateAndGetEnemies();
            ImGui.Text($"Enemies on map {enemies.Count}");
            RenderEnemiesTable(enemies);
        }
    }

    private void RenderEnemiesTable(Dictionary<ulong, EnemyPosition> enemies)
    {
        using var table = ImRaii.Table("##EnemiesOnMap", 15,
                                       ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable | ImGuiTableFlags.Borders |
                                       ImGuiTableFlags.RowBg);
        if (table)
        {
            // Add table headers
            ImGui.TableSetupColumn("ID");
            ImGui.TableSetupColumn("Type ID");
            ImGui.TableSetupColumn("Name");
            ImGui.TableSetupColumn("Level");
            ImGui.TableSetupColumn("X");
            ImGui.TableSetupColumn("Y");
            ImGui.TableSetupColumn("Z");
            ImGui.TableSetupColumn("Adapted");
            ImGui.TableSetupColumn("Mutated");
            ImGui.TableSetupColumn("Element");
            ImGui.TableSetupColumn("In Combat");
            ImGui.TableSetupColumn("Has Been In Combat");
            ImGui.TableSetupColumn("Territory ID");
            ImGui.TableSetupColumn("Map ID");
            ImGui.TableSetupColumn("Timestamp");
            ImGui.TableHeadersRow();

            // Add table rows
            foreach (var enemyPair in enemies)
            {
                var enemy = enemyPair.Value;
                ImGui.TableNextRow();
                // Column 0: ID
                ImGui.TableNextColumn();
                ImGui.Text(enemyPair.Key.ToString());
                // Column 0: ID
                ImGui.TableNextColumn();
                ImGui.Text(enemy.MobIngameId.ToString());

                // Column 1: Name
                ImGui.TableNextColumn();
                ImGui.Text(enemy.MobName ?? string.Empty);

                // Column 2: Level
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Level.ToString());

                // Column 3: X
                ImGui.TableNextColumn();
                ImGui.Text(enemy.X.ToString("F1"));

                // Column 4: Y
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Y.ToString("F1"));

                // Column 5: Z
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Z.ToString("F1"));

                // Column 6: Adapted
                ImGui.TableNextColumn();
                ImGui.Text(enemy.IsAdapted ? "Yes" : "No");

                // Column 7: Mutated
                ImGui.TableNextColumn();
                ImGui.Text(enemy.IsMutated ? "Yes" : "No");

                // Column 8: Element
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Element ?? string.Empty);

                // Column 9: In Combat
                ImGui.TableNextColumn();
                ImGui.Text(enemy.IsInCombat ? "Yes" : "No");

                // Column 10: Has Been In Combat
                ImGui.TableNextColumn();
                ImGui.Text(enemy.HasBeenInCombat ? "Yes" : "No");

                // Column 11: Territory ID
                ImGui.TableNextColumn();
                ImGui.Text(enemy.TerritoryId.ToString());

                // Column 12: Map ID
                ImGui.TableNextColumn();
                ImGui.Text(enemy.MapId.ToString());

                // Column 13: Timestamp
                ImGui.TableNextColumn();
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(enemy.TimeStamp).DateTime;
                ImGui.Text(dateTime.ToString("HH:mm:ss"));
            }
        }
        //
        // ImGui.EndTable();
    }
}
