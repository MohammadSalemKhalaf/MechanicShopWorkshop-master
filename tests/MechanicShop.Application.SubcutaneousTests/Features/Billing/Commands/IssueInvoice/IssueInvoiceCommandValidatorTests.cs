using FluentValidation.TestHelper;

using MechanicShop.Application.Features.Billing.Commands.IssueInvoice;

using Xunit;

namespace MechanicShop.Application.SubcutaneousTests.Features.Billing.Commands.IssueInvoice;

public class IssueInvoiceCommandValidatorTests
{
    private readonly IssueInvoiceCommandValidator _validator = new();

    [Fact]
    public void Should_Have_Error_When_WorkOrderId_Is_Empty()
    {
        // Arrange
        var command = new IssueInvoiceCommand(Guid.Empty);

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(c => c.WorkOrderId)
              .WithErrorCode("WorkOrderId_Is_Required")
              .WithErrorMessage("WorkOrderId is required.");
    }

    [Fact]
    public void Should_Not_Have_Error_When_WorkOrderId_Is_Valid()
    {
        // Arrange
        var command = new IssueInvoiceCommand(Guid.NewGuid());

        // Act & Assert
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(c => c.WorkOrderId);
    }
}