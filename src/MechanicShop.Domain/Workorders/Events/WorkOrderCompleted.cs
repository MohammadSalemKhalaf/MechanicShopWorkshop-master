using MechanicShop.Domain.Common;

namespace MechanicShop.Domain.Workorders.Events;

public sealed class WorkOrderCompleted : DomainEvent
{
    public Guid WorkOrderId { get; set; }
}