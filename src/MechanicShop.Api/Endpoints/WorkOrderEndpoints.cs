using Asp.Versioning.Builder;

using MechanicShop.Api.Extensions;
using MechanicShop.Application.Common.Models;
using MechanicShop.Application.Features.Scheduling.Dtos;
using MechanicShop.Application.Features.Scheduling.Queries.GetDailyScheduleQuery;
using MechanicShop.Application.Features.WorkOrders.Commands.AssignLabor;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.DeleteWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.RelocateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateOrderState;
using MechanicShop.Application.Features.WorkOrders.Commands.UpdateWorkOrderRepairTasks;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrderByIdQuery;
using MechanicShop.Application.Features.WorkOrders.Queries.GetWorkOrders;
using MechanicShop.Contracts.Requests.WorkOrders;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.Workorders.Enums;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Endpoints;

public static class WorkOrderEndpoints
{
    public static void MapWorkOrdersEndpoints(this IEndpointRouteBuilder app, ApiVersionSet apiVersionSet)
    {
        var endpoints = app.MapGroup("/api/v{apiVersion:apiVersion}/workorders")
    .WithOpenApi()
    .WithApiVersionSet(apiVersionSet)
    .HasApiVersion(1.0)
    .RequireAuthorization();

        endpoints.MapGet("/", GetWorkOrders)
            .WithName("GetWorkOrders")
            .MapToApiVersion(1.0)
            .WithSummary("Retrieves all work orders.")
            .WithDescription("Returns a paginated list of all work orders.")
            .Produces<PaginatedList<WorkOrderListItemDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/completed", GetWorkOrders)
            .WithName("GetCompletedWorkOrders")
            .MapToApiVersion(1.0)
            .WithSummary("Retrieves all completed work orders.")
            .WithDescription("Returns a paginated list of work orders filtered by 'Completed' status.")
            .Produces<PaginatedList<WorkOrderDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/{WorkOrderId:guid}", GetWorkOrderById)
            .WithName("GetWorkOrderById")
            .MapToApiVersion(1.0)
            .WithSummary("Retrieves a work order by ID.")
            .Produces<WorkOrderDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapPost("/", CreateWorkOrder)
            .WithName("CreateWorkOrder")
            .MapToApiVersion(1.0)
            .RequireAuthorization("ManagerOnly")
            .WithSummary("Creates a new work order.")
            .Produces<WorkOrderDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapPut("{WorkOrderId:guid}/relocation", RelocateWorkOrder)
            .WithName("RelocateWorkOrder")
            .MapToApiVersion(1.0)
            .RequireAuthorization("ManagerOnly")
            .WithSummary("Updates work order start time and assigned spot.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapPut("{WorkOrderId:guid}/labor", AssignLabor)
            .WithName("AssignLaborToWorkOrder")
            .MapToApiVersion(1.0)
            .RequireAuthorization("ManagerOnly")
            .WithSummary("Assigns labor to a work order.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapPut("{WorkOrderId:guid}/state", UpdateWorkOrderState)
            .WithName("UpdateWorkOrderState")
            .MapToApiVersion(1.0)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Manager), nameof(Role.Labor)))
            .RequireAuthorization("SelfScopedWorkOrderAccess")
            .WithSummary("Updates the state of a work order.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapPut("{WorkOrderId:guid}/repair-task", UpdateWorkOrderRepairTasks)
            .WithName("UpdateWorkOrderRepairTasks")
            .MapToApiVersion(1.0)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Manager)))
            .WithSummary("Updates repair tasks of a work order.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapDelete("{WorkOrderId:guid}", DeleteWorkOrder)
            .WithName("DeleteWorkOrder")
            .MapToApiVersion(1.0)
            .RequireAuthorization("ManagerOnly")
            .WithSummary("Deletes a work order.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/schedule/{date}", GetSchedule)
            .WithName("GetDailySchedule")
            .MapToApiVersion(1.0)
            .RequireAuthorization("ManagerOnly")
            .WithSummary("Gets the daily work order schedule.")
            .WithDescription("Returns the schedule view of work orders for a specific day. Defaults to today if no date is provided.")
            .Produces<ScheduleDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetWorkOrders(
    ISender sender,
    [AsParameters] WorkOrderFilterRequest filterRequest,
    [AsParameters] PageRequest pageRequest,
    CancellationToken ct)
    {
        // Input validation
        if (pageRequest.Page <= 0)
        {
            return Results.BadRequest("Page must be greater than 0");
        }

        if (pageRequest.PageSize <= 0 || pageRequest.PageSize > 100)
        {
            return Results.BadRequest("PageSize must be between 1 and 100");
        }

        var query = new GetWorkOrdersQuery(
            pageRequest.Page,
            pageRequest.PageSize,
            filterRequest.SearchTerm,
            filterRequest.SortColumn,
            filterRequest.SortDirection,
            filterRequest.State is not null ? (WorkOrderState)(int)filterRequest.State : null,
            filterRequest.VehicleId,
            filterRequest.LaborId,
            filterRequest.StartDateFrom,
            filterRequest.StartDateTo,
            filterRequest.EndDateFrom,
            filterRequest.EndDateTo,
            filterRequest.Spot is not null ? (Spot)(int)filterRequest.Spot : null);

        var result = await sender.Send(query, ct);

        return result.Match(
            workorders => Results.Ok(workorders),
            error => error.ToProblem());
    }

    private static async Task<IResult> GetSchedule(
    DateOnly? date,
    [FromQuery] Guid? laborId,
    [FromHeader(Name = "X-TimeZone")] string? tz,
    ISender sender,
    CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(tz))
        {
            return Results.Problem(
                detail: "Missing time zone in 'X-TimeZone' header.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Time Zone Required");
        }

        TimeZoneInfo timeZone;

        try
        {
            timeZone = TimeZoneInfo.FindSystemTimeZoneById(tz);
        }
        catch
        {
            return Results.Problem(
                detail: $"Invalid or unknown time zone: '{tz}'.",
                statusCode: StatusCodes.Status400BadRequest,
                title: "Invalid Time Zone");
        }

        var scheduleDate = date ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var result = await sender.Send(new GetDailyScheduleQuery(timeZone, scheduleDate, laborId), ct);

        return result.Match<IResult>(
            response => Results.Ok(response),
            error => error.ToProblem());
    }

    private static async Task<IResult> GetWorkOrderById(
        ISender sender,
        Guid WorkOrderId,
        CancellationToken ct)
    {
        var result = await sender.Send(new GetWorkOrderByIdQuery(WorkOrderId), ct);

        return result.Match(
               value => Results.Ok(value),
               error => error.ToProblem());
    }

    private static async Task<IResult> CreateWorkOrder(
        ISender sender,
        CreateWorkOrderCommand command,
        CancellationToken ct)
    {
        var result = await sender.Send(command, ct);

        return result.Match(
          wo => TypedResults.CreatedAtRoute(
            value: result.Value,
            routeName: "GetWorkOrderById",
            routeValues: new { wo.WorkOrderId }),
          error => error.ToProblem());
    }

    private static async Task<IResult> RelocateWorkOrder(
        ISender sender,
        Guid WorkOrderId,
        RelocateWorkOrderRequest request,
        CancellationToken ct)
    {
        var command = new RelocateWorkOrderCommand(
            WorkOrderId,
            request.NewStartAtUtc,
            (Spot)(int)request.NewSpot);

        var result = await sender.Send(command, ct);

        return result.Match(
                _ => Results.NoContent(),
                error => error.ToProblem());
    }

    private static async Task<IResult> AssignLabor(
        ISender sender,
        Guid WorkOrderId,
        AssignLaborRequest request,
        CancellationToken ct)
    {
        var command = new AssignLaborCommand(WorkOrderId, Guid.Parse(request.LaborId));

        var result = await sender.Send(command, ct);

        return result.Match(
               _ => Results.NoContent(),
               error => error.ToProblem());
    }

    private static async Task<IResult> UpdateWorkOrderState(
        ISender sender,
        Guid WorkOrderId,
        UpdateWorkOrderStateRequest request,
        CancellationToken ct)
    {
        var command = new UpdateWorkOrderStateCommand(
            WorkOrderId,
            (WorkOrderState)request.State);

        var result = await sender.Send(command, ct);

        return result.Match(
               _ => Results.NoContent(),
               error => error.ToProblem());
    }

    private static async Task<IResult> UpdateWorkOrderRepairTasks(
    ISender sender,
    Guid WorkOrderId,
    ModifyRepairTaskRequest request,
    CancellationToken ct)
    {
        var command = new UpdateWorkOrderRepairTasksCommand(WorkOrderId, request.RepairTaskIds);

        var result = await sender.Send(command, ct);

        return result.Match(
               _ => Results.NoContent(),
               error => error.ToProblem());
    }

    private static async Task<IResult> DeleteWorkOrder(
        ISender sender,
        Guid WorkOrderId,
        CancellationToken ct)
    {
        var result = await sender.Send(new DeleteWorkOrderCommand(WorkOrderId), ct);

        return result.Match(
               _ => Results.NoContent(),
               error => error.ToProblem());
    }
}