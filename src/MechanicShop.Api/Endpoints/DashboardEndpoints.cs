using Asp.Versioning.Builder;
using MechanicShop.Api.Extensions;
using MechanicShop.Application.Features.Dashboard.Dtos;
using MechanicShop.Application.Features.Dashboard.Queries.GetWorkOrderStats;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Endpoints;

public static class DashboardEndpoints
{
    public static void MapDashboardEndpoints(this IEndpointRouteBuilder app, ApiVersionSet versionSet)
    {
        var endpoints = app.MapGroup("/api/v{apiVersion:apiVersion}/dashboard")
            .WithApiVersionSet(versionSet)
            .WithOpenApi()
            .RequireAuthorization("ManagerOnly");

        endpoints.MapGet("/stats", GetWorkOrderStats)
            .WithSummary("Get today's work order stats")
            .Produces<TodayWorkOrderStatsDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .MapToApiVersion(1.0);
    }

    private static async Task<IResult> GetWorkOrderStats(
        [AsParameters] DashboardStatsQuery queryParams,
        ISender sender,
        CancellationToken ct)
    {
        var date = queryParams.Date ?? DateOnly.FromDateTime(DateTime.UtcNow);
        var result = await sender.Send(new GetWorkOrderStatsQuery(date), ct);

        return result.Match(
            value => Results.Ok(value),
            error => error.ToProblem());
    }

    public sealed record DashboardStatsQuery([FromQuery] DateOnly? Date);
}