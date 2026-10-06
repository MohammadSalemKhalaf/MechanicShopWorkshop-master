using MechanicShop.Tests.Common.Auth;

using Xunit;

namespace MechanicShop.Domain.UnitTests.Auth;

public class RefreshTokenTests
{
    [Fact]
    public void CreateRefreshToken_ShouldSucceed_WithValidData()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid().ToString();
        const string tokenValue = "token";
        var expiresOnUtc = DateTimeOffset.UtcNow.AddDays(7);

        var result = RefreshTokenFactory.CreateRefreshToken(id: id, token: tokenValue, userId: userId, expiresOnUtc: expiresOnUtc);

        Assert.True(result.IsSuccess);

        var token = result.Value;

        Assert.NotNull(token);
        Assert.Equal(tokenValue, token.Token);
        Assert.False(string.IsNullOrWhiteSpace(token.UserId));
        Assert.Equal(userId, token.UserId);
        Assert.True(token.ExpiresOnUtc > DateTimeOffset.UtcNow);
    }

    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenIdEmpty()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(id: Guid.Empty);

        Assert.True(result.IsError);

        Assert.Equal("RefreshToken_Id_Required", result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRefreshToken_ShouldFail_WhenTokenInvalid(string? invalidToken)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(token: invalidToken);

        Assert.True(result.IsError);

        Assert.Equal("RefreshToken_Token_Required", result.TopError.Code);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void CreateRefreshToken_ShouldFail_WhenUserIdInvalid(string? invalidUserId)
    {
        var result = RefreshTokenFactory.CreateRefreshToken(userId: invalidUserId);

        Assert.True(result.IsError);

        Assert.Equal("RefreshToken_UserId_Required", result.TopError.Code);
    }

    [Fact]
    public void CreateRefreshToken_ShouldFail_WhenExpiresOnUtcIsInPast()
    {
        var result = RefreshTokenFactory.CreateRefreshToken(expiresOnUtc: DateTimeOffset.UtcNow.AddMinutes(-1));

        Assert.True(result.IsError);

        Assert.Equal("RefreshToken_Expiry_Invalid", result.TopError.Code);
    }
}