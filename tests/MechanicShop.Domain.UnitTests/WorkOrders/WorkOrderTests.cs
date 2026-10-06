using MechanicShop.Domain.Workorders;
using MechanicShop.Domain.Workorders.Enums;
using MechanicShop.Tests.Common.RepaireTasks;

using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders;

public class WorkOrderTests
{
    [Fact]
    public void Create_ShouldReturnError_WhenIdIsEmpty()
    {
        var wo = WorkOrder.Create(
                    id: Guid.Empty,
                    vehicleId: Guid.NewGuid(),
                    startAt: DateTimeOffset.UtcNow,
                    endAt: DateTimeOffset.UtcNow.AddHours(1),
                    laborId: Guid.NewGuid(),
                    spot: Spot.A,
                    repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(wo.IsSuccess);

        Assert.Equal(WorkOrderErrors.WorkOrderIdRequired.Code, wo.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenVehicleIdIsEmpty()
    {
        var wo = WorkOrder.Create(
                           id: Guid.NewGuid(),
                           vehicleId: Guid.Empty,
                           startAt: DateTimeOffset.UtcNow,
                           endAt: DateTimeOffset.UtcNow.AddHours(1),
                           laborId: Guid.NewGuid(),
                           spot: Spot.A,
                           repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(wo.IsSuccess);

        Assert.Equal(WorkOrderErrors.VehicleIdRequired.Code, wo.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenNoRepairTasks()
    {
        var wo = WorkOrder.Create(
                           id: Guid.NewGuid(),
                           vehicleId: Guid.NewGuid(),
                           startAt: DateTimeOffset.UtcNow,
                           endAt: DateTimeOffset.UtcNow.AddHours(1),
                           laborId: Guid.NewGuid(),
                           spot: Spot.A,
                           repairTasks: []);

        Assert.False(wo.IsSuccess);

        Assert.Equal(WorkOrderErrors.RepairTasksRequired.Code, wo.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenLaborIdIsEmpty()
    {
        var wo = WorkOrder.Create(
                              id: Guid.NewGuid(),
                              vehicleId: Guid.NewGuid(),
                              startAt: DateTimeOffset.UtcNow,
                              endAt: DateTimeOffset.UtcNow.AddHours(1),
                              laborId: Guid.Empty,
                              spot: Spot.A,
                              repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(wo.IsSuccess);

        Assert.Equal(WorkOrderErrors.LaborIdRequired.Code, wo.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenTimingInvalid()
    {
        var wo = WorkOrder.Create(
                           id: Guid.NewGuid(),
                           vehicleId: Guid.NewGuid(),
                           startAt: DateTimeOffset.UtcNow.AddHours(1),
                           endAt: DateTimeOffset.UtcNow,
                           laborId: Guid.NewGuid(),
                           spot: Spot.A,
                           repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(wo.IsSuccess);

        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, wo.TopError.Code);
    }

    [Fact]
    public void Create_ShouldReturnError_WhenSpotInvalid()
    {
        const Spot invalidSpot = (Spot)999;

        var wo = WorkOrder.Create(
                      id: Guid.NewGuid(),
                      vehicleId: Guid.NewGuid(),
                      startAt: DateTimeOffset.UtcNow,
                      endAt: DateTimeOffset.UtcNow.AddHours(1),
                      laborId: Guid.NewGuid(),
                      spot: invalidSpot,
                      repairTasks: [RepairTaskFactory.CreateRepairTask().Value]);

        Assert.False(wo.IsSuccess);

        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, wo.TopError.Code);
    }

    [Fact]
    public void AddRepairTask_ShouldReturnError_WhenNotEditable()
    {
        var wo = WorkOrder.Create(
                   id: Guid.NewGuid(),
                   vehicleId: Guid.NewGuid(),
                   startAt: DateTimeOffset.UtcNow,
                   endAt: DateTimeOffset.UtcNow.AddHours(1),
                   laborId: Guid.NewGuid(),
                   spot: Spot.A,
                   repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        wo.UpdateState(WorkOrderState.InProgress);
        wo.UpdateState(WorkOrderState.Completed);

        var result = wo.AddRepairTask(RepairTaskFactory.CreateRepairTask().Value);

        Assert.False(result.IsSuccess);
        Assert.True(result.Errors.Count > 0);
    }

    [Fact]
    public void UpdateLabor_ShouldReturnError_WhenLaborIdEmpty()
    {
        var wo = WorkOrder.Create(
                       id: Guid.NewGuid(),
                       vehicleId: Guid.NewGuid(),
                       startAt: DateTimeOffset.UtcNow,
                       endAt: DateTimeOffset.UtcNow.AddHours(1),
                       laborId: Guid.NewGuid(),
                       spot: Spot.A,
                       repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateLabor(Guid.Empty);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.LaborIdEmpty(wo.Id.ToString()).Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateSpot_ShouldReturnError_WhenSpotInvalid()
    {
        var wo = WorkOrder.Create(
               id: Guid.NewGuid(),
               vehicleId: Guid.NewGuid(),
               startAt: DateTimeOffset.UtcNow,
               endAt: DateTimeOffset.UtcNow.AddHours(1),
               laborId: Guid.NewGuid(),
               spot: Spot.A,
               repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        const Spot invalidSpot = (Spot)999;
        var result = wo.UpdateSpot(invalidSpot);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.SpotInvalid.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateTiming_ShouldReturnError_WhenInvalid()
    {
        var wo = WorkOrder.Create(
                          id: Guid.NewGuid(),
                          vehicleId: Guid.NewGuid(),
                          startAt: DateTimeOffset.UtcNow,
                          endAt: DateTimeOffset.UtcNow.AddHours(1),
                          laborId: Guid.NewGuid(),
                          spot: Spot.A,
                          repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateTiming(DateTimeOffset.UtcNow.AddHours(2), DateTimeOffset.UtcNow);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.InvalidTiming.Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateState_ShouldReturnError_WhenTransitionInvalid()
    {
        var wo = WorkOrder.Create(
                      id: Guid.NewGuid(),
                      vehicleId: Guid.NewGuid(),
                      startAt: DateTimeOffset.UtcNow,
                      endAt: DateTimeOffset.UtcNow.AddHours(1),
                      laborId: Guid.NewGuid(),
                      spot: Spot.A,
                      repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateState(WorkOrderState.Completed);

        Assert.False(result.IsSuccess);
        Assert.Equal(WorkOrderErrors.InvalidStateTransition(WorkOrderState.Scheduled, WorkOrderState.Completed).Code, result.TopError.Code);
    }

    [Fact]
    public void UpdateLabor_ShouldReturnSuccess_AndSetNewLaborId()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newLabor = Guid.NewGuid();
        var result = wo.UpdateLabor(newLabor);

        Assert.True(result.IsSuccess);
        Assert.Equal(newLabor, wo.LaborId);
    }

    [Fact]
    public void UpdateSpot_ShouldReturnSuccess_AndSetNewSpot()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateSpot(Spot.B);

        Assert.True(result.IsSuccess);
        Assert.Equal(Spot.B, wo.Spot);
    }

    [Fact]
    public void UpdateTiming_ShouldReturnSuccess_AndSetNewTiming()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var newStart = wo.StartAtUtc.AddHours(2);
        var newEnd = newStart.AddHours(1);
        var result = wo.UpdateTiming(newStart, newEnd);

        Assert.True(result.IsSuccess);
        Assert.Equal(newStart, wo.StartAtUtc);
        Assert.Equal(newEnd, wo.EndAtUtc);
    }

    [Fact]
    public void UpdateState_ShouldReturnSuccess_AndSetStateToInProgress()
    {
        var wo = WorkOrder.Create(
            id: Guid.NewGuid(),
            vehicleId: Guid.NewGuid(),
            startAt: DateTimeOffset.UtcNow,
            endAt: DateTimeOffset.UtcNow.AddHours(1),
            laborId: Guid.NewGuid(),
            spot: Spot.A,
            repairTasks: [RepairTaskFactory.CreateRepairTask().Value]).Value;

        var result = wo.UpdateState(WorkOrderState.InProgress);

        Assert.True(result.IsSuccess);
        Assert.Equal(WorkOrderState.InProgress, wo.State);
    }
}