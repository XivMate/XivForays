using System;
using Dalamud.Plugin.Services;
using Lumina.Excel.Sheets;

namespace XivMate.DataGathering.Forays.Dalamud.Services;

public class ForayService : IDisposable
{
    private readonly TerritoryService territoryService;
    private readonly Configuration.Configuration configuration;
    private readonly IClientState clientState;
    private readonly IPluginLog log;
    private TerritoryType? lastTerritory;

    public ForayService(
        Plugin plugin,
        TerritoryService territoryService, IClientState clientState, IPluginLog log)
    {
        this.territoryService = territoryService;
        configuration = plugin.Configuration;

        this.clientState = clientState;
        this.log = log;
        clientState.TerritoryChanged += OnTerritoryChanged;
        
        if (clientState.IsLoggedIn)
            this.OnTerritoryChanged(clientState.TerritoryType);
    }

    public bool IsInRecordableTerritory()
    {
        log.Debug($"IsInRecordableTerritory: {clientState.IsLoggedIn} - {lastTerritory?.PlaceName.Value.Name.ExtractText()} - {lastTerritory?.Map.Value.RowId} - {clientState.MapId}");
        return clientState.IsLoggedIn && lastTerritory != null &&
               lastTerritory?.Map.Value.RowId == clientState.MapId;
    }

    private void OnTerritoryChanged(ushort obj)
    {
        lastTerritory = territoryService.GetTerritoryForId(obj);
    }

    public void Dispose()
    {
        territoryService.Dispose();
        clientState.TerritoryChanged -= OnTerritoryChanged;
        lastTerritory = null;
    }
}
