using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Lumina.Excel.Sheets;
using PushyFinder.Delivery;
using PushyFinder.Util;

namespace PushyFinder.Impl;

public class DutyListener
{
    private IClientState ClientState { get; init; }
    private IPluginLog PluginLog { get; init; }
    private CharacterUtil CharacterUtil { get; init; }
    private MasterDelivery MasterDelivery { get; init; }
    private Configuration Configuration { get; init; }

    public DutyListener(IClientState clientState, IPluginLog pluginLog, CharacterUtil characterUtil, MasterDelivery masterDelivery, Configuration configuration)
    {
        ClientState = clientState;
        PluginLog = pluginLog;
        CharacterUtil = characterUtil;
        MasterDelivery = masterDelivery;
        Configuration = configuration;
    }
    
    public void On()
    {
        PluginLog.Debug("DutyListener On");
        ClientState.CfPop += OnDutyPop;
    }

    public void Off()
    {
        PluginLog.Debug("DutyListener Off");
        ClientState.CfPop -= OnDutyPop;
    }

    private void OnDutyPop(ContentFinderCondition e)
    {
        if (!Configuration.EnableForDutyPops)
            return;

        if (!CharacterUtil.IsClientAfk())
            return;

        var dutyName = e.RowId == 0 ? "Duty Roulette" : e.Name.ToDalamudString().TextValue;
        this.MasterDelivery.Deliver("Duty pop", $"Duty registered: '{dutyName}'.");
    }
}