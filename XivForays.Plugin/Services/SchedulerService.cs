using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;

namespace XivMate.DataGathering.Forays.Dalamud.Services;

public class SchedulerService(IFramework framework, IPluginLog log) : IDisposable
{
    private readonly Dictionary<string, ThreadLoopData> threads = new();

    public void ScheduleOnFrameworkThread(Action action, int intervalMs)
    {
        var actionName = ActionToString(action);
        if (threads.ContainsKey(actionName)) throw new Exception("Thread already exists");

        var threadLoop = new ThreadLoop();
        threadLoop.Start(() => { framework.RunOnFrameworkThread(action); }, intervalMs);
        log.Debug("Started threadloop for " + action.Method.Name);
        threads.Add(actionName, new ThreadLoopData()
        {
            ThreadLoop = threadLoop
        });
    }

    public void ScheduleOnNewThread(Action action, int intervalMs)
    {
        var actionName = ActionToString(action);
        if (threads.ContainsKey(actionName)) throw new Exception("Thread already exists");

        var threadLoop = new ThreadLoop();
        threadLoop.Start(action.Invoke, intervalMs);
        log.Debug("Started threadloop for " + action.Method.Name);
        threads.Add(actionName, new ThreadLoopData()
        {
            ThreadLoop = threadLoop
        });
    }

    public void CancelScheduledTask(Action action)
    {
        if (threads.TryGetValue(ActionToString(action), out var loopData))
        {
            loopData.ThreadLoop.Stop();
            threads.Remove(ActionToString(action));
            log.Debug("Stopped threadloop for " + action.Method.Name + ". Remaining threads: " +
                      string.Join(", ", threads.Keys));
        }
        else
        {
            log.Debug("Couldn't find threadloop for: " + action.Method.Name + ". Remaining threads: " +
                      string.Join(", ", threads.Keys));
            foreach (var threadDataPair in threads.ToList()) // ToList() to avoid modification during iteration
            {
                log.Verbose($"- {ActionToString(action)} != {threadDataPair}");
            }
        }
    }

    private string ActionToString(Action action)
    {
        return action.Method.Name;
    }

    public void Dispose()
    {
        log.Debug("Disposing SchedulerService");
        foreach (var threadDataPair in
                 threads.ToList()) // ToList() to avoid modification during iteration if Dispose leads to CancelScheduledTask
        {
            var action = threadDataPair.Key;
            var threadLoopData = threadDataPair.Value;
            log.Debug("Stopping and disposing threadloop for " + action);
            threadLoopData?.ThreadLoop?.Stop();
            threadLoopData?.ThreadLoop?.Dispose(); // Ensure ThreadLoop is disposed
        }

        threads.Clear();
        log.Debug("All threads stopped and disposed");
        GC.SuppressFinalize(this);
    }
}

internal class ThreadLoopData
{
    public required ThreadLoop ThreadLoop { get; init; }
}

internal class ThreadLoop : IDisposable
{
    private Task? task; // Changed to nullable
    private readonly CancellationTokenSource cancellationTokenSource = new();
    private bool _disposed = false; // Add disposed flag

    public void Start(Action action, int interval)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(ThreadLoop));
        task = Task.Factory.StartNew(() =>
        {
            while (!cancellationTokenSource.IsCancellationRequested)
            {
                try
                {
                    if (cancellationTokenSource.IsCancellationRequested)
                        return;
                    action.Invoke();
                    // Use WaitAsync for better cancellation responsiveness
                    // and to avoid blocking if the token is already cancelled.
                    try
                    {
                        cancellationTokenSource.Token.WaitHandle.WaitOne(interval);
                    }
                    catch (OperationCanceledException)
                    {
                        // Expected when cancellation is requested during WaitOne
                        break;
                    }
                }
                catch (Exception ex) // Catch specific exceptions if possible
                {
                    // Log the exception, but continue the loop unless it's fatal
                    // Consider adding logging here: log.Error($"Exception in scheduled task: {ex}");
                    if (cancellationTokenSource.IsCancellationRequested)
                        break;
                    try
                    {
                        cancellationTokenSource.Token.WaitHandle.WaitOne(interval);
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
            }
        }, cancellationTokenSource.Token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
    }


    public void Stop()
    {
        if (!_disposed && !cancellationTokenSource.IsCancellationRequested)
        {
            cancellationTokenSource.Cancel();
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        if (disposing)
        {
            Stop(); // Ensure cancellation is signaled

            try
            {
                task?.Wait(TimeSpan.FromSeconds(5)); // Wait for the task to complete, with a timeout
            }
            catch (AggregateException ae)
            {
                ae.Handle(e => e is TaskCanceledException); // Handle TaskCanceledException
            }
            catch (OperationCanceledException)
            {
                // Expected if the task was cancelled
            }
            catch (ObjectDisposedException)
            {
                // Task might already be disposed if Stop was called and task completed quickly
            }

            task?.Dispose();
            cancellationTokenSource.Dispose();
        }

        _disposed = true;
    }

    // Add a finalizer if ThreadLoop directly owns unmanaged resources,
    // or if it's critical to release CancellationTokenSource even if Dispose isn't called.
    // However, for purely managed resources like Task and CancellationTokenSource,
    // relying on the containing class (SchedulerService) to call Dispose is usually sufficient.
    /*
    ~ThreadLoop()
    {
        Dispose(false);
    }
    */
}
