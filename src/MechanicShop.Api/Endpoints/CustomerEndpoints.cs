using Asp.Versioning.Builder;

using MechanicShop.Api.Extensions;
using MechanicShop.Application.Features.Customers.Commands.CreateCustomer;
using MechanicShop.Application.Features.Customers.Commands.RemoveCustomer;
using MechanicShop.Application.Features.Customers.Commands.UpdateCustomer;
using MechanicShop.Application.Features.Customers.Dtos;
using MechanicShop.Application.Features.Customers.Queries.GetCustomerById;
using MechanicShop.Application.Features.Customers.Queries.GetCustomers;
using MechanicShop.Contracts.Requests.Customers;
using MechanicShop.Domain.Identity;

using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Endpoints;

public static class CustomerEndpoints
{
    public static void MapCustomerEndpoints(this IEndpointRouteBuilder app, ApiVersionSet apiVersionSet)
    {
        var endpoints = app.MapGroup("/api/v{apiVersion:apiVersion}/customers")
            .WithApiVersionSet(apiVersionSet)
            .HasApiVersion(1.0)
            .WithOpenApi()
            .RequireAuthorization();

        endpoints.MapGet("/", GetCustomers)
            .WithName("GetCustomers")
            .WithSummary("Retrieves all customers.")
            .WithDescription("Returns a list of all registered customers.")
            .Produces<List<CustomerDto>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)))
            .MapToApiVersion(1.0);

        endpoints.MapGet("/{customerId:guid}", GetCustomer)
            .WithName("GetCustomerById")
            .WithSummary("Retrieves a customer by ID.")
            .WithDescription("Returns detailed customer information for the given customer ID.")
            .Produces<CustomerDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .CacheOutput(c => c.Expire(TimeSpan.FromSeconds(60)))
            .MapToApiVersion(1.0);

        endpoints.MapPost("/", CreateCustomer)
            .RequireAuthorization("ManagerOnly")
            .WithName("CreateCustomer")
            .WithSummary("Creates a new customer.")
            .WithDescription("Adds a new customer to the system.")
            .Produces<CustomerDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
            .MapToApiVersion(1.0);

        endpoints.MapPut("/{customerId:guid}", UpdateCustomer)
          .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Manager)))
          .WithName("UpdateCustomer")
          .WithSummary("Updates an existing customer.")
          .WithDescription("Updates a customer and its associated vehicle.")
          .Produces<CustomerDto>(StatusCodes.Status200OK)
          .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
          .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
          .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
          .MapToApiVersion(1.0);

        endpoints.MapDelete("/{customerId:guid}", RemoveCustomer)
           .RequireAuthorization(policy => policy.RequireRole(nameof(Role.Manager)))
           .WithName("RemoveCustomer")
           .WithSummary("Removes a customer.")
           .WithDescription("Deletes the specified customer from the system.")
           .Produces(StatusCodes.Status204NoContent)
           .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
           .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError)
           .MapToApiVersion(1.0);
    }

    private static async Task<IResult> GetCustomers(ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomersQuery(), ct);
        return result.Match(Results.Ok, e => e.ToProblem());
    }

    private static async Task<IResult> GetCustomer(ISender sender, Guid customerId, CancellationToken ct)
    {
        var result = await sender.Send(new GetCustomerByIdQuery(customerId), ct);
        return result.Match(Results.Ok, e => e.ToProblem());
    }

    private static async Task<IResult> CreateCustomer(
        [FromBody] CreateCustomerRequest request,
        ISender sender,
        CancellationToken ct)
    {
        var vehicles = request.Vehicles
           .ConvertAll(v => new CreateVehicleCommand(v.Make, v.Model, v.Year, v.LicensePlate));

        var result = await sender.Send(
            new CreateCustomerCommand(
                request.Name,
                request.PhoneNumber,
                request.Email, vehicles),
            ct);

        return result.Match(
            r => Results.CreatedAtRoute("GetCustomerById", new { version = "1.0", customerId = r.CustomerId }, r),
            e => e.ToProblem());
    }

    private static async Task<IResult> UpdateCustomer(Guid customerId, UpdateCustomerRequest request, ISender sender, CancellationToken ct)
    {
        var vehicles = request.Vehicles
            .ConvertAll(v => new UpdateVehicleCommand(v.VehicleId, v.Make, v.Model, v.Year, v.LicensePlate));

        var result = await sender.Send(
            new UpdateCustomerCommand(
            customerId,
            request.Name,
            request.PhoneNumber,
            request.Email,
            vehicles),
            ct);

        return result.Match(_ => Results.Created(), e => e.ToProblem());
    }

    private static async Task<IResult> RemoveCustomer(Guid customerId, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(new RemoveCustomerCommand(customerId), ct);
        return result.Match(_ => Results.NoContent(), e => e.ToProblem());
    }
}