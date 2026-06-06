using CdbCalc.Domain;

namespace CdbCalc.Application;

public class CdbCalculatorUseCase : ICdbCalculatorUseCase
{
    public CdbCalculation Execute(decimal initialValue, int months)
    {
        if (initialValue <= 0)
            throw new ArgumentException("Initial value must be greater than zero.", nameof(initialValue));

        if (months <= 1)
            throw new ArgumentException("Months must be greater than 1.", nameof(months));

        return CdbCalculation.Calculate(initialValue, months);
    }
}
