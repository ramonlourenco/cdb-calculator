using CdbCalc.Domain;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CdbCalc.Domain.Tests;

[ExcludeFromCodeCoverage]
public class CdbCalculationDomainTests
{
    [Theory(DisplayName = "Validar precisão matemática absoluta com referências 1000/6 e 1000/12")]
    [MemberData(nameof(GetCalculationTestData))]
    public void Calculate_WithReferenceValues_ReturnsExactValues(decimal initialValue, int months, decimal expectedGross, decimal expectedNet)
    {
        // Act
        var result = CdbCalculation.Calculate(initialValue, months);

        // Assert - Comparação exata, byte a byte, sem arredondamento
        Assert.NotNull(result);
        Assert.Equal(expectedGross, result.GrossValue);
        Assert.Equal(expectedNet, result.NetValue);

        // Assertions independentes (Prova real)
        Assert.Equal(result.GrossValue - result.IncomeTax, result.NetValue);
    }

    public static IEnumerable<object[]> GetCalculationTestData()
    {
        yield return new object[] {
            1000m, 6,
            1059.7556770148984501188388823m,
            1046.3106496865462988421001338m
        };
        yield return new object[] {
            1000m, 12,
            1123.0820949653057631841036240m,
            1098.4656759722446105472828992m
        };
    }

    [Fact(DisplayName = "Validar que valores não são arredondados para evitar erros progressivos")]
    public void Calculate_ValuesAreNotRounded()
    {
        var initialValue = 5000m;
        var months = 7;
        var result = CdbCalculation.Calculate(initialValue, months);

        decimal expectedTax = 70.056502195483263053993996240m;
        decimal expectedCalc = result.GrossValue - result.NetValue;

        Assert.Equal(expectedTax, result.IncomeTax);
    }
}
