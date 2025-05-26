using System;
using System.Collections.Generic;
using System.Linq;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Game.Inventory;
using Dalamud.Game.Inventory.InventoryEventArgTypes;
using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using Newtonsoft.Json;
using XivMate.DataGathering.Forays.Dalamud.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;

public class RandomTestingModule(
    IClientState clientState,
    IFateTable fateTable,
    IFramework framework,
    SchedulerService schedulerService,
    TerritoryService territoryService,
    IPluginLog log,
    ApiService apiService,
    IGameGui gameGui,
    IGameInventory gameInventory,
    IDataManager dataManager,
    IChatGui chatGui)
    : IModule
{
    public void Dispose()
    {
        schedulerService.Dispose();
        territoryService.Dispose();
        chatGui.CheckMessageHandled -= OnHandleMessage;
        gameInventory.InventoryChanged -= OnGameInventoryOnInventoryChanged;
    }

    public bool Enabled { get; } = true;


    void OnGameInventoryOnInventoryChanged(IReadOnlyCollection<InventoryEventArgs> events)
    {
        unsafe
        {
            log.Info($"----------------------------------------");
            log.Info(
                $"Inventory changed: {string.Join(", ", events.Select(p => $"[{p.Item.ItemId},{p.Item.Quantity}]"))}");
            LogEndedFates();
            foreach (var item in events)
            {
                try
                {
                    var inventory = gameInventory.GetInventoryItems(item.Item.ContainerType);
                    if (item.Item.InventorySlot <= inventory.Length)
                    {
                        var specificItem = inventory[(int)item.Item.InventorySlot];
                        log.Debug(
                            $"[IC-I-Loop]Item: {specificItem}, {specificItem.ToString()}, {specificItem.Quantity}, {specificItem.InventorySlot}");
                        continue;
                    }

                    var baseItem = dataManager.GetExcelSheet<Item>()?.GetRow(item.Item.ItemId);
                    log.Debug($"[IC]Item: {item}, {item.Type.ToString()}, {item.Item.Quantity}, {baseItem?.Name}");
                }
                catch (Exception e)
                {
                    log.Info($"[IC]Error getting item: {e}");
                }
            }

            var addon = (AddonFateReward*)gameGui.GetAddonByName("FateReward");
            if (addon == null)
            {
                log.Info("Index 1 is null");
                addon = (AddonFateReward*)gameGui.GetAddonByName("FateReward", 0);
                if (addon != null)
                {
                    log.Info("Index 0 is not null");
                }
            }

            if (addon == null)
            {
                log.Info("[IC]Addon is null");
                return;
            }


            //Get Fate
            var unitBase = addon->AtkUnitBase;
            if (!unitBase.IsVisible)
            {
                log.Info("[IC]UnitBase is not visible");
                return;
            }

            log.Info(
                $"[IC]UnitBase is visible - Atk values: {unitBase.AtkValues->ToString()}, NameString: {unitBase.NameString}");
            log.Info($"----------------------------------------");
        }
    }

    public void LoadConfig(Configuration.Configuration configuration)
    {
        chatGui.CheckMessageHandled += OnHandleMessage;

        gameInventory.InventoryChanged += OnGameInventoryOnInventoryChanged;
    }

    private void OnHandleMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool ishandled)
    {
        if (message.TextValue.Contains("EXP chain") ||
            message.TextValue.Contains("You obtain") ||
            message.TextValue.Contains("You were unable to obtain") ||
            message.TextValue.Contains("Unable to obtain") ||
            message.TextValue.ToLower().Contains("defeats the") ||
            message.TextValue.ToLower().Contains("you defeat the"))
        {
            log.Info($"----------------------------------------");
            log.Info("OnHandleMessage");
            log.Information(
                $"Type: {type},, sender: {sender.TextValue}, Message: {message.TextValue}, is handled: {ishandled}, timestamp: {timestamp}");

            foreach (var payload in message.Payloads)
            {
                log.Info($"[Random] Payload: {payload.Type} - {payload.Dirty} - {payload}");
            }

            LogEndedFates();
            log.Info($"----------------------------------------");
        }
    }

    private void LogEndedFates()
    {
        var singleOrDefault = fateTable.Where(p => p.State == FateState.Ended);
        var orDefault = singleOrDefault as IFate[] ?? singleOrDefault.ToArray();
        switch (orDefault.Count())
        {
            case > 1:
                log.Info("[Random-LogEndedFates] Found multiple fates ended");
                return;
            case 0:
                log.Info(
                    $"[Random-LogEndedFates] No fates ended. Options were: {string.Join(", ", fateTable.Select(p => p.Name + " - " + p.State))}");
                return;
            default:
            {
                var fate = orDefault.Single();
                log.Info(
                    $"[Random-LogEndedFates] Fate ended: {fate.Name} - {fate.State}");
                break;
            }
        }
    }
}
