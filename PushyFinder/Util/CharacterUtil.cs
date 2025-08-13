using Dalamud.Plugin.Services;

namespace PushyFinder.Util;

public static class CharacterUtil // Make the class static
{
    private static IClientState ClientState { get; set; } = null!;

    // Change the constructor to a static Initialize method
    public static void Initialize(IClientState clientState)
    {
        ClientState = clientState;
    }
    
    public static bool IsClientAfk()
    {
        if (Plugin.Configuration.IgnoreAfkStatus)
            return true;

        if (!ClientState.IsLoggedIn ||
            ClientState.LocalPlayer == null)
            return false;
        // 17 = AFK, 18 = Camera Mode (should catch idle camera. also has the effect of catching gpose!)
        // update 26/11/2024 (patch 7.11): the above IDs are still correct - not sure how long it'll persist
        // considering the game likes to change these every once in a while
        return ClientState.LocalPlayer.OnlineStatus.RowId is 17 or 18;
    }
}