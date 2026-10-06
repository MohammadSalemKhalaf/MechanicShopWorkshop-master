using MechanicShop.Api.Extensions;
using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;
using MechanicShop.Application.Features.Billing.Dtos;
using MechanicShop.Application.Features.Billing.Queries.GetInvoiceById;
using MechanicShop.Application.Features.Billing.Queries.GetInvoicePdf;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace MechanicShop.Api.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this IEndpointRouteBuilder app, Asp.Versioning.Builder.ApiVersionSet apiVersionSet)
    {
        var endpoints = app.MapGroup("/api/v{apiVersion:apiVersion}/invoices")
               .WithApiVersionSet(apiVersionSet)
               .HasApiVersion(1.0)
               .WithOpenApi()
               .RequireAuthorization("ManagerOnly");

        endpoints.MapPost("/workorders/{workOrderId:guid}", IssueInvoice)
            .WithName("IssueInvoiceForWorkOrder")
            .MapToApiVersion(1.0)
            .WithSummary("Issues an invoice for a completed work order.")
            .WithDescription("Creates an invoice for the specified work order and returns the generated invoice.")
            .Produces<InvoiceDto>(StatusCodes.Status201Created)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/{invoiceId:guid}", GetInvoice)
            .WithName("GetInvoice")
            .MapToApiVersion(1.0)
            .WithSummary("Retrieves an invoice by ID.")
            .WithDescription("Returns detailed information about a specific invoice.")
            .Produces<InvoiceDto>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);

        endpoints.MapGet("/{invoiceId:guid}/pdf", GetInvoicePdf)
            .WithName("DownloadInvoicePdf")
            .MapToApiVersion(1.0)
            .WithSummary("Downloads the invoice as a PDF.")
            .WithDescription("Returns the PDF file associated with the given invoice ID.")
            .Produces(StatusCodes.Status200OK, contentType: "application/pdf")
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces<ProblemDetails>(StatusCodes.Status500InternalServerError);
    }

    private static async Task<IResult> GetInvoice(
   ISender sender,
   Guid invoiceId,
   CancellationToken ct)
    {
        var result = await sender.Send(new GetInvoiceByIdQuery(invoiceId), ct);
        return result.Match(
            invoice => Results.Ok(invoice),
            error => error.ToProblem());
    }

    private static async Task<IResult> IssueInvoice(
    ISender sender,
    Guid workOrderId,
    CancellationToken ct)
    {
        var command = new IssueInvoiceCommand(workOrderId);

        var result = await sender.Send(command, ct);

        return result.Match(
            invoice => Results.Created($"/api/v1/invoices/{invoice.InvoiceId}", invoice),
            error => error.ToProblem());
    }

    private static async Task<IResult> GetInvoicePdf(
    ISender sender,
    Guid invoiceId,
    CancellationToken ct)
    {
        var result = await sender.Send(new GetInvoicePdfQuery(invoiceId), ct);

        return result.Match(
            pdf =>
                Results.File(pdf.Content!, "application/pdf", pdf.FileName),
            error => error.ToProblem());
    }
}