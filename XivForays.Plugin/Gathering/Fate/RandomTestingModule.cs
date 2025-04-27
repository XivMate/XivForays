using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Plugin.Services;
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
    IChatGui chatGui)
    : IModule
{
    public void Dispose()
    {
        schedulerService.Dispose();
        territoryService.Dispose();
        chatGui.CheckMessageHandled -= OnHandleMessage;
    }

    public bool Enabled { get; } = false;

    public void LoadConfig(Configuration.Configuration configuration)
    {
        chatGui.CheckMessageHandled += OnHandleMessage;
    }

    private void OnHandleMessage(
        XivChatType type, int timestamp, ref SeString sender, ref SeString message, ref bool ishandled)
    {
        if (message.TextValue.Contains("EXP chain") || message.TextValue.Contains("You obtain") ||
            message.TextValue.ToLower().Contains("defeats the") ||
            message.TextValue.ToLower().Contains("you defeat the"))
        {
            log.Information(
                $"Type: {type},, sender: {sender.TextValue}, Message: {message.TextValue}, is handled: {ishandled}, timestamp: {timestamp}");
        }
    }
}
