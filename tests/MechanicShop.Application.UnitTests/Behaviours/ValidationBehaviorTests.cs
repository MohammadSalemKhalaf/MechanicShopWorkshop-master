using FluentValidation;
using FluentValidation.Results;

using MechanicShop.Application.Common.Behaviours;
using MechanicShop.Application.Features.WorkOrders.Commands.CreateWorkOrder;
using MechanicShop.Application.Features.WorkOrders.Dtos;
using MechanicShop.Application.Features.WorkOrders.Mappers;
using MechanicShop.Domain.Common.Results;
using MechanicShop.Tests.Common.WorkOrders;

using MediatR;

using NSubstitute;

using Xunit;

namespace MechanicShop.Application.UnitTests.Behaviours;

public class ValidationBehaviorTests
{
    private readonly ValidationBehavior<CreateWorkOrderCommand, Result<WorkOrderDto>> _validationBehavior;
    private readonly IValidator<CreateWorkOrderCommand> _mockValidator;
    private readonly RequestHandlerDelegate<Result<WorkOrderDto>> _mockNextBehavior;

    public ValidationBehaviorTests()
    {
        _mockNextBehavior = Substitute.For<RequestHandlerDelegate<Result<WorkOrderDto>>>();
        _mockValidator = Substitute.For<IValidator<CreateWorkOrderCommand>>();

        _validationBehavior = new(_mockValidator);
    }

    [Fact]
    public async Task InvokeValidationBehavior_WhenValidatorResultIsValid_ShouldInvokeNextBehavior()
    {
        // Arrange
        var createWorkOrderCommand = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
        var workOrderResponse = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        _mockValidator
            .ValidateAsync(createWorkOrderCommand, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult());

        _mockNextBehavior.Invoke().Returns(workOrderResponse);

        // Act
        var result = await _validationBehavior.Handle(createWorkOrderCommand, _mockNextBehavior, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(workOrderResponse, result.Value);
    }

    [Fact]
    public async Task InvokeValidationBehavior_WhenValidatorResultIsNotValid_ShouldReturnListOfErrors()
    {
        // Arrange
        var createWorkOrderCommand = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();

        List<ValidationFailure> validationFailures = [new(propertyName: "property1", errorMessage: "property1 is invalid")];

        _mockValidator
            .ValidateAsync(createWorkOrderCommand, Arg.Any<CancellationToken>())
            .Returns(new ValidationResult(validationFailures));

        // Act
        var result = await _validationBehavior.Handle(createWorkOrderCommand, _mockNextBehavior, default);

        // Assert
        Assert.True(result.IsError);
        Assert.Equal("property1", result.TopError.Code);
        Assert.Equal("property1 is invalid", result.TopError.Description);
    }

    [Fact]
    public async Task InvokeValidationBehavior_WhenNoValidator_ShouldInvokeNextBehavior()
    {
        // Arrange
        var createWorkOrderCommand = WorkOrderCommandFactory.CreateCreateWorkOrderCommand();
        var validationBehavior = new ValidationBehavior<CreateWorkOrderCommand, Result<WorkOrderDto>>();

        var workOrder = WorkOrderFactory.CreateWorkOrder().Value;

        var workOrderResponse = WorkOrderFactory.CreateWorkOrder().Value.ToDto();

        _mockNextBehavior.Invoke().Returns(workOrderResponse);

        // Act
        var result = await validationBehavior.Handle(createWorkOrderCommand, _mockNextBehavior, default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(workOrderResponse, result.Value);
    }
}