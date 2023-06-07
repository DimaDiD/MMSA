using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Dtos;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculationController : ControllerBase
    {
        private readonly ICalculationService _calculationService;
        private readonly IExcelService _excelService;

        public CalculationController(ICalculationService calculationService, IExcelService excelService)
        {
            _calculationService = calculationService;
            _excelService = excelService;
        }

        [HttpGet("MakeCalculation")]
        public IActionResult MakeCalculation([FromQuery] CalculationInputDto calculationInput)
        {
            var product = _calculationService.MakeCalculation(calculationInput);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpGet("GetFile")]
        public IActionResult GenerateExcelFile([FromQuery] TableData tableResults)
        {
            var parsedTableResults = _excelService.ParseTableResults(tableResults.CalculationResults);
            
            return Ok(_excelService.GetFile(parsedTableResults));
        }
    }
}
