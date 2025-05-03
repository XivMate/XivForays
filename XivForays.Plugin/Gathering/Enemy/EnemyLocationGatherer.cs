using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dalamud.Plugin.Services;
using XivMate.DataGathering.Forays.Dalamud.Models;
using XivMate.DataGathering.Forays.Dalamud.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Gathering.Enemy;

public class EnemyLocationGatherer(
    EnemyTrackingService enemyTrackingService,
    SchedulerService schedulerService,
    IPluginLog log,
    ApiService apiService,
    IMapper mapper)
    : IModule
{
    private readonly ConcurrentQueue<List<EnemyPosition>> _enemyQueue = new();
    private Dictionary<ulong, EnemyPosition> _lastSnapshot = new();

    private bool _enabled = false;
    public bool Enabled => _enabled;

    /// <summary>
    /// Loads configuration and enables/disables the module accordingly
    /// </summary>
    public void LoadConfig(Configuration.Configuration configuration)
    {
        if (configuration.CanCrowdsourceData && !_enabled)
        {
            schedulerService.ScheduleOnFrameworkThread(EnemyTick, 5000);
            schedulerService.ScheduleOnNewThread(EnemyUpload, 3000);
            _enabled = true;
            _lastSnapshot.Clear();
        }
        else if (_enabled && !configuration.CanCrowdsourceData)
        {
            schedulerService.CancelScheduledTask(EnemyTick);
            schedulerService.CancelScheduledTask(EnemyUpload);
            _enabled = false;
        }
    }

    /// <summary>
    /// Processes enemy information on each tick
    /// </summary>
    private void EnemyTick()
    {
        if (!enemyTrackingService.IsInRecordableTerritory())
        {
            return;
        }

        try
        {
            // Get latest enemy data from the tracking service
            var currentEnemies = enemyTrackingService.UpdateAndGetEnemies();
            if (currentEnemies.Any())
            {
                _enemyQueue.Enqueue(currentEnemies.Select(e => mapper.Map(e.Value, new EnemyPosition())).ToList());
            }
        }catch (Exception ex)
        {
            log.Warning($"Error processing enemy data: {ex.Message}");
        }
    }

    /// <summary>
    /// Detects significant changes in enemy data that should be reported
    /// </summary>
    private List<EnemyPosition> DetectSignificantChanges(Dictionary<ulong, EnemyPosition> currentEnemies)
    {
        var changedEnemies = new List<EnemyPosition>();

        foreach (var (id, enemy) in currentEnemies)
        {
            // Create a deep copy of the enemy data
            changedEnemies.Add(mapper.Map(enemy, new EnemyPosition()));
        }

        return changedEnemies;
    }

    /// <summary>
    /// Uploads enemies from the queue to the API
    /// </summary>
    private void EnemyUpload()
    {
        if (_enemyQueue.TryDequeue(out var enemies))
        {
            try
            {
                apiService.UploadEnemyPosition(enemies).GetAwaiter().GetResult();
                log.Debug($"Successfully uploaded {enemies.Count} enemy positions");
            }
            catch (Exception ex)
            {
                // Re-queue the data if upload fails (up to a limit)
                _enemyQueue.Enqueue(enemies);
                log.Warning($"Error uploading enemy data ({enemies.Count} entries): {ex.Message}");
            }
        }

        // Prevent queue from growing too large
        if (_enemyQueue.Count > 10)
        {
            log.Debug($"Clearing enemy queue, size exceeds limit");
            _enemyQueue.Clear();
        }
    }

    /// <summary>
    /// Disposes resources used by the module
    /// </summary>
    public void Dispose()
    {
        schedulerService.CancelScheduledTask(EnemyTick);
        schedulerService.CancelScheduledTask(EnemyUpload);
        GC.SuppressFinalize(this);
    }
}
