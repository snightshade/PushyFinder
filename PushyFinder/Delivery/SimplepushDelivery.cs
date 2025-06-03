using System;
using System.Threading.Tasks;
using Dalamud.Utility;
using Flurl.Http;

namespace PushyFinder.Delivery;

public class SimplepushDelivery : IDelivery
{
    public bool IsActive =>
        !Plugin.Configuration.SimplepushKey.IsNullOrWhitespace();

    public void Deliver(string title, string text)
    {
        Task.Run(() => DeliverAsync(title, text));
    }

    private static async Task DeliverAsync(string title, string text)
    {
        var key = Plugin.Configuration.SimplepushKey;
        var configuredTitle = Plugin.Configuration.SimplepushTitle;
        var eventParam = Plugin.Configuration.SimplepushEvent;

        if (key.IsNullOrWhitespace())
            return;

        // Use passed-in title if available, otherwise fallback to configured title
        var finalTitle = string.IsNullOrWhiteSpace(title) ? configuredTitle ?? "FFXIV Notification" : title;

        try
        {
            IFlurlRequest request = new FlurlRequest("https://api.simplepush.io/send")
                .SetQueryParam("key", key)
                .SetQueryParam("title", finalTitle)
                .SetQueryParam("msg", text ?? "");

            if (!eventParam.IsNullOrWhitespace())
                request = request.SetQueryParam("event", eventParam);

            await request.GetAsync();
            Service.PluginLog.Debug("Sent Simplepush message");
        }
        catch (FlurlHttpException e)
        {
            Service.PluginLog.Error($"Failed to send Simplepush message: '{e.Message}'");
            Service.PluginLog.Error($"{e.StackTrace}");
        }
        catch (ArgumentException e)
        {
            Service.PluginLog.Error($"Invalid argument: {e.Message}");
            Service.PluginLog.Error($"{e.StackTrace}");
        }
    }
}