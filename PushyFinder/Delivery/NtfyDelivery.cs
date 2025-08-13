using System;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Flurl.Http;

namespace PushyFinder.Delivery;

public class NtfyDelivery : IDelivery
{
    private readonly Configuration configuration;
    private readonly IPluginLog pluginLog;

    public NtfyDelivery(Configuration configuration, IPluginLog pluginLog)
    {
        this.configuration = configuration;
        this.pluginLog = pluginLog;
    }
    
    public bool IsActive => !configuration.NtfyServer.IsNullOrWhitespace() &&
                            !configuration.NtfyTopic.IsNullOrWhitespace() && 
                            Uri.IsWellFormedUriString(configuration.NtfyServer, UriKind.Absolute);

    public void Deliver(string title, string text)
    {
        Task.Run(() => DeliverAsync(title, text));
    }

    private async Task DeliverAsync(string title, string text)
    {
        // Assuming `title` is used as the main message
        // Since ntfy doesn't have a separate title field, you can prepend it to the message or just send the message
        var args = string.IsNullOrEmpty(title) ? text : $"{title}: {text}";
       
        IFlurlRequest request = new FlurlRequest(configuration.NtfyServer);
        if (!configuration.NtfyTopic.IsNullOrWhitespace())
            request = request.AppendPathSegment(configuration.NtfyTopic);
        if (!configuration.NtfyToken.IsNullOrWhitespace())
            request = request.WithOAuthBearerToken(configuration.NtfyToken);

        try
        {
            await request.PostJsonAsync(args);
            pluginLog.Debug("Sent Ntfy message");
        }
        catch (FlurlHttpException e)
        {
            pluginLog.Error($"Failed to make Ntfy request: '{e.Message}'");
            pluginLog.Error($"{e.StackTrace}");
        }
        catch (ArgumentException e)
        {
            pluginLog.Error($"{e.StackTrace}");
        }

    }
}