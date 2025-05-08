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

public class ApiService(
    Plugin plugin,
    IDalamudPluginInterface dalamudPluginInterface,
    IPluginLog log)
{
    private async Task PostRequest<T>(T obj, string endpoint)
    {
        var config = plugin.Configuration;
        var baseUrl = config.SystemConfiguration.ApiUrl ?? throw new Exception($"Api URL not set");
        if (!baseUrl.EndsWith('/'))
            baseUrl += "/";
        var url = $"{baseUrl}{endpoint}";
        var client = new HttpClient();
        client.DefaultRequestHeaders.Add($"User-Agent", $"XivForays/{dalamudPluginInterface.Manifest.AssemblyVersion}");
        client.DefaultRequestHeaders.Add($"X-API-Key", config.SystemConfiguration.ApiKey);
        
        log.Debug($"Sending request to {url} with payload {JsonConvert.SerializeObject(obj)}");
        log.Debug($"User-Agent: {client.DefaultRequestHeaders.UserAgent}");
        
        
        var result = await client.PostAsJsonAsync(url, obj);
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
        await PostRequest(fate, $"fateended");
    }

    public async Task UploadEnemyPosition(List<EnemyPosition> enemy)
    {
        await PostRequest(enemy, $"enemyposition");
    }
}
