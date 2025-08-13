using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PushyFinder.Delivery;
using PushyFinder.Impl;
using PushyFinder.Util;
using PushyFinder.Windows;

namespace PushyFinder;

public sealed class Plugin : IDalamudPlugin
{
    public string Name => "PushyFinder";
    private const string CommandName = "/pushyfinder";

    private IDalamudPluginInterface PluginInterface { get; init; }
    private ICommandManager CommandManager { get; init; }
    private Service Service { get; init; }

    private readonly PartyListener partyListener;
    private readonly DutyListener dutyListener;
    private readonly CharacterUtil characterUtil;
    private readonly CrossWorldPartyListSystem crossWorldPartyListSystem;
    private readonly MasterDelivery masterDelivery;
    private readonly LuminaDataUtil luminaDataUtil;

    public Configuration Configuration { get; private set; }

    public WindowSystem WindowSystem = new("PushyFinder");

    private ConfigWindow ConfigWindow { get; init; }

    public Plugin(
        IDalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IClientState clientState,
        IPartyList partyList,
        IFramework framework,
        IChatGui chatGui,
        IDataManager dataManager,
        IPluginLog pluginLog)
    {
        PluginInterface = pluginInterface;
        CommandManager = commandManager;
        
        Service = new Service(pluginInterface, commandManager, clientState, partyList, framework, chatGui, dataManager, pluginLog);

        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        Configuration.Initialize(PluginInterface);
        
        characterUtil = new CharacterUtil(clientState, Configuration);
        luminaDataUtil = new LuminaDataUtil(dataManager);
        
        masterDelivery = new MasterDelivery(new IDelivery[]
        {
            new PushoverDelivery(Configuration, pluginLog),
            new NtfyDelivery(Configuration, pluginLog),
            new SimplepushDelivery(Configuration, pluginLog),
            new DiscordDelivery(Configuration, pluginLog)
        });

        ConfigWindow = new ConfigWindow(Configuration, masterDelivery, characterUtil);

        WindowSystem.AddWindow(ConfigWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens the configuration window."
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += DrawConfigUI;
        PluginInterface.UiBuilder.OpenConfigUi += DrawConfigUI;

        crossWorldPartyListSystem = new CrossWorldPartyListSystem(framework, clientState);
        partyListener = new PartyListener(pluginLog, characterUtil, crossWorldPartyListSystem, masterDelivery, luminaDataUtil);
        dutyListener = new DutyListener(clientState, pluginLog, characterUtil, masterDelivery, Configuration);

        crossWorldPartyListSystem.Start();
        
        partyListener.On();
        dutyListener.On();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();

        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenMainUi -= DrawConfigUI;
        PluginInterface.UiBuilder.OpenConfigUi -= DrawConfigUI;

        crossWorldPartyListSystem.Stop();
        
        partyListener.Off();
        dutyListener.Off();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        if (args == "debugOnlineStatus")
        {
            this.Service.ChatGui.Print($"OnlineStatus ID = {this.Service.ClientState.LocalPlayer!.OnlineStatus.RowId}");
            return;
        }

        ConfigWindow.IsOpen = true;
    }

    private void DrawUI()
    {
        WindowSystem.Draw();
    }

    public void DrawConfigUI()
    {
        ConfigWindow.IsOpen = true;
    }
}