namespace CdbCalc.Adapters.Primary.WebApi.Dtos;

public class CdbCalculationRequest
{
    public decimal InitialValue { get; set; }
    public int Months { get; set; }
}

public class CdbCalculationResponse
{
    public decimal InitialValue { get; set; }
    public int Months { get; set; }
    public decimal GrossValue { get; set; }
    public decimal IncomeTax { get; set; }
    public decimal NetValue { get; set; }
}
