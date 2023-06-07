using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using Microsoft.Extensions.Logging;
using MMSA.BLL.Services.Interfaces;
using System.Globalization;

namespace MMSA.BLL.Services.Implementation
{
    public class ExcelService: IExcelService
    {
        private readonly ILogger _logger;
        public ExcelService(ILogger<CalculationService> logger)
        {
            _logger = logger;
        }

        public List<List<double>> ParseTableResults(List<string> stringTableResults)
        {
            _logger.LogInformation("Start parcing...");

            var data = new List<List<double>>();
            foreach (var row in stringTableResults)
            {
                var stringNumbers = row.Split(",");
                var rowList = new List<double>();

                foreach (var number in stringNumbers)
                    rowList.Add(Double.Parse(number, NumberStyles.Float, CultureInfo.InvariantCulture));

                data.Add(rowList);
            }

            _logger.LogInformation("return results...");
            return data;
        }

        public string GetFile(List<List<double>> tableResults)
        {
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

                    for (var row = 0; row < tableResults.Count; row++)
                    {
                        var rowData = tableResults[row];

                        var rowElement = new Row();

                        for (var col = 0; col < rowData.Count; col++)
                        {
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

            return (Convert.ToBase64String(excelBytes));
        }
    }
}
