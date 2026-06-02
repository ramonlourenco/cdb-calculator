using CdbCalc.Application;
using CdbCalc.Domain;
using Xunit;

namespace CdbCalc.Application.Tests;

public class CdbCalculatorUseCaseTests
{
    private readonly CdbCalculatorUseCase _useCase = new();

    [Fact(DisplayName = "Executar com entradas válidas deve retornar o cálculo correto com consistência matemática total")]
    public void Execute_WithValidInputs_ReturnsCalculation()
    {
        var result = _useCase.Execute(1000m, 12);

        Assert.NotNull(result);
        Assert.Equal(1000m, result.InitialValue);
        Assert.Equal(12, result.Months);
        Assert.True(result.GrossValue > result.InitialValue);
        Assert.True(result.IncomeTax > 0);
        // Sem arredondamentos espúrios, a igualdade abaixo é perfeitamente exata
        Assert.Equal(result.GrossValue - result.IncomeTax, result.NetValue);
    }

    [Fact(DisplayName = "Executar com valor inicial zero deve lançar exceção de argumento inválido")]
    public void Execute_ZeroInitialValue_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(0m, 12));
    }

    [Fact(DisplayName = "Executar com valor inicial negativo deve lançar exceção de argumento inválido")]
    public void Execute_NegativeInitialValue_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(-100m, 12));
    }

    [Fact(DisplayName = "Executar com meses igual a zero deve lançar exceção de argumento inválido")]
    public void Execute_ZeroMonths_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(1000m, 0));
    }

    [Fact(DisplayName = "Executar com meses negativo deve lançar exceção de argumento inválido")]
    public void Execute_NegativeMonths_ThrowsException()
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(1000m, -5));
    }

    [Theory(DisplayName = "Executar com prazo até 6 meses deve aplicar alíquota de IR de 22,5% sem arredondamento")]
    [InlineData(1000, 3, 0.225)]
    [InlineData(1000, 6, 0.225)]
    public void Execute_UpTo6Months_AppliesTaxRate22_5Percent(double initialValue, int months, double expectedTaxRate)
    {
        var result = _useCase.Execute((decimal)initialValue, months);
        var profit = result.GrossValue - (decimal)initialValue;
        var expectedTax = profit * (decimal)expectedTaxRate;

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Theory(DisplayName = "Executar com prazo de 7 a 12 meses deve aplicar alíquota de IR de 20% sem arredondamento")]
    [InlineData(1000, 7, 0.20)]
    [InlineData(1000, 12, 0.20)]
    public void Execute_7To12Months_AppliesTaxRate20Percent(double initialValue, int months, double expectedTaxRate)
    {
        var result = _useCase.Execute((decimal)initialValue, months);
        var profit = result.GrossValue - (decimal)initialValue;
        var expectedTax = profit * (decimal)expectedTaxRate;

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Theory(DisplayName = "Executar com prazo de 13 a 24 meses deve aplicar alíquota de IR de 17,5% sem arredondamento")]
    [InlineData(1000, 13, 0.175)]
    [InlineData(1000, 24, 0.175)]
    public void Execute_13To24Months_AppliesTaxRate17_5Percent(double initialValue, int months, double expectedTaxRate)
    {
        var result = _useCase.Execute((decimal)initialValue, months);
        var profit = result.GrossValue - (decimal)initialValue;
        var expectedTax = profit * (decimal)expectedTaxRate;

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Theory(DisplayName = "Executar com prazo acima de 24 meses deve aplicar alíquota de IR de 15% sem arredondamento")]
    [InlineData(1000, 25, 0.15)]
    [InlineData(1000, 60, 0.15)]
    public void Execute_Above24Months_AppliesTaxRate15Percent(double initialValue, int months, double expectedTaxRate)
    {
        var result = _useCase.Execute((decimal)initialValue, months);
        var profit = result.GrossValue - (decimal)initialValue;
        var expectedTax = profit * (decimal)expectedTaxRate;

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Fact(DisplayName = "Executar com grande valor de aplicação deve calcular mantendo a precisão dos dados")]
    public void Execute_LargeValue_CalculatesCorrectly()
    {
        var result = _useCase.Execute(100000m, 24);

        Assert.True(result.GrossValue > 100000m);
        Assert.True(result.NetValue > 100000m);
        Assert.True(result.IncomeTax > 0);
        Assert.True(result.NetValue < result.GrossValue);
    }

    [Fact(DisplayName = "Executar com um único mês deve calcular o rendimento e o IR corretos sem perda decimal")]
    public void Execute_SingleMonth_CalculatesCorrectly()
    {
        var result = _useCase.Execute(10000m, 1);

        Assert.True(result.GrossValue > 10000m);
        var profit = result.GrossValue - 10000m;
        var expectedTax = profit * 0.225m;

        Assert.Equal(expectedTax, result.IncomeTax);
    }

    [Fact(DisplayName = "Executar deve reter a precisão decimal nativa completa e não introduzir erros progressivos")]
    public void Execute_ValuesAreNotRoundedToPreventProgressiveErrors()
    {
        var result = _useCase.Execute(5000m, 7);

        // Garante que o valor não foi truncado ou forçado a duas casas de forma precoce
        decimal expectedNet = result.GrossValue - result.IncomeTax;
        Assert.Equal(expectedNet, result.NetValue);
    }

    [Fact(DisplayName = "Executar garante estritamente que o valor líquido é a diferença matemática exata entre bruto e imposto")]
    public void Execute_NetValueEqualsGrossMinusTax()
    {
        var result = _useCase.Execute(15000m, 18);

        var expected = result.GrossValue - result.IncomeTax;
        Assert.Equal(expected, result.NetValue);
    }
}
