using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
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
            var vm = _calculationService.GetVm(calculationInput.InputFunction, calculationInput.OperatorValues, calculationInput.Operators, calculationInput.Scopes, calculationInput.LeftSide, calculationInput.RightSide);

            var cm = _calculationService.GetCm(vm, calculationInput.LeftSide, calculationInput.RightSide);

            var muNewCm = _calculationService.GetMu(cm);

            var concreteRoots = _calculationService.GetMainResult(muNewCm[2], muNewCm[0]);

            var un = _calculationService.GetUn(vm, cm, concreteRoots);

            var plot = _calculationService.GetPlot(un, calculationInput.LeftSide, calculationInput.RightSide);

            return Ok(new CalculationResultDto { MU = (double[][])concreteRoots, PlotXi = (double[])plot[0], PlotFXi = (double[][][])plot[1] });
        }

        [HttpGet("GetFile")]
        public IActionResult GenerateExcelFile()
        {
            var data = new double[][]
{
    new double[] { 12.1, 3.2 },
    new double[] { 4, 6 },
    new double[] { 7, 8, 9 }
};

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

                    // Add a new worksheet
                    var sheet = new Sheet
                    {
                        Id = spreadsheetDocument.WorkbookPart.GetIdOfPart(worksheetPart),
                        SheetId = 1,
                        Name = "Sheet1"
                    };
                    sheets.Append(sheet);

                    var sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    // Populate the cells with data
                    for (var row = 0; row < data.Length; row++)
                    {
                        var rowData = data[row];

                        var rowElement = new Row();

                        for (var col = 0; col < rowData.Length; col++)
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
            //return File(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "data.xlsx");
        }
    }
}
