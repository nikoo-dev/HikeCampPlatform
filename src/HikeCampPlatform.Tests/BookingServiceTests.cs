using Xunit;
using HikeCampPlatform.Api.Services;

namespace HikeCampPlatform.Tests;

public class BookingServiceTests
{
    private readonly BookingService _sut; // "sut" = System Under Test, a common naming convention

    public BookingServiceTests()
    {
        _sut = new BookingService();
    }

    [Fact]
    public void CalculateTotalPrice_MultipliesPriceByParticipants()
    {
        // Arrange
        decimal pricePerPerson = 450.00m;
        int participants = 3;

        // Act
        var result = _sut.CalculateTotalPrice(pricePerPerson, participants);

        // Assert
        Assert.Equal(1350.00m, result);
    }

    [Fact]
    public void CalculateTotalPrice_SingleParticipant_ReturnsBasePrice()
    {
        var result = _sut.CalculateTotalPrice(450.00m, 1);
        Assert.Equal(450.00m, result);
    }

    [Fact]
    public void CalculateTotalPrice_ZeroParticipants_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.CalculateTotalPrice(450.00m, 0));
    }

    [Fact]
    public void CalculateTotalPrice_NegativeParticipants_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _sut.CalculateTotalPrice(450.00m, -2));
    }

    [Theory]
    [InlineData(12, 0, 5, true)]   // 12 max, 0 booked, asking for 5 -> fits
    [InlineData(12, 10, 2, true)]  // 12 max, 10 booked, asking for 2 -> exactly fits
    [InlineData(12, 10, 3, false)] // 12 max, 10 booked, asking for 3 -> doesn't fit
    [InlineData(12, 12, 1, false)] // fully booked -> nothing fits
    public void HasEnoughCapacity_ReturnsExpectedResult(int max, int current, int requested, bool expected)
    {
        var result = _sut.HasEnoughCapacity(max, current, requested);
        Assert.Equal(expected, result);
    }

    [Fact]
    public void CalculateSpotsRemaining_ReturnsCorrectDifference()
    {
        var result = _sut.CalculateSpotsRemaining(maxParticipants: 12, currentParticipants: 7);
        Assert.Equal(5, result);
    }
}