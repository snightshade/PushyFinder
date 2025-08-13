using System.Collections.Generic;
using System.Linq;
using Dalamud.Memory;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.UI.Info;

namespace PushyFinder.Util;

public class CrossWorldPartyListSystem
{
    public delegate void CrossWorldJoinDelegate(CrossWorldMember m);
    public delegate void CrossWorldLeaveDelegate(CrossWorldMember m);

    private readonly List<CrossWorldMember> members = new();
    private List<CrossWorldMember> oldMembers = new();

    public event CrossWorldJoinDelegate? OnJoin;
    public event CrossWorldLeaveDelegate? OnLeave;

    private readonly IFramework framework;
    private readonly IClientState clientState;

    public CrossWorldPartyListSystem(IFramework framework, IClientState clientState)
    {
        this.framework = framework;
        this.clientState = clientState;
    }

    public void Start()
    {
        framework.Update += Update;
    }

    public void Stop()
    {
        framework.Update -= Update;
    }

    private bool ListContainsMember(List<CrossWorldMember> l, CrossWorldMember m)
        => l.Any(a => a.Name == m.Name);

    private unsafe void Update(IFramework framework)
    {
        if (!clientState.IsLoggedIn)
            return;

        if (!InfoProxyCrossRealm.IsCrossRealmParty())
            return;

        members.Clear();
        var partyCount = InfoProxyCrossRealm.GetPartyMemberCount();
        for (var i = 0u; i < partyCount; i++)
        {
            var addr = InfoProxyCrossRealm.GetGroupMember(i);
            var name = addr->NameString;
            var mObj = new CrossWorldMember
            {
                Name = name,
                PartyCount = partyCount,
                Level = addr->Level,
                JobId = addr->ClassJobId
            };
            members.Add(mObj);
        }

        if (members.Count != oldMembers.Count)
        {
            // Check for joins
            foreach (var i in members)
                if (!ListContainsMember(oldMembers, i))
                {
                    // member joined
                    OnJoin?.Invoke(i);
                }

            // Check for leaves
            foreach (var i in oldMembers)
                if (!ListContainsMember(members, i))
                {
                    // member left
                    OnLeave?.Invoke(i);
                }
        }
        
        // Fix potential broken references caused by memory semantics
        oldMembers = members.ToList();
    }

    public struct CrossWorldMember
    {
        public string Name;
        public int PartyCount;
        public uint Level;
        public uint JobId;
    }
}