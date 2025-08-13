using Dalamud.Plugin.Services;
using PushyFinder.Delivery;
using PushyFinder.Util;

namespace PushyFinder.Impl;

public class PartyListener
{
    private IPluginLog PluginLog { get; init; }
    private CharacterUtil CharacterUtil { get; init; }
    private CrossWorldPartyListSystem CrossWorldPartyListSystem { get; init; }
    private MasterDelivery MasterDelivery { get; init; }
    private LuminaDataUtil LuminaDataUtil { get; init; }

    public PartyListener(IPluginLog pluginLog, CharacterUtil characterUtil, CrossWorldPartyListSystem crossWorldPartyListSystem, MasterDelivery masterDelivery, LuminaDataUtil luminaDataUtil)
    {
        PluginLog = pluginLog;
        CharacterUtil = characterUtil;
        CrossWorldPartyListSystem = crossWorldPartyListSystem;
        MasterDelivery = masterDelivery;
        LuminaDataUtil = luminaDataUtil;
    }
    
    public void On()
    {
        PluginLog.Debug("PartyListener On");
        CrossWorldPartyListSystem.OnJoin += OnJoin;
        CrossWorldPartyListSystem.OnLeave += OnLeave;
    }

    public void Off()
    {
        PluginLog.Debug("PartyListener Off");
        CrossWorldPartyListSystem.OnJoin -= OnJoin;
        CrossWorldPartyListSystem.OnLeave -= OnLeave;
    }

    private void OnJoin(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;

        var jobAbbr = this.LuminaDataUtil.GetJobAbbreviation(m.JobId);

        if (m.PartyCount == 8)
        {
            this.MasterDelivery.Deliver("Party full",
                                   $"{m.Name} (Lv{m.Level} {jobAbbr}) joins the party.\nParty recruitment ended. All spots have been filled.");
        }
        else
        {
            this.MasterDelivery.Deliver($"{m.PartyCount}/8: Party join",
                                   $"{m.Name} (Lv{m.Level} {jobAbbr}) joins the party.");
        }
    }

    private void OnLeave(CrossWorldPartyListSystem.CrossWorldMember m)
    {
        if (!CharacterUtil.IsClientAfk()) return;

        var jobAbbr = this.LuminaDataUtil.GetJobAbbreviation(m.JobId);

        this.MasterDelivery.Deliver($"{m.PartyCount - 1}/8: Party leave",
                               $"{m.Name} (Lv{m.Level} {jobAbbr}) has left the party.");
    }
}