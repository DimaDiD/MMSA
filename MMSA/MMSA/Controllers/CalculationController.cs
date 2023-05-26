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

        public CalculationController(ICalculationService calculationService)
        {
            _calculationService = calculationService;   
        }

        [HttpGet("GetVm")]
        public IActionResult GetVm([FromQuery] CalculationInputDto calculationInput)
        {
            var vm = _calculationService.GetVm(calculationInput.InputFunction, calculationInput.CalculationType);

            var cm = _calculationService.GetCm(vm);

            var mu = _calculationService.GetMu(cm);

            var un = _calculationService.GetUn(vm, cm, mu[0]);

            var plot = _calculationService.GetPlot(un, calculationInput.CalculationType);

            return Ok(new CalculationResultDto { MU = (double[][])mu[1], PlotXi = (double[])plot[0], PlotFXi = (double[][][])plot[1]});
        }
    }
}
