using MechanicShop.Domain.Workorders.Billing;

using Xunit;

namespace MechanicShop.Domain.UnitTests.WorkOrders.Billing;

public class InvoiceLineItemTests
{
    [Fact]
    public void Create_WithValidData_ShouldSucceed()
    {
        var invoiceId = Guid.NewGuid();
        const int lineNumber = 1;
        const string description = "Brake Pad";
        const int quantity = 2;
        const decimal unitPrice = 50m;

        var result = InvoiceLineItem.Create(invoiceId, lineNumber, description, quantity, unitPrice);

        Assert.True(result.IsSuccess);
        var item = result.Value;
        Assert.Equal(invoiceId, item.InvoiceId);
        Assert.Equal(lineNumber, item.LineNumber);
        Assert.Equal(description, item.Description);
        Assert.Equal(quantity, item.Quantity);
        Assert.Equal(unitPrice, item.UnitPrice);
        Assert.Equal(100m, item.LineTotal);
    }

    [Fact]
    public void Create_WithEmptyInvoiceId_ShouldFail()
    {
        var result = InvoiceLineItem.Create(Guid.Empty, 1, "Item", 1, 10m);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceLineItemErrors.InvoiceIdRequired.Code, result.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.InvoiceIdRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_WithInvalidLineNumber_ShouldFail()
    {
        var result = InvoiceLineItem.Create(Guid.NewGuid(), 0, "Item", 1, 10m);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceLineItemErrors.LineNumberInvalid.Code, result.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.LineNumberInvalid.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_WithEmptyDescription_ShouldFail()
    {
        var result = InvoiceLineItem.Create(Guid.NewGuid(), 1, " ", 1, 10m);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceLineItemErrors.DescriptionRequired.Code, result.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.DescriptionRequired.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_WithInvalidQuantity_ShouldFail()
    {
        var result = InvoiceLineItem.Create(Guid.NewGuid(), 1, "Item", 0, 10m);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceLineItemErrors.QuantityInvalid.Code, result.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.QuantityInvalid.Description, result.TopError.Description);
    }

    [Fact]
    public void Create_WithInvalidUnitPrice_ShouldFail()
    {
        var result = InvoiceLineItem.Create(Guid.NewGuid(), 1, "Item", 1, 0m);

        Assert.True(result.IsError);
        Assert.Equal(InvoiceLineItemErrors.UnitPriceInvalid.Code, result.TopError.Code);
        Assert.Equal(InvoiceLineItemErrors.UnitPriceInvalid.Description, result.TopError.Description);
    }
}