using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Dalamud.Utility;
using Flurl.Http;

namespace PushyFinder.Delivery;

public class NtfyDelivery : IDelivery
{
    public bool IsActive => !Plugin.Configuration.NtfyServer.IsNullOrWhitespace() &&
                            !Plugin.Configuration.NtfyTopic.IsNullOrWhitespace() && 
                            Uri.IsWellFormedUriString(Plugin.Configuration.NtfyServer, UriKind.Absolute);

    public void Deliver(string title, string text)
    {
        Task.Run(() => DeliverAsync(title, text));
    }

    private static async Task DeliverAsync(string title, string text)
    {
        // Assuming `title` is used as the main message
        // Since ntfy doesn't have a separate title field, you can prepend it to the message or just send the message
        var args = string.IsNullOrEmpty(title) ? text : $"{title}: {text}";
       
        IFlurlRequest request = new FlurlRequest(Plugin.Configuration.NtfyServer);
        if (!Plugin.Configuration.NtfyTopic.IsNullOrWhitespace())
            request = request.AppendPathSegment(Plugin.Configuration.NtfyTopic);
        if (!Plugin.Configuration.NtfyToken.IsNullOrWhitespace())
            request = request.WithOAuthBearerToken(Plugin.Configuration.NtfyToken);

        try
        {
            await request.PostJsonAsync(args);
            Service.PluginLog.Debug("Sent Ntfy message");
        }
        catch (FlurlHttpException e)
        {
            Service.PluginLog.Error($"Failed to make Ntfy request: '{e.Message}'");
            Service.PluginLog.Error($"{e.StackTrace}");
        }
        catch (ArgumentException e)
        {
            Service.PluginLog.Error($"{e.StackTrace}");
        }

    }
}
