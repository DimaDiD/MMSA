namespace MMSA.BLL.Services.Interfaces
{
    public interface ICalculationService
    {
        public object GetVm(object funV0, object GreenFunction);
        public object GetCm(object vm);
        public object[] GetMu(object cm);
        public object GetUn(object vm, object cm, object mu);
        public object[] GetPlot(object un, object GreenFunction);
        //public List<string> GetVm(object funV0, object GreenFunction);
    }
}
