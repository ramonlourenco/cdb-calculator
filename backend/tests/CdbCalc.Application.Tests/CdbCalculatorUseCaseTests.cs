using CdbCalc.Application;
using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace CdbCalc.Application.Tests;

[ExcludeFromCodeCoverage]
public class CdbCalculatorUseCaseTests
{
    private readonly CdbCalculatorUseCase _useCase = new();

    [Theory(DisplayName = "Validar que valores iniciais menores ou iguais a zero lançam exceção")]
    [InlineData(0)]
    [InlineData(-100)]
    public void Execute_InvalidInitialValue_ThrowsArgumentException(decimal initialValue)
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(initialValue, 12));
    }

    [Theory(DisplayName = "Validar que meses menores ou iguais a um lançam exceção")]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-5)]
    public void Execute_InvalidMonths_ThrowsArgumentException(int months)
    {
        Assert.Throws<ArgumentException>(() => _useCase.Execute(1000m, months));
    }

    [Fact(DisplayName = "Validar que valores válidos não lançam exceção e retornam cálculo")]
    public void Execute_ValidInputs_ReturnsCalculation()
    {
        var result = _useCase.Execute(1000m, 12);

        Assert.NotNull(result);
        Assert.Equal(1000m, result.InitialValue);
    }
}
