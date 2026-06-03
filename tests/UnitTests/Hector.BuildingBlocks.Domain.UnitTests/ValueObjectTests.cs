using FluentAssertions;
using Hector.BuildingBlocks.Domain.Primitives;

namespace Hector.BuildingBlocks.Domain.UnitTests;

public sealed class ValueObjectTests
{
    [Fact]
    public void Should_ReturnTrue_When_ValueObjectHaveSameValues()
    {
        // Arrange
        var address1 = new Address("Tehran", "Valiasr");
        var address2 = new Address("Tehran", "Valiasr");

        // Act
        var result = address1 == address2;
    
        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Should_ReturnFalse_When_ValueObjectsHaveDiffrenteValues()
    {
        // Arrange
        var address1 = new Address("Tehran", "Valiasr");
        var address2 = new Address("Tehran", "Tajrish");

        // Act
        var result = address1 == address2;
    
        // Assert
        result.Should().BeFalse();
    }

    internal class Address : ValueObject
    {
        public string City { get; }
        public string Street { get; }

        public Address(string city, string street)
        {
            City = city;
            Street = street;
        }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return City;
            yield return Street;
        }
    }
}