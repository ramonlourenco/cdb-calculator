using CdbCalc.Adapters.Primary.WebApi.Dtos;
using CdbCalc.Domain;
using Microsoft.AspNetCore.Mvc;

namespace CdbCalc.Adapters.Primary.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CdbCalculatorController : ControllerBase
{
    private readonly ICdbCalculatorUseCase _useCase;
    private readonly ILogger<CdbCalculatorController> _logger;

    public CdbCalculatorController(ICdbCalculatorUseCase useCase, ILogger<CdbCalculatorController> logger)
    {
        _useCase = useCase;
        _logger = logger;
    }

    [HttpPost("calculate")]
    public IActionResult Calculate([FromBody] CdbCalculationRequest request)
    {
        try
        {
            var calculation = _useCase.Execute(request.InitialValue, request.Months);

            var response = new CdbCalculationResponse
            {
                InitialValue = calculation.InitialValue,
                Months = calculation.Months,
                GrossValue = calculation.GrossValue,
                IncomeTax = calculation.IncomeTax,
                NetValue = calculation.NetValue
            };

            _logger.LogInformation("CDB calculation executed successfully");
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Invalid request: {ex.Message}");
            return BadRequest(new { error = ex.Message });
        }
    }
}
