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

public class ApiService(IDalamudPluginInterface dalamudPluginInterface, IPluginLog log)
{
    private async Task PostRequest<T>(T obj, string endpoint)
    {
        var config = dalamudPluginInterface.GetPluginConfig() as Configuration.Configuration;
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
