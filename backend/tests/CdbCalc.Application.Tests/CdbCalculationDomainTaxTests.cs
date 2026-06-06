using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CdbCalc.Domain.Tests;

[ExcludeFromCodeCoverage]
public class CdbCalculationDomainTaxTests
{
    [Theory(DisplayName = "Validar aplicação correta de alíquotas conforme tabela regressiva")]
    [InlineData(6, 0.225)]
    [InlineData(12, 0.20)]
    [InlineData(24, 0.175)]
    [InlineData(25, 0.15)]
    public void GetIncomeTaxRate_ReturnsCorrectRatePerTier(int months, double expectedRate)
    {
        var result = CdbCalculation.Calculate(1000m, months);
        var profit = result.GrossValue - 1000m;
        var expectedTax = profit * (decimal)expectedRate;

        Assert.Equal(expectedTax, result.IncomeTax);
    }
}
