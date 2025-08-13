using System.Collections.Generic;

namespace PushyFinder.Delivery;

// Change 'internal' to 'public' here
public interface IDelivery
{
    public bool IsActive { get; }
    public void Deliver(string title, string text);
}

public class MasterDelivery
{
    private readonly IReadOnlyList<IDelivery> deliveries;

    public MasterDelivery(IReadOnlyList<IDelivery> deliveries)
    {
        this.deliveries = deliveries;
    }

    public void Deliver(string title, string text)
    {
        foreach (var delivery in deliveries)
            if (delivery.IsActive)
                delivery.Deliver(title, text);
    }
}