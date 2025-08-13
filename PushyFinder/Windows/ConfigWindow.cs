using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui; 
using PushyFinder.Delivery;
using PushyFinder.Util;
using System.Text;

namespace PushyFinder.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration Configuration;
    private readonly TimedBool notifSentMessageTimer = new(3.0f);

    public ConfigWindow(Plugin plugin) : base(
        "PushyFinder Configuration",
        ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
        ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        Configuration = Plugin.Configuration;
    }

    public void Dispose() { }

    private void DrawPushoverConfig()
    {
        var pushoverAppKey = Configuration.PushoverAppKey ?? "";
        if (ImGui.InputText("Application key", ref pushoverAppKey, 2048)) Configuration.PushoverAppKey = pushoverAppKey;

        var pushoverUserKey = Configuration.PushoverUserKey ?? "";
        if (ImGui.InputText("User key", ref pushoverUserKey, 2048)) Configuration.PushoverUserKey = pushoverUserKey;
        
        var pushoverDevice = Configuration.PushoverDevice ?? "";
        if (ImGui.InputText("Device name", ref pushoverDevice, 2048)) Configuration.PushoverDevice = pushoverDevice;
    }

    private void DrawNtfyConfig()
    {
        var ntfyServer = Configuration.NtfyServer ?? "";
        if (ImGui.InputText("Server", ref ntfyServer, 2048)) Configuration.NtfyServer = ntfyServer;

        var ntfyTopic = Configuration.NtfyTopic ?? "";
        if (ImGui.InputText("Topic", ref ntfyTopic, 2048)) Configuration.NtfyTopic = ntfyTopic;

        var ntfyToken = Configuration.NtfyToken ?? "";
        if (ImGui.InputText("Token (if exists)", ref ntfyToken, 2048)) Configuration.NtfyToken = ntfyToken;
    }

    private void DrawDiscordConfig()
    {
        var discordMessage = Configuration.DiscordMessage ?? "";
        if (ImGui.InputText("Message", ref discordMessage, 2048)) Configuration.DiscordMessage = discordMessage;

        var discordWebhookToken = Configuration.DiscordWebhookToken ?? "";
        if (ImGui.InputText("Webhook URL", ref discordWebhookToken, 2048)) Configuration.DiscordWebhookToken = discordWebhookToken;
        
        {
            var cfg = Configuration.DiscordUseEmbed;
            if (ImGui.Checkbox("Use embeds?", ref cfg)) Configuration.DiscordUseEmbed = cfg;
        }
        {
            var vec3Col = new Vector3();
            var cfg = Configuration.DiscordEmbedColor;
            vec3Col.X = ((cfg >> 16) & 0xFF) / 255.0f;
            vec3Col.Y = ((cfg >> 8) & 0xFF) / 255.0f;
            vec3Col.Z = (cfg & 0xFF) / 255.0f;
            if (ImGui.ColorEdit3("Embed color", ref vec3Col))
            {
                cfg = ((uint)(vec3Col.X * 255) << 16) | ((uint)(vec3Col.Y * 255) << 8) | (uint)(vec3Col.Z * 255);
                Configuration.DiscordEmbedColor = cfg;
            }
        }
    }

    public override void Draw()
    {
        using (var tabBar = ImRaii.TabBar("Services"))
        {
            if (tabBar)
            {
                using (var pushoverTab = ImRaii.TabItem("Pushover"))
                {
                    if (pushoverTab) DrawPushoverConfig();
                }
                using (var ntfyTab = ImRaii.TabItem("Ntfy"))
                {
                    if (ntfyTab) DrawNtfyConfig();
                }
                using (var discordTab = ImRaii.TabItem("Discord"))
                {
                    if (discordTab) DrawDiscordConfig();
                }
            }
        }

        ImGui.NewLine();

        if (ImGui.Button("Send test notification"))
        {
            notifSentMessageTimer.Start();
            MasterDelivery.Deliver("Test notification",
                                   "If you received this, PushyFinder is configured correctly.");
        }

        if (notifSentMessageTimer.Value)
        {
            ImGui.SameLine();
            ImGui.Text("Notification sent!");
        }

        {
            var cfg = Configuration.EnableForDutyPops;
            if (ImGui.Checkbox("Send message for duty pop?", ref cfg)) Configuration.EnableForDutyPops = cfg;
        }
        {
            var cfg = Configuration.IgnoreAfkStatus;
            if (ImGui.Checkbox("Ignore AFK status and always notify", ref cfg)) Configuration.IgnoreAfkStatus = cfg;
        }

        if (!Configuration.IgnoreAfkStatus)
        {
            if (!CharacterUtil.IsClientAfk())
            {
                var red = new Vector4(1.0f, 0.0f, 0.0f, 1.0f);
                ImGui.TextColored(red, "This plugin will only function while your client is AFK (/afk, red icon)!");

                if (ImGui.IsItemHovered())
                {
                    ImGui.BeginTooltip();
                    ImGui.Text("The reasoning for this is that if you are not AFK, you are assumed to");
                    ImGui.Text("be at your computer, and ready to respond to a join or a duty pop.");
                    ImGui.Text("Notifications would be bothersome, so they are disabled.");
                    ImGui.EndTooltip();
                }
            }
            else
            {
                var green = new Vector4(0.0f, 1.0f, 0.0f, 1.0f);
                ImGui.TextColored(green, "You are AFK. The plugin is active and notifications will be served.");
            }
        }

        if (ImGui.Button("Save and close"))
        {
            Configuration.Save();
            IsOpen = false;
        }
    }
}
