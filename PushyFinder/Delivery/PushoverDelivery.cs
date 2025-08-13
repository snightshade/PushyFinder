using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Flurl.Http;

namespace PushyFinder.Delivery;

public class PushoverDelivery : IDelivery
{
    public static readonly string PushoverApi = "https://api.pushover.net/1/messages.json";

    private readonly Configuration configuration;
    private readonly IPluginLog pluginLog;

    public PushoverDelivery(Configuration configuration, IPluginLog pluginLog)
    {
        this.configuration = configuration;
        this.pluginLog = pluginLog;
    }

    public bool IsActive => !configuration.PushoverAppKey.IsNullOrWhitespace() &&
                            !configuration.PushoverDevice.IsNullOrWhitespace() &&
                            !configuration.PushoverUserKey.IsNullOrWhitespace();

    public void Deliver(string title, string text)
    {
        if (!IsActive) return;
        Task.Run(() => DeliverAsync(title, text));
    }

    private async Task DeliverAsync(string title, string text)
    {
        var args = new Dictionary<string, string>
        {
            { "token", configuration.PushoverAppKey },
            { "user", configuration.PushoverUserKey },
            { "device", configuration.PushoverDevice },
            { "title", title },
            { "message", text }
        };

        try
        {
            await PushoverApi.PostUrlEncodedAsync(args);
            pluginLog.Debug("Sent Pushover message");
        }
        catch (FlurlHttpException e)
        {
            pluginLog.Error($"Failed to make Pushover request: '{e.Message}'");
            pluginLog.Error($"{e.StackTrace}");
        }
    }
}