using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Dalamud.Game.ClientState.Fates;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using XivMate.DataGathering.Forays.Dalamud.Extensions;
using XivMate.DataGathering.Forays.Dalamud.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;

public class FateModule(
    IClientState clientState,
    IFateTable fateTable,
    IFramework framework,
    SchedulerService schedulerService,
    TerritoryService territoryService,
    IPluginLog log,
    ApiService apiService)
    : IModule
{
    private bool _isInRecordableTerritory = false;
    private Guid _instanceGuid = Guid.NewGuid();
    private readonly Dictionary<uint, Models.Fate> _fates = new();
    private readonly ConcurrentQueue<Models.Fate> _fateQueue = new();

    private bool _enabled = false;
    public bool Enabled => _enabled;

    public IEnumerable<Models.Fate> ActiveFates => _fates.Values.ToList();

    /// <summary>
    /// Loads configuration and enables/disables the module accordingly
    /// </summary>
    public void LoadConfig(Configuration.Configuration configuration)
    {
        if (configuration.CanCrowdsourceData&& !_enabled)
        {
            clientState.TerritoryChanged += OnTerritoryChanged;
            schedulerService.ScheduleOnFrameworkThread(FateTick, 500);
            schedulerService.ScheduleOnNewThread(FateUpload, 2500);
            OnTerritoryChanged(clientState.TerritoryType);
            _enabled = true;
            _fates.Clear();
        }
        else if (_enabled && !configuration.CanCrowdsourceData)
        {
            clientState.TerritoryChanged -= OnTerritoryChanged;
            schedulerService.CancelScheduledTask(FateTick);
            schedulerService.CancelScheduledTask(FateUpload);
            _enabled = false;
        }
    }

    /// <summary>
    /// Handles territory changes and determines if the current territory is recordable
    /// </summary>
    private void OnTerritoryChanged(ushort territoryId)
    {
        if (!clientState.IsLoggedIn)
        {
            return;
        }

        var territory = territoryService.GetTerritoryForId(territoryId);
        log.Info($"Current territory id is: ", territoryId);
        log.Info("Territory Placename is : ", territory?.PlaceName.ValueNullable);
        if (territory?.PlaceName.ValueNullable.HasValue ?? false)
        {
            string territoryName = territory.Value.PlaceName.Value.Name.ToString();
            bool isForayTerritory = territoryName.Contains("Eureka") ||
                                    territoryName.Contains("Zadnor") ||
                                    territoryName.Contains("Bozjan Southern Front");

            if (isForayTerritory)
            {
                _isInRecordableTerritory = true;
                _instanceGuid = Guid.NewGuid();
                log.Info(
                    $"Foray territory: {territoryName}, local guid: {_instanceGuid}, local territory {clientState.TerritoryType}, map {clientState.MapId}");
            }
            else
            {
                log.Info($"Not Foray territory: {territoryName}");
                _isInRecordableTerritory = false;
            }
        }
        else
        {
            _isInRecordableTerritory = false;
        }
    }

    /// <summary>
    /// Processes fate information on each tick
    /// </summary>
    private void FateTick()
    {
        if (clientState.LocalPlayer == null || !_isInRecordableTerritory)
        {
            return;
        }

        try
        {
            ProcessActiveFates();
            RemoveNonExistentFates();
        }
        catch (Exception ex)
        {
            log.Error($"Error in FateTick: {ex.Message}");
        }
    }

    /// <summary>
    /// Processes active fates from the fate table
    /// </summary>
    private void ProcessActiveFates()
    {
        foreach (var fate in fateTable)
        {
            if (_fates.TryGetValue(fate.FateId, out var modelFate))
            {
                if (fate.State is FateState.Ended or FateState.Failed)
                {
                    RemoveFate(modelFate);
                }
            }
            else if (fate.State is FateState.Running && fate.StartTimeEpoch > 0 && fate.Position != Vector3.Zero)
            {
                // Add new fate
                log.Info(
                    $"New fate found: {fate.Name}, started at: {fate.StartTimeEpoch}, position: {fate.Position}, status: {fate.State}, territory: {fate.TerritoryType.Value.RowId}, map: {clientState.MapId}");

                modelFate = new Models.Fate()
                {
                    Name = fate.Name.ToString(),
                    FateId = fate.FateId,
                    X = fate.Position.X,
                    Y = fate.Position.Y,
                    Z = fate.Position.Z,
                    Radius = fate.Radius,
                    StartedAt = fate.StartTimeEpoch,
                    InstanceId = _instanceGuid,
                    TerritoryId = Convert.ToInt32(fate.TerritoryType.Value.RowId),
                    MapId = (int)clientState.MapId,
                    LevelId = fate.Level
                };

                _fates.Add(fate.FateId, modelFate);
            }
        }
    }

    /// <summary>
    /// Removes fates that no longer exist in the fate table
    /// </summary>
    private void RemoveNonExistentFates()
    {
        var fatesToRemove = _fates.Keys
                                  .Where(fateId => fateTable.All(existingFate => existingFate.FateId != fateId))
                                  .ToList();

        foreach (var fateId in fatesToRemove)
        {
            if (_fates.TryGetValue(fateId, out var removedFate))
            {
                RemoveFate(removedFate);
            }
        }
    }

    /// <summary>
    /// Removes a fate from tracking and adds it to the upload queue
    /// </summary>
    private void RemoveFate(Models.Fate modelFate)
    {
        if (_fates.Remove(modelFate.FateId))
        {
            modelFate.EndedAt = DateTime.UtcNow.ToUnixTime();

            if (modelFate.TerritoryId == clientState.TerritoryType)
            {
                _fateQueue.Enqueue(modelFate);
            }

            log.Info($"Fate ended: {modelFate.Name}, at: {modelFate.EndedAt}");
            log.Info(
                $"#1 - Fate ended territoryid {modelFate.TerritoryId}, client territory: {clientState.TerritoryType}, client map: {clientState.MapId}");
            log.Info(JsonConvert.SerializeObject(modelFate));
        }
    }

    /// <summary>
    /// Uploads fates from the queue to the API
    /// </summary>
    private void FateUpload()
    {
        if (_fateQueue.TryDequeue(out var fate))
        {
            try
            {
                apiService.UploadFate(fate).GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _fateQueue.Enqueue(fate);
                log.Warning($"Error uploading {fate.Name}: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Disposes resources used by the module
    /// </summary>
    public void Dispose()
    {
        clientState.TerritoryChanged -= OnTerritoryChanged;
        schedulerService.CancelScheduledTask(FateTick);
        schedulerService.CancelScheduledTask(FateUpload);
        GC.SuppressFinalize(this);
    }
}
