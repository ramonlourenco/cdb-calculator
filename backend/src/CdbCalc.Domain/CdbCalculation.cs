namespace CdbCalc.Domain;

public class CdbCalculation
{
    public decimal InitialValue { get; set; }
    public int Months { get; set; }
    public decimal GrossValue { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal NetValue { get; set; }

    public static CdbCalculation Calculate(decimal initialValue, int months)
    {
        const decimal cdi = 0.009m;
        const decimal taxBracket = 1.08m;
        decimal currentValue = initialValue;

        // Cálculo iterativo composto
        for (int i = 0; i < months; i++)
        {
            currentValue *= (1 + (cdi * taxBracket));
        }

        decimal grossValue = currentValue;
        decimal profit = grossValue - initialValue;
        decimal incomeTaxRate = GetIncomeTaxRate(months);
        decimal incomeTax = profit * incomeTaxRate;
        decimal netValue = grossValue - incomeTax;

        return new CdbCalculation
        {
            InitialValue = initialValue,
            Months = months,
            GrossValue = grossValue,
            IncomeTax = incomeTax,
            NetValue = netValue
        };
    }

    private static decimal GetIncomeTaxRate(int months)
    {
        return months switch
        {
            <= 6 => 0.225m,
            <= 12 => 0.20m,
            <= 24 => 0.175m,
            _ => 0.15m
        };
    }
}
