using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Utility;
using Flurl.Http;

namespace PushyFinder.Delivery;

public class SimplepushDelivery : IDelivery
{
    public static readonly string SimplepushApi = "https://api.simplepush.io/send";

    public bool IsActive => !Plugin.Configuration.SimplepushKey.IsNullOrWhitespace();

    public void Deliver(string title, string text)
    {
        Task.Run(() => DeliverAsync(title, text));
    }

    private static async Task DeliverAsync(string title, string text)
    {
        var T = Plugin.Configuration.SimplepushTitle.IsNullOrWhitespace()
            ? title
            : Plugin.Configuration.SimplepushTitle;
        
        var args = new Dictionary<string, string>
        {
            { "key", Plugin.Configuration.SimplepushKey },
            { "title", title },
            { "msg", text }
        };
        
        if (!Plugin.Configuration.SimplepushEvent.IsNullOrWhitespace())
            args.Add("event", Plugin.Configuration.SimplepushEvent);

        try
        {
            await SimplepushApi.PostJsonAsync(args);
            Service.PluginLog.Debug("Sent Simplepush message");
        }
        catch (FlurlHttpException e)
        {
            Service.PluginLog.Error($"Failed to make Simplepush request: '{e.Message}'");
            Service.PluginLog.Error($"{e.StackTrace}");
        }
    }
}
