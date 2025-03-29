using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Utility;
using Flurl.Http;

namespace PushyFinder.Delivery;

public class PushoverDelivery : IDelivery
{
    public static readonly string PushoverApi = "https://api.pushover.net/1/messages.json";

    public bool IsActive => !Plugin.Configuration.PushoverAppKey.IsNullOrWhitespace() &&
                            !Plugin.Configuration.PushoverDevice.IsNullOrWhitespace() &&
                            !Plugin.Configuration.PushoverUserKey.IsNullOrWhitespace();

    public void Deliver(string title, string text)
    {
        Task.Run(() => DeliverAsync(title, text));
    }

    private static async Task DeliverAsync(string title, string text)
    {
        var args = new Dictionary<string, string>
        {
            { "token", Plugin.Configuration.PushoverAppKey },
            { "user", Plugin.Configuration.PushoverUserKey },
            { "device", Plugin.Configuration.PushoverDevice },
            { "title", title },
            { "message", text }
        };

        try
        {
            await PushoverApi.PostJsonAsync(args);
            Service.PluginLog.Debug("Sent Pushover message");
        }
        catch (FlurlHttpException e)
        {
            Service.PluginLog.Error($"Failed to make Pushover request: '{e.Message}'");
            Service.PluginLog.Error($"{e.StackTrace}");
        }
    }
}
