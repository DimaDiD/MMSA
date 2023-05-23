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
        public async Task<IActionResult> GetVm([FromQuery] CalculationInputDto calculationInput)
        {
            var vm = _calculationService.GetVm(calculationInput.InputFunction, calculationInput.CalculationType);

            var cm = _calculationService.GetCm(vm);

            var mu = _calculationService.GetMu(cm);

            var un = _calculationService.GetUn(vm, cm, mu[0]);

            var plot = _calculationService.GetPlot(un, "2");

            var f = (double[][])mu[1];
            var result = new CalculationResultDto { Plot = (string)plot, MU = f };
            return Ok(result);
        }
    }
}
