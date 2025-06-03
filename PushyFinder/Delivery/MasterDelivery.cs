using System.Collections.Generic;

namespace PushyFinder.Delivery;

internal interface IDelivery
{
    public bool IsActive { get; }
    public void Deliver(string title, string text);
}

public static class MasterDelivery
{
    private static readonly IReadOnlyList<IDelivery> Deliveries =
    [
        new PushoverDelivery(),
        new NtfyDelivery(),
        new SimplepushDelivery(),
        new DiscordDelivery()
    ];

    public static void Deliver(string title, string text)
    {
        foreach (var delivery in Deliveries)
            if (delivery.IsActive)
                delivery.Deliver(title, text);
    }
}
