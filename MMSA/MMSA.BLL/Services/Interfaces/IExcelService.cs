namespace MMSA.BLL.Services.Interfaces
{
    public interface IExcelService
    {
        public List<List<double>> ParseTableResults(List<string> stringTableResults);
        public string GetFile(List<List<double>> tableResults);
    }
}
