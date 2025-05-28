using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Interface.Utility.Raii;
using ImGuiNET;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using FFXIVClientStructs.FFXIV.Client.UI;
using XivMate.DataGathering.Forays.Dalamud.Models;
using XivMate.DataGathering.Forays.Dalamud.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

public class DebugTab(
    FateModule fateModule,
    EnemyTrackingService enemyTrackingService,
    IGameGui gameGui,
    IPluginLog log,
    IGameInventory inventory) : ITab
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

            RenderTest();

            ImGui.Text($"Fates on map {fateModule.ActiveFates.Count()}");
            RenderFateTable();

            ImGui.Text($"Enemies on map {enemies.Count}");
            RenderEnemiesTable(enemies);
        }
    }

    private void RenderFateTable()
    {
        using var table = ImRaii.Table("##FatesOnMap", 8,
                                       ImGuiTableFlags.Resizable | ImGuiTableFlags.Sortable | ImGuiTableFlags.Borders |
                                       ImGuiTableFlags.RowBg);
        if (table)
        {
            // Add table headers
            ImGui.TableSetupColumn("Fate ID");
            ImGui.TableSetupColumn("Level");
            ImGui.TableSetupColumn("Position X");
            ImGui.TableSetupColumn("Position Y");
            ImGui.TableSetupColumn("Position Z");
            ImGui.TableSetupColumn("Territory ID");
            ImGui.TableSetupColumn("Map ID");
            ImGui.TableSetupColumn("Last Updated");
            ImGui.TableHeadersRow();

            // Add table rows
            foreach (var fatePair in fateModule.ActiveFates)
            {
                var fate = fatePair!;
                ImGui.TableNextRow();
                // Column 0: ID
                ImGui.TableNextColumn();
                ImGui.Text(fate.FateId.ToString());
                // Column 1: Type ID
                ImGui.TableNextColumn();
                ImGui.Text(fate.LevelId.ToString());

                // Column 4: X
                ImGui.TableNextColumn();
                ImGui.Text(fate.X.ToString("F1"));

                // Column 5: Y
                ImGui.TableNextColumn();
                ImGui.Text(fate.Y.ToString("F1"));

                // Column 6: Z
                ImGui.TableNextColumn();
                ImGui.Text(fate.Z.ToString("F1"));

                // Column 7: Adapted
                ImGui.TableNextColumn();
                ImGui.Text(fate.Radius+"");

                // Column 8: Mutated
                ImGui.TableNextColumn();
                ImGui.Text(fate.EndedAt.ToString());

                // Column 12: Territory ID
                ImGui.TableNextColumn();
                ImGui.Text(fate.TerritoryId.ToString());

                // Column 13: Map ID
                ImGui.TableNextColumn();
                ImGui.Text(fate.MapId.ToString());

                // // Column 14: Timestamp
                // ImGui.TableNextColumn();
                // var dateTime = DateTimeOffset.FromUnixTimeSeconds(fate.TimeStamp).DateTime;
                // ImGui.Text(dateTime.ToString("HH:mm:ss"));
            }
        }
    }

    private void RenderTest()
    {
        unsafe
        {
            //Get Addon
            var addon = (AddonFateReward*)gameGui.GetAddonByName("FateReward", 1);
            if (addon == null)
            {
                ImGui.Text("Addon is null");
                return;
            }

            //Get Fate
            var unitBase = addon->AtkUnitBase;
            if (!unitBase.IsVisible)
            {
                ImGui.Text("UnitBase is not visible");
                return;
            }

            ImGui.Text(
                $"UnitBase is visible - Atk values: {unitBase.AtkValues->ToString()}, NameString: {unitBase.NameString}");
            var exp = addon->AtkValues[8].String.ExtractText();
            ImGui.Text($"Exp: {exp}");

            log.Debug($"Fate Window: {unitBase.AtkValues->ToString()}");
            log.Debug($"Fate Window: {unitBase.NameString}");
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
                // Column 1: Type ID
                ImGui.TableNextColumn();
                ImGui.Text(enemy.MobIngameId.ToString());

                // Column 2: Name
                ImGui.TableNextColumn();
                ImGui.Text(enemy.MobName ?? string.Empty);

                // Column 3: Level
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Level.ToString());

                // Column 4: X
                ImGui.TableNextColumn();
                ImGui.Text(enemy.X.ToString("F1"));

                // Column 5: Y
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Y.ToString("F1"));

                // Column 6: Z
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Z.ToString("F1"));

                // Column 7: Adapted
                ImGui.TableNextColumn();
                ImGui.Text(enemy.IsAdapted ? "Yes" : "No");

                // Column 8: Mutated
                ImGui.TableNextColumn();
                ImGui.Text(enemy.IsMutated ? "Yes" : "No");

                // Column 9: Element
                ImGui.TableNextColumn();
                ImGui.Text(enemy.Element ?? string.Empty);

                // Column 10: In Combat
                ImGui.TableNextColumn();
                ImGui.Text(enemy.IsInCombat ? "Yes" : "No");

                // Column 11: Has Been In Combat
                ImGui.TableNextColumn();
                ImGui.Text(enemy.HasBeenInCombat ? "Yes" : "No");

                // Column 12: Territory ID
                ImGui.TableNextColumn();
                ImGui.Text(enemy.TerritoryId.ToString());

                // Column 13: Map ID
                ImGui.TableNextColumn();
                ImGui.Text(enemy.MapId.ToString());

                // Column 14: Timestamp
                ImGui.TableNextColumn();
                var dateTime = DateTimeOffset.FromUnixTimeSeconds(enemy.TimeStamp).DateTime;
                ImGui.Text(dateTime.ToString("HH:mm:ss"));
            }
        }
    }
}
