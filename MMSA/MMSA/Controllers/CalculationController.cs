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
            //var vm = _calculationService.GetVm(calculationInput.InputFunction, calculationInput.OperatorValues, calculationInput.Operators, calculationInput.Scopes, calculationInput.LeftSide, calculationInput.RightSide);

            //var cm = _calculationService.GetCm(vm, calculationInput.LeftSide, calculationInput.RightSide);

            //var muNewCm = _calculationService.GetMu(cm);

            //var concreteRoots = _calculationService.GetMainResult(muNewCm[2], muNewCm[0]);

            //var un = _calculationService.GetUn(vm, cm, concreteRoots);

            //var plot = _calculationService.GetPlot(un, calculationInput.LeftSide, calculationInput.RightSide);

            //return Ok(new CalculationResultDto { MU = (double[][])concreteRoots, PlotXi = (double[])plot[0], PlotFXi = (double[][][])plot[1] });
        }

        [HttpGet("GetFile")]
        public IActionResult GenerateExcelFile([FromQuery] TableData tableResults)
        {
            var parsedTableResults = _excelService.ParseTableResults(tableResults.CalculationResults);
            
            return Ok(_excelService.GetFile(parsedTableResults));
        }
    }
}
