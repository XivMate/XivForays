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

            // Compare with last snapshot to find significant changes
            if (_lastSnapshot.Count > 0)
            {
                var changedEnemies = DetectSignificantChanges(currentEnemies);

                // Enqueue changed enemies for uploading if there are any
                if (changedEnemies.Any())
                {
                    _enemyQueue.Enqueue(changedEnemies);
                }
            }

            // Save the current snapshot for next comparison
            _lastSnapshot = currentEnemies;

            // Also periodically enqueue combat-active enemies for guaranteed updates
            var combatActive = enemyTrackingService.GetCombatActiveEnemies();
            if (combatActive.Any())
            {
                var cloned = combatActive.Select(e => mapper.Map(e, new EnemyPosition())).ToList();
                _enemyQueue.Enqueue(cloned);
            }
        }
        catch (Exception ex)
        {
            log.Error($"Error in EnemyTick: {ex.Message}");
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
            bool shouldReport = false;

            // Check if the enemy is new or has significant changes
            if (!_lastSnapshot.TryGetValue(id, out var previous))
            {
                // New enemy
                shouldReport = true;
            }
            else
            {
                // Check for status changes
                if (previous.IsAdapted != enemy.IsAdapted ||
                    previous.IsMutated != enemy.IsMutated ||
                    previous.IsInCombat != enemy.IsInCombat ||
                    previous.HasBeenInCombat != enemy.HasBeenInCombat)
                {
                    shouldReport = true;
                }

                // Check for significant position change (more than 5 units)
                var distanceSquared =
                    Math.Pow(previous.X - enemy.X, 2) +
                    Math.Pow(previous.Y - enemy.Y, 2) +
                    Math.Pow(previous.Z - enemy.Z, 2);
                if (distanceSquared > 25) // 5^2 = 25
                {
                    shouldReport = true;
                }
            }

            if (shouldReport)
            {
                // Create a deep copy of the enemy data
                changedEnemies.Add(mapper.Map(enemy, new EnemyPosition()));
            }
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
