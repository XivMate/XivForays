using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Newtonsoft.Json;
using XivMate.DataGathering.Forays.Dalamud.Models;

namespace XivMate.DataGathering.Forays.Dalamud.Services;

public class ApiService : IDisposable
{
    private readonly Plugin plugin;
    private readonly IDalamudPluginInterface dalamudPluginInterface;
    private readonly IPluginLog log;
    private readonly HttpClient httpClient;
    private bool _disposed = false;

    public ApiService(
        Plugin plugin,
        IDalamudPluginInterface dalamudPluginInterface,
        IPluginLog log)
    {
        this.plugin = plugin;
        this.dalamudPluginInterface = dalamudPluginInterface;
        this.log = log;
        this.httpClient = new HttpClient();
        // Configure HttpClient instance here if needed (e.g., BaseAddress, DefaultRequestHeaders)
        // For User-Agent and X-API-Key, it's better to set them per-request if the API key can change,
        // or if different requests need different headers.
        // If they are static for the lifetime of ApiService, they can be set here.
        // httpClient.DefaultRequestHeaders.Add("User-Agent", $"XivForays/{dalamudPluginInterface.Manifest.AssemblyVersion}");
        // The API key might change, so it's safer to add it per request or ensure config is re-read if it can change.
    }

    private async Task PostRequest<T>(T obj, string endpoint)
    {
        var config = plugin.Configuration;
        var baseUrl = config.SystemConfiguration.ApiUrl ?? throw new Exception("Api URL not set");
        if (!baseUrl.EndsWith('/'))
            baseUrl += "/";
        var url = $"{baseUrl}{endpoint}";

        // Use a HttpRequestMessage to set headers per request if they can vary
        // or if the API key needs to be fetched fresh each time.
        using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
        requestMessage.Headers.Add("User-Agent", $"XivForays/{dalamudPluginInterface.Manifest.AssemblyVersion}");
        requestMessage.Headers.Add("X-API-Key", config.SystemConfiguration.ApiKey);
        requestMessage.Content = JsonContent.Create(obj);

        log.Debug($"Sending request to {url} with payload {JsonConvert.SerializeObject(obj)}");
        log.Debug($"User-Agent: {requestMessage.Headers.UserAgent}");

        var result = await httpClient.SendAsync(requestMessage);
        if (result.StatusCode == HttpStatusCode.Unauthorized)
        {
            log.Error($"Unauthorized request to {url}");
            throw new UnauthorizedAccessException("Unauthorized request to API");
        }
        else if (result.StatusCode == HttpStatusCode.Forbidden)
        {
            log.Error($"Forbidden request to {url}");
            throw new UnauthorizedAccessException("Forbidden request to API");
        }
        else if (result.StatusCode != HttpStatusCode.OK)
        {
            log.Error($"Error {result.StatusCode} on request to {url}");
            throw new Exception($"Error {result.StatusCode} on request to API");
        }
        result.EnsureSuccessStatusCode();
    }

    public async Task UploadFate(Fate fate)
    {
        await PostRequest(fate, "fateended");
    }

    public async Task UploadEnemyPosition(List<EnemyPosition> enemy)
    {
        await PostRequest(enemy, "enemyposition");
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
            httpClient?.Dispose();
        }

        _disposed = true;
    }
}
