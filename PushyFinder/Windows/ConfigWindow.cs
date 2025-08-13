using System;
using System.Numerics;
using Dalamud.Interface.Utility.Raii;
using Dalamud.Interface.Windowing;
using Dalamud.Bindings.ImGui;
using PushyFinder.Delivery;
using PushyFinder.Util;

namespace PushyFinder.Windows;

public class ConfigWindow : Window, IDisposable
{
    private readonly Configuration configuration;
    private readonly MasterDelivery masterDelivery;
    private readonly CharacterUtil characterUtil;

    private readonly TimedBool notifSentMessageTimer = new(3.0f);

    public ConfigWindow(Configuration configuration, MasterDelivery masterDelivery, CharacterUtil characterUtil) : base(
        "PushyFinder Configuration",
        ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        this.configuration = configuration;
        this.masterDelivery = masterDelivery;
        this.characterUtil = characterUtil;
    }

    public void Dispose() { }

    private void DrawPushoverConfig()
    {
        var appKey = this.configuration.PushoverAppKey ?? "";
        if (ImGui.InputText("Application key", ref appKey, 30))
        {
            this.configuration.PushoverAppKey = appKey;
            this.configuration.Save();
        }

        var userKey = this.configuration.PushoverUserKey ?? "";
        if (ImGui.InputText("User key", ref userKey, 30))
        {
            this.configuration.PushoverUserKey = userKey;
            this.configuration.Save();
        }
        
        var device = this.configuration.PushoverDevice ?? "";
        if (ImGui.InputText("Device name (optional)", ref device, 25))
        {
            this.configuration.PushoverDevice = device;
            this.configuration.Save();
        }
    }

    private void DrawNtfyConfig()
    {
        var server = this.configuration.NtfyServer ?? "";
        if (ImGui.InputText("Server", ref server, 128))
        {
            this.configuration.NtfyServer = server;
            this.configuration.Save();
        }

        var topic = this.configuration.NtfyTopic ?? "";
        if (ImGui.InputText("Topic", ref topic, 64))
        {
            this.configuration.NtfyTopic = topic;
            this.configuration.Save();
        }
        
        var token = this.configuration.NtfyToken ?? "";
        if (ImGui.InputText("Token (optional)", ref token, 128))
        {
            this.configuration.NtfyToken = token;
            this.configuration.Save();
        }
    }

    private void DrawSimplepushConfig()
    {
        var key = this.configuration.SimplepushKey ?? "";
        if (ImGui.InputText("Key", ref key, 16))
        {
            this.configuration.SimplepushKey = key;
            this.configuration.Save();
        }

        var title = this.configuration.SimplepushTitle ?? "";
        if (ImGui.InputText("Title (optional)", ref title, 64))
        {
            this.configuration.SimplepushTitle = title;
            this.configuration.Save();
        }

        var ev = this.configuration.SimplepushEvent ?? "";
        if (ImGui.InputText("Event (optional)", ref ev, 64))
        {
            this.configuration.SimplepushEvent = ev;
            this.configuration.Save();
        }
    }

    private void DrawDiscordConfig()
    {
        var webhookUrl = this.configuration.DiscordWebhookToken ?? "";
        if (ImGui.InputText("Webhook URL", ref webhookUrl, 200))
        {
            this.configuration.DiscordWebhookToken = webhookUrl;
            this.configuration.Save();
        }

        var message = this.configuration.DiscordMessage ?? "";
        if (ImGui.InputText("Message", ref message, 256))
        {
            this.configuration.DiscordMessage = message;
            this.configuration.Save();
        }

        var useEmbed = this.configuration.DiscordUseEmbed;
        if (ImGui.Checkbox("Use embeds?", ref useEmbed))
        {
            this.configuration.DiscordUseEmbed = useEmbed;
            this.configuration.Save();
        }
        
        var vec3Col = new Vector3();
        var cfg = this.configuration.DiscordEmbedColor;
        vec3Col.X = ((cfg >> 16) & 0xFF) / 255.0f;
        vec3Col.Y = ((cfg >> 8) & 0xFF) / 255.0f;
        vec3Col.Z = (cfg & 0xFF) / 255.0f;
        if (ImGui.ColorEdit3("Embed color", ref vec3Col))
        {
            this.configuration.DiscordEmbedColor = ((uint)(vec3Col.X * 255) << 16) | ((uint)(vec3Col.Y * 255) << 8) | (uint)(vec3Col.Z * 255);
            this.configuration.Save();
        }
    }

    public override void Draw()
    {
        using (var tabBar = ImRaii.TabBar("Services"))
        {
            if (!tabBar) return;
            
            using (var pushoverTab = ImRaii.TabItem("Pushover"))
            {
                if (pushoverTab) DrawPushoverConfig();
            }
            using (var ntfyTab = ImRaii.TabItem("Ntfy"))
            {
                if (ntfyTab) DrawNtfyConfig();
            }
            using (var simplepushTab = ImRaii.TabItem("Simplepush"))
            {
                if (simplepushTab) DrawSimplepushConfig();
            }
            using (var discordTab = ImRaii.TabItem("Discord"))
            {
                if (discordTab) DrawDiscordConfig();
            }
        }

        ImGui.Separator();

        if (ImGui.Button("Send test notification"))
        {
            notifSentMessageTimer.Start();
            masterDelivery.Deliver("Test notification",
                                   "If you received this, PushyFinder is configured correctly.");
        }

        if (notifSentMessageTimer.Value)
        {
            ImGui.SameLine();
            ImGui.Text("Notification sent!");
        }

        ImGui.Separator();
        
        var enablePops = this.configuration.EnableForDutyPops;
        if (ImGui.Checkbox("Send message for duty pop?", ref enablePops))
        {
            this.configuration.EnableForDutyPops = enablePops;
            this.configuration.Save();
        }

        var ignoreAfk = this.configuration.IgnoreAfkStatus;
        if (ImGui.Checkbox("Ignore AFK status and always notify", ref ignoreAfk))
        {
            this.configuration.IgnoreAfkStatus = ignoreAfk;
            this.configuration.Save();
        }

        if (!configuration.IgnoreAfkStatus)
        {
            ImGui.Dummy(new Vector2(0.0f, 5.0f));
            if (!characterUtil.IsClientAfk())
            {
                var red = new Vector4(1.0f, 0.2f, 0.2f, 1.0f);
                ImGui.TextColored(red, "Plugin is inactive (you are not AFK).");
                
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
                var green = new Vector4(0.2f, 1.0f, 0.2f, 1.0f);
                ImGui.TextColored(green, "Plugin is active (you are AFK).");
            }
        }

        ImGui.Dummy(new Vector2(0.0f, 10.0f));

        if (ImGui.Button("Save and Close"))
        {
            this.configuration.Save();
            this.IsOpen = false;
        }
    }
}