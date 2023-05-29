using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Dtos;
using MMSA.DAL.Entities;

namespace MMSA.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CalculationController : ControllerBase
    {
        private readonly ICalculationService _calculationService;
        private readonly RepositoryContext _repositoryContext;

        public CalculationController(ICalculationService calculationService, RepositoryContext repositoryContext)
        {
            _calculationService = calculationService;   
            _repositoryContext = repositoryContext;
        }

        [HttpGet("GetVm")]
        public IActionResult GetVm([FromQuery] CalculationInputDto calculationInput)
        {
            //var a = _repositoryContext.Calculations.FirstOrDefault();
            var vm = _calculationService.GetVm(calculationInput.InputFunction, calculationInput.OperatorValues, calculationInput.Operators, calculationInput.Scopes, calculationInput.LeftSide, calculationInput.RightSide);

            var cm = _calculationService.GetCm(vm, calculationInput.LeftSide, calculationInput.RightSide);

            var mu = _calculationService.GetMu(cm);

            var un = _calculationService.GetUn(vm, cm, mu[0]);

            var plot = _calculationService.GetPlot(un);

            var res = _calculationService.GetMainResult(cm, mu);

            return Ok(new CalculationResultDto { MU = (double[][])mu[1], PlotXi = (double[])plot[0], PlotFXi = (double[][][])plot[1]});
        }
    }
}
