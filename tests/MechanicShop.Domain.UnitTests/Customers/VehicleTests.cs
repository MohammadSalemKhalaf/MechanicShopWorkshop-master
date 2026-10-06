using MechanicShop.Tests.Common.Customers;

using Xunit;

namespace MechanicShop.Domain.UnitTests.Customers;

public class VehicleTests
{
    [Fact]
    public void CreateVehicle_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        const string make = "Honda";
        const string model = "Accord";
        const int year = 2024;
        const string licensePlate = "ABC 123";

        var result = VehicleFactory.CreateVehicle(id: id, make: make, model: model, year: year, licensePlate: licensePlate);

        Assert.True(result.IsSuccess);

        var vehicle = result.Value;
        Assert.Equal(make, vehicle.Make);
        Assert.Equal(model, vehicle.Model);
        Assert.Equal(year, vehicle.Year);
        Assert.Equal(licensePlate, vehicle.LicensePlate);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenMakeInvalid(string make)
    {
        var result = VehicleFactory.CreateVehicle(make: make);
        Assert.True(result.IsError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenModelInvalid(string model)
    {
        var result = VehicleFactory.CreateVehicle(model: model);
        Assert.True(result.IsError);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateVehicle_ShouldFail_WhenLicensePlateInvalid(string plate)
    {
        var result = VehicleFactory.CreateVehicle(licensePlate: plate);
        Assert.True(result.IsError);
    }

    [Theory]
    [InlineData(1700)]
    [InlineData(3000)]
    public void CreateVehicle_ShouldFail_WhenYearInvalid(int year)
    {
        var result = VehicleFactory.CreateVehicle(year: year);
        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateVehicle_ShouldSucceed_WithValidData()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Toyota", "Camry", 2022, "XYZ 789");

        Assert.True(result.IsSuccess);
        Assert.Equal("Toyota", vehicle.Make);
        Assert.Equal("Camry", vehicle.Model);
        Assert.Equal(2022, vehicle.Year);
        Assert.Equal("XYZ 789", vehicle.LicensePlate);
    }

    [Fact]
    public void UpdateVehicle_ShouldFail_WhenMakeIsInvalid()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update(string.Empty, "Model", 2022, "XYZ123");

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateVehicle_ShouldFail_WhenModelIsInvalid()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Make", string.Empty, 2022, "XYZ123");

        Assert.True(result.IsError);
    }

    [Fact]
    public void UpdateVehicle_ShouldFail_WhenLicensePlateIsInvalid()
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Make", "Model", 2022, string.Empty);

        Assert.True(result.IsError);
    }

    [Theory]
    [InlineData(1800)]
    [InlineData(5000)]
    public void UpdateVehicle_ShouldFail_WhenYearInvalid(int year)
    {
        var vehicle = VehicleFactory.CreateVehicle().Value;

        var result = vehicle.Update("Make", "Model", year, "XYZ123");

        Assert.True(result.IsError);
    }

    [Fact]
    public void VehicleInfo_ShouldReturnFormattedString()
    {
        var vehicle = VehicleFactory.CreateVehicle(make: "Ford", model: "Mustang", year: 2021).Value;

        Assert.Equal("Ford | Mustang | 2021", vehicle.VehicleInfo);
    }
}