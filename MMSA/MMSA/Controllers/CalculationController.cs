using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.AspNetCore.Mvc;
using MMSA.BLL.Services.Interfaces;
using MMSA.DAL.Dtos;
using static System.Runtime.InteropServices.JavaScript.JSType;
using System.Globalization;

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

        [HttpGet("MakeCalculation")]
        public IActionResult GetVm([FromQuery] CalculationInputDto calculationInput)
        {
            var vm = _calculationService.GetVm(calculationInput.InputFunction, calculationInput.OperatorValues, calculationInput.Operators, calculationInput.Scopes, calculationInput.LeftSide, calculationInput.RightSide);

            var cm = _calculationService.GetCm(vm, calculationInput.LeftSide, calculationInput.RightSide);

            var muNewCm = _calculationService.GetMu(cm);

            var concreteRoots = _calculationService.GetMainResult(muNewCm[2], muNewCm[0]);

            var un = _calculationService.GetUn(vm, cm, concreteRoots);

            var plot = _calculationService.GetPlot(un, calculationInput.LeftSide, calculationInput.RightSide);

            return Ok(new CalculationResultDto { MU = (double[][])concreteRoots, PlotXi = (double[])plot[0], PlotFXi = (double[][][])plot[1] });
        }

        [HttpGet("GetFile")]
        public IActionResult GenerateExcelFile([FromQuery] TableData tableResults)
        {
            var data = new List<List<double>>();
            foreach (var item in tableResults.CalculationResults)
            {
                var lineData = item.Split(",");
                var rowList = new List<double>();

                foreach (var item2 in lineData)
                    rowList.Add(Double.Parse(item2, NumberStyles.Float, CultureInfo.InvariantCulture));
                
                data.Add(rowList);
            }

            byte[] excelBytes;
            using (var memoryStream = new MemoryStream())
            {
                using (var spreadsheetDocument = SpreadsheetDocument.Create(memoryStream, SpreadsheetDocumentType.Workbook))
                {
                    var workbookPart = spreadsheetDocument.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();

                    var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    var sheets = spreadsheetDocument.WorkbookPart.Workbook.AppendChild(new Sheets());

                    var sheet = new Sheet
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);

                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    for (var row = 0; row < data.Count; row++)
                    {
                        var rowData = data[row];

                        var rowElement = new Row();

                        for (var col = 0; col < rowData.Count; col++)
                        {
                            //var cellValue = ;
                            var cell = new Cell
                            {
                                DataType = CellValues.Number,
                                CellValue = new CellValue(rowData[col])
                            };

                            rowElement.Append(cell);
                        }

                        sheetData.Append(rowElement);
                    }
                }

                excelBytes = memoryStream.ToArray();
            }

            var base64String = Convert.ToBase64String(excelBytes);
            return Ok(base64String);
        }
    }
}
