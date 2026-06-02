using CdbCalc.Application;
using CdbCalc.Domain;
using Xunit;

namespace CdbCalc.Application.Tests;

public class CdbCalculatorUseCaseTests
{
    private readonly CdbCalculatorUseCase _useCase = new();

    [Fact]
    public void Execute_WithValidInputs_ReturnsCalculation()
    {
        var result = _useCase.Execute(1000m, 12);

        Assert.NotNull(result);
        Assert.Equal(1000m, result.InitialValue);
        Assert.Equal(12, result.Months);
        Assert.True(result.GrossValue > result.InitialValue);
        Assert.True(result.IncomeTax > 0);
        Assert.Equal(result.GrossValue - result.IncomeTax, result.NetValue);
    }

    [Fact]
    public void Execute_ZeroInitialValue_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(0m, 12));
    }

    [Fact]
    public void Execute_NegativeInitialValue_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(-100m, 12));
    }

    [Fact]
    public void Execute_ZeroMonths_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(1000m, 0));
    }

    [Fact]
    public void Execute_NegativeMonths_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(1000m, -5));
    }

    [Theory]
    [InlineData(1000m, 3, 0.225)]
    [InlineData(1000m, 6, 0.225)]
    public void Execute_UpTo6Months_AppliesTaxRate22_5Percent(decimal initialValue, int months, decimal expectedTaxRate)
    {
        var result = _useCase.Execute(initialValue, months);
        var profit = result.GrossValue - initialValue;
        var expectedTax = Math.Round(profit * (decimal)expectedTaxRate, 2);

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Theory]
    [InlineData(1000m, 7, 0.20)]
    [InlineData(1000m, 12, 0.20)]
    public void Execute_7To12Months_AppliesTaxRate20Percent(decimal initialValue, int months, decimal expectedTaxRate)
    {
        var result = _useCase.Execute(initialValue, months);
        var profit = result.GrossValue - initialValue;
        var expectedTax = Math.Round(profit * (decimal)expectedTaxRate, 2);

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Theory]
    [InlineData(1000m, 13, 0.175)]
    [InlineData(1000m, 24, 0.175)]
    public void Execute_13To24Months_AppliesTaxRate17_5Percent(decimal initialValue, int months, decimal expectedTaxRate)
    {
        var result = _useCase.Execute(initialValue, months);
        var profit = result.GrossValue - initialValue;
        var expectedTax = Math.Round(profit * (decimal)expectedTaxRate, 2);

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Theory]
    [InlineData(1000m, 25, 0.15)]
    [InlineData(1000m, 60, 0.15)]
    public void Execute_Above24Months_AppliesTaxRate15Percent(decimal initialValue, int months, decimal expectedTaxRate)
    {
        var result = _useCase.Execute(initialValue, months);
        var profit = result.GrossValue - initialValue;
        var expectedTax = Math.Round(profit * (decimal)expectedTaxRate, 2);

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Fact]
    public void Execute_LargeValue_CalculatesCorrectly()
    {
        var result = _useCase.Execute(100000m, 24);

        Assert.True(result.GrossValue > 100000m);
        Assert.True(result.NetValue > 100000m);
        Assert.True(result.IncomeTax > 0);
        Assert.True(result.NetValue < result.GrossValue);
    }

    [Fact]
    public void Execute_SingleMonth_CalculatesCorrectly()
    {
        var result = _useCase.Execute(10000m, 1);

        Assert.True(result.GrossValue > 10000m);
        var profit = result.GrossValue - 10000m;
        var expectedTax = Math.Round(profit * 0.225m, 2);

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Fact]
    public void Execute_ValuesAreRoundedToTwoDecimals()
    {
        var result = _useCase.Execute(5000m, 7);

        var grossStr = result.GrossValue.ToString("F2");
        var taxStr = result.IncomeTax.ToString("F2");
        var netStr = result.NetValue.ToString("F2");

        Assert.DoesNotContain(".", grossStr.Substring(grossStr.IndexOf(".") + 1));
        Assert.True(grossStr.Split('.')[1].Length <= 2);
        Assert.True(taxStr.Split('.')[1].Length <= 2);
        Assert.True(netStr.Split('.')[1].Length <= 2);
    }

    [Fact]
    public void Execute_NetValueEqualsGrossMinusTax()
    {
        var result = _useCase.Execute(15000m, 18);

        var expected = Math.Round(result.GrossValue - result.IncomeTax, 2);
        Assert.Equal(expected, result.NetValue);
    }
}
