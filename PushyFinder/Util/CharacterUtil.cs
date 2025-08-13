using Dalamud.Plugin.Services;

namespace PushyFinder.Util;

public class CharacterUtil
{
    private readonly IClientState clientState;
    private readonly Configuration configuration;

    public CharacterUtil(IClientState clientState, Configuration configuration)
    {
        this.clientState = clientState;
        this.configuration = configuration;
    }

    public bool IsClientAfk()
    {
        if (configuration.IgnoreAfkStatus)
            return true;

        if (!clientState.IsLoggedIn ||
            clientState.LocalPlayer == null)
            return false;
        // 17 = AFK, 18 = Camera Mode (should catch idle camera. also has the effect of catching gpose!)
        // update 26/11/2024 (patch 7.11): the above IDs are still correct - not sure how long it'll persist
        // considering the game likes to change these every once in a while
        return clientState.LocalPlayer.OnlineStatus.RowId is 17 or 18;
    }
}