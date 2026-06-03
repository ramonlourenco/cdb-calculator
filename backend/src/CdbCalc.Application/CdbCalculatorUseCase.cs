using CdbCalc.Domain;

namespace CdbCalc.Application;

public class CdbCalculatorUseCase : ICdbCalculatorUseCase
{
    public CdbCalculation Execute(decimal initialValue, int months)
    {
        return CdbCalculation.Calculate(initialValue, months);
    }
}
