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

    public DutyListener(IClientState clientState, IPluginLog pluginLog)
    {
        ClientState = clientState;
        PluginLog = pluginLog;
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
        if (!Plugin.Configuration.EnableForDutyPops)
            return;

        if (!CharacterUtil.IsClientAfk())
            return;

        var dutyName = e.RowId == 0 ? "Duty Roulette" : e.Name.ToDalamudString().TextValue;
        MasterDelivery.Deliver("Duty pop", $"Duty registered: '{dutyName}'.");
    }
}
