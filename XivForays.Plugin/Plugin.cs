﻿using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI;
using Lumina.Excel.Sheets;
using Microsoft.Extensions.DependencyInjection;
using XivMate.DataGathering.Forays.Dalamud.Extensions;
using XivMate.DataGathering.Forays.Dalamud.Gathering;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Enemy;
using XivMate.DataGathering.Forays.Dalamud.Gathering.Fate;
using XivMate.DataGathering.Forays.Dalamud.Services;
using XivMate.DataGathering.Forays.Dalamud.Windows;
using XivMate.DataGathering.Forays.Dalamud.Windows.Tabs;

namespace XivMate.DataGathering.Forays.Dalamud;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService]
    internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;

    [PluginService]
    internal static ITextureProvider TextureProvider { get; private set; } = null!;

    [PluginService]
    internal static ICommandManager CommandManager { get; private set; } = null!;

    [PluginService]
    internal static IClientState ClientState { get; private set; } = null!;

    [PluginService]
    internal static IChatGui ChatGui { get; private set; } = null!;

    [PluginService]
    internal static IGameGui GameGui { get; private set; } = null!;

    [PluginService]
    internal static IDataManager DataManager { get; private set; } = null!;

    [PluginService]
    internal static IPluginLog Log { get; private set; } = null!;

    [PluginService]
    internal static IFateTable FateTable { get; private set; } = null!;

    [PluginService]
    internal static IObjectTable ObjectTable { get; private set; } = null!;

    [PluginService]
    internal static IFramework Framework { get; private set; } = null!;

    [PluginService]
    internal static IAddonEventManager AddonEventManager { get; private set; } = null!;

    [PluginService]
    public static IGameInventory GameInventory { get; private set; } = null;

    private const string CommandName = "/xivforays";

    public Configuration.Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("XivMate");
    private ServiceProvider provider;
    private ConfigWindow ConfigWindow { get; init; }
    private MainWindow MainWindow { get; init; }
    private ApiKeyWindow ApiKeyWindow { get; set; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration.Configuration ??
                        new Configuration.Configuration();
        
        var resourcesPath = PluginInterface.AssemblyLocation.Directory?.FullName!;

        SetupServices();
        InitializeModules();

        MainWindow =
            provider?.GetService<MainWindow>() ??
            throw new InvalidOperationException(); //new MainWindow(this, provider.GetService<TerritoryService>());
        ApiKeyWindow =
            provider.GetService<ApiKeyWindow>() ??
            throw new InvalidOperationException(); //new MainWindow(this, provider.GetService<TerritoryService>());

        RegisterCommands();

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUI;

        var tabs = provider.GetServices<ITab>();
        ConfigWindow = new ConfigWindow(this, tabs, Log);
        ApiKeyWindow = new ApiKeyWindow(this, Log);
        WindowSystem.AddWindow(ConfigWindow);
        WindowSystem.AddWindow(MainWindow);
        WindowSystem.AddWindow(ApiKeyWindow);

        if (string.IsNullOrWhiteSpace(Configuration.SystemConfiguration.ApiKey))
        {
            Log.Info("No API key found, opening API key window");
            if (!ApiKeyWindow.IsOpen)
                ApiKeyWindow.Toggle();
        }
        else
        {
            Log.Info($"API key found: '{Configuration.SystemConfiguration.ApiKey}'");
        }
    }

    private void SetupServices()
    {
        var services = ConfigureServices();
        provider = services.BuildServiceProvider();
    }

    private void InitializeModules()
    {
        var modules = provider.GetServices<IModule>();
        foreach (var module in modules)
        {
            module.LoadConfig(Configuration);
        }
    }

    private void RegisterCommands()
    {
        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open XivForays main window"
        });
    }

    private ServiceCollection ConfigureServices()
    {
        var services = new ServiceCollection();
        services.AddAutoMapper(typeof(Plugin).Assembly);

        services.AddSingleton(this);
        services.AddSingleton(TextureProvider);
        services.AddSingleton(ChatGui);
        services.AddSingleton(PluginInterface);
        services.AddSingleton(CommandManager);
        services.AddSingleton(ClientState);
        services.AddSingleton(DataManager);
        services.AddSingleton(Framework);
        services.AddSingleton(FateTable);
        services.AddSingleton(ObjectTable);
        services.AddSingleton(Log);
        services.AddSingleton(AddonEventManager);
        services.AddSingleton(GameGui);
        services.AddSingleton<FateModule>();
        services.AddSingleton<SchedulerService>();
        services.AddSingleton<TerritoryService>();
        services.AddSingleton<ApiService>();
        services.AddSingleton<EnemyTrackingService>();
        services.AddSingleton<EnemyLocationGatherer>();
        services.AddSingleton<ForayService>();
        services.AddAllTypesImplementing<ITab>();
        services.AddAllTypesImplementing<IModule>();
        services.AddSingleton<MainWindow>();
        services.AddSingleton<ApiKeyWindow>();
        return services;
    }


    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();

        ConfigWindow.Dispose();
        MainWindow.Dispose();

        CommandManager.RemoveHandler(CommandName);
        provider.Dispose();
        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi -= ToggleConfigUI;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUI;
        
    }

    private void OnCommand(string command, string args)
    {
        // in response to the slash command, just toggle the display status of our main ui
        if (args == "config")
            ToggleConfigUI();
        else
            ToggleMainUI();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
    public void ToggleMainUI() => MainWindow.Toggle();

    public void ReloadModules()
    {
        foreach(var module in provider.GetServices<IModule>())
            module.LoadConfig(Configuration);
    }
}
