namespace CdbCalc.Domain;

public interface ICdbCalculatorUseCase
{
    CdbCalculation Execute(decimal initialValue, int months);
}
