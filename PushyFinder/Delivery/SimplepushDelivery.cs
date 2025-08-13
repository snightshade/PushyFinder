using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Flurl.Http;

namespace PushyFinder.Delivery;

public class SimplepushDelivery : IDelivery
{
    private readonly Configuration configuration;
    private readonly IPluginLog pluginLog;
    
    public SimplepushDelivery(Configuration configuration, IPluginLog pluginLog)
    {
        this.configuration = configuration;
        this.pluginLog = pluginLog;
    }
    
    public bool IsActive =>
        !configuration.SimplepushKey.IsNullOrWhitespace();

    public void Deliver(string title, string text)
    {
        if (!IsActive) return;
        Task.Run(() => DeliverAsync(title, text));
    }

    private async Task DeliverAsync(string title, string text)
    {
        var key = configuration.SimplepushKey;
        var configuredTitle = configuration.SimplepushTitle;
        var eventParam = configuration.SimplepushEvent;

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
            pluginLog.Debug("Sent Simplepush message");
        }
        catch (FlurlHttpException e)
        {
            var responseBody = await e.GetResponseStringAsync();
            pluginLog.Error($"Failed to send Simplepush message: '{e.Message}'");
            pluginLog.Error($"Simplepush API response: {responseBody}");
        }
        catch (ArgumentException e)
        {
            pluginLog.Error($"Invalid argument: {e.Message}");
            pluginLog.Error($"{e.StackTrace}");
        }
    }
}