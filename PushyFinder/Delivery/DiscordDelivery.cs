using System;
using System.Text.Json;
using System.Threading.Tasks;
using Dalamud.Plugin.Services;
using Dalamud.Utility;
using Flurl.Http;
using PushyFinder.Discord;

namespace PushyFinder.Delivery;

internal class DiscordDelivery : IDelivery
{
    private readonly Configuration configuration;
    private readonly IPluginLog pluginLog;

    public DiscordDelivery(Configuration configuration, IPluginLog pluginLog)
    {
        this.configuration = configuration;
        this.pluginLog = pluginLog;
    }

    public bool IsActive => !configuration.DiscordWebhookToken.IsNullOrWhitespace() &&
                            Uri.IsWellFormedUriString(configuration.DiscordWebhookToken, UriKind.Absolute);

    public void Deliver(string title, string text)
    {
        Task.Run(() => DeliverAsync(title, text));
    }

    private async Task DeliverAsync(string title, string text)
    {
        var webhook = new WebhookBuilder();

        if (!configuration.DiscordUseEmbed)
            webhook.WithContent(title + "\n" + text);
        else
        {
            webhook
                .WithContent(configuration.DiscordMessage)
                .WithEmbed(new EmbedBuilder()
                              .WithDescription(text)
                              .WithTitle(title)
                              .WithColor(configuration.DiscordEmbedColor)
                              .WithAuthor("PushyFinder", "https://github.com/snightshade/PushyFinder",
                                          "https://raw.githubusercontent.com/goatcorp/PluginDistD17/main/stable/PushyFinder/images/icon.png"));
        }

        webhook.WithAvatarUrl(
            "https://raw.githubusercontent.com/goatcorp/PluginDistD17/main/stable/PushyFinder/images/icon.png");
        webhook.WithUsername("PushyFinder");

        try
        {
            // this can break if they register a webhook to a channel type of forum or media
            await configuration.DiscordWebhookToken.PostJsonAsync(webhook.Build());
            pluginLog.Debug("Sent Discord message");
        }
        catch (FlurlHttpException e)
        {
            // Discord returns a json object within the message that contains the error not sure if this is something that should be parsed or not
            pluginLog.Error($"Failed to make Discord request: '{e.Message}'");
            pluginLog.Error($"{e.StackTrace}");
            pluginLog.Debug(JsonSerializer.Serialize(webhook.Build()));
        }
        catch (ArgumentException e)
        {
            pluginLog.Error($"{e.StackTrace}");
        }
    }
}