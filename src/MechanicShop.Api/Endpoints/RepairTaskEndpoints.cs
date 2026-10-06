using Asp.Versioning.Builder;

using MechanicShop.Api.Extensions;
using MechanicShop.Application.Features.RepairTasks.Commands.CreateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.RemoveRepairTask;
using MechanicShop.Application.Features.RepairTasks.Commands.UpdateRepairTask;
using MechanicShop.Application.Features.RepairTasks.Dtos;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTaskById;
using MechanicShop.Application.Features.RepairTasks.Queries.GetRepairTasks;
using MechanicShop.Contracts.Requests.RepairTasks;
using MechanicShop.Domain.Identity;
using MechanicShop.Domain.RepairTasks.Enums;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Endpoints;

public static class RepairTaskEndpoints
{
    public static void MapRepairTaskEndpoints(this IEndpointRouteBuilder app, ApiVersionSet apiVersionSet)
    {
        var endpoints = app.MapGroup("/api/v{apiVersion:apiVersion}/repair-tasks")
            .WithApiVersionSet(apiVersionSet)
            .HasApiVersion(1.0)
            .WithOpenApi()
            .RequireAuthorization();

        endpoints.MapGet("/", GetRepairTasks)
            .WithName("GetRepairTasks")
            .WithSummary("Retrieves all repair tasks.")
            .WithDescription("Returns a list of all repair tasks available in the system.")
            .Produces<List<RepairTaskDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)))
            .MapToApiVersion(1.0);

        endpoints.MapGet("/{repairTaskId:guid}", GetRepairTaskById)
            .WithName("GetRepairTaskById")
            .WithSummary("Retrieves a repair task by ID.")
            .WithDescription("Returns detailed information for the specified repair task if it exists.")
            .Produces<RepairTaskDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)))
            .MapToApiVersion(1.0);

        endpoints.MapPost("/", CreateRepairTask)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Manager)))
            .WithName("CreateRepairTask")
            .WithSummary("Creates a new repair task.")
            .WithDescription("Creates a repair task and optionally includes parts.")
            .Produces<RepairTaskDto>(StatusCodes.Status201Created)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .MapToApiVersion(1.0);

        endpoints.MapPut("/{repairTaskId:guid}", UpdateRepairTask)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Manager)))
            .WithName("UpdateRepairTask")
            .WithSummary("Updates an existing repair task.")
            .WithDescription("Updates a repair task and its associated parts.")
            .Produces<RepairTaskDto>(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .MapToApiVersion(1.0);

        endpoints.MapDelete("/{repairTaskId:guid}", RemoveRepairTask)
            .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Manager)))
            .WithName("RemoveRepairTask")
            .WithSummary("Removes a repair task.")
            .WithDescription("Deletes the specified repair task from the system.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .MapToApiVersion(1.0);
    }

    private static async Task<IResult> GetRepairTasks(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTasksQuery(), ct);
        return result.Match(Results.Ok, e => e.ToProblem());
    }

    private static async Task<IResult> GetRepairTaskById(ISender sender, Guid repairTaskId, CancellationToken ct)
    {
        var result = await sender.Send(new GetRepairTaskByIdQuery(repairTaskId), ct);
        return result.Match(Results.Ok, e => e.ToProblem());
    }

    private static async Task<IResult> CreateRepairTask(CreateRepairTaskRequest request, ISender sender, CancellationToken ct)
    {
        var parts = request.Parts
            .ConvertAll(p => new CreateRepairTaskPartCommand(p.Name, p.Cost, p.Quantity));

        var command = new CreateRepairTaskCommand(
            request.Name,
            request.LaborCost,
            request.EstimatedDurationInMins is not null ? (RepairDurationInMinutes)request.EstimatedDurationInMins : null,
            parts);

        var result = await sender.Send(command, ct);

        return result.Match(
            r => Results.CreatedAtRoute("GetRepairTaskById", new { version = "1.0", repairTaskId = r.RepairTaskId }, r),
            e => e.ToProblem());
    }

    private static async Task<IResult> UpdateRepairTask(
        Guid repairTaskId,
        UpdateRepairTaskRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var parts = request.Parts
            .ConvertAll(p => new UpdateRepairTaskPartCommand(p.PartId, p.Name, p.Cost, p.Quantity));

        var command = new UpdateRepairTaskCommand(
            repairTaskId,
            request.Name,
            request.LaborCost,
            (RepairDurationInMinutes)request.EstimatedDurationInMins,
            parts);

        var result = await sender.Send(command, ct);
        return result.Match(Results.Ok, e => e.ToProblem());
    }

    private static async Task<IResult> RemoveRepairTask(Guid repairTaskId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveRepairTaskCommand(repairTaskId), ct);
        return result.Match(_ => Results.NoContent(), e => e.ToProblem());
    }
}