using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace PushyFinder;

public class Service
{
    public IDalamudPluginInterface PluginInterface { get; }
    public ICommandManager CommandManager { get; }
    public IClientState ClientState { get; }
    public IPartyList PartyList { get; }
    public IFramework Framework { get; }
    public IChatGui ChatGui { get; }
    public IDataManager DataManager { get; }
    public IPluginLog PluginLog { get; }

    public Service(
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
        ClientState = clientState;
        PartyList = partyList;
        Framework = framework;
        ChatGui = chatGui;
        DataManager = dataManager;
        PluginLog = pluginLog;
    }
}