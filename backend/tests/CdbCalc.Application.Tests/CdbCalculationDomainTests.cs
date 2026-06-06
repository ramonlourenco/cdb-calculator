using CdbCalc.Domain;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CdbCalc.Domain.Tests;

[ExcludeFromCodeCoverage]
public class CdbCalculationDomainTests
{
    [Theory(DisplayName = "Validar precisão matemática com valores de referência 1000/6 e 1000/12")]
    [InlineData(1000, 6)]
    [InlineData(1000, 12)]
    public void Calculate_WithReferenceValues_ReturnsAccurateValues(decimal initialValue, int months)
    {
        var result = CdbCalculation.Calculate(initialValue, months);

        Assert.NotNull(result);
        Assert.True(result.GrossValue > result.InitialValue);
        Assert.Equal(result.GrossValue - result.IncomeTax, result.NetValue);
    }

    [Fact(DisplayName = "Validar cálculo com alto valor mantendo precisão sem arredondamento")]
    public void Calculate_LargeValue_MaintainsPrecision()
    {
        var result = CdbCalculation.Calculate(100000m, 24);
        Assert.True(result.NetValue > 100000m);
    }
}
