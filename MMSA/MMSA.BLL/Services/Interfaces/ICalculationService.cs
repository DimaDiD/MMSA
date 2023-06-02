namespace MMSA.BLL.Services.Interfaces
{
    public interface ICalculationService
    {
        public object GetVm(object funV0, object operatorValues, object operators, object scopes, object leftSide, object rightSide);
        public object GetCm(object vm, object leftSide, object rightSide);
        public object[] GetMu(object cm);
        public object GetUn(object vm, object cm, object mu);
        public object[] GetPlot(object un, object leftSide, object rightSide);
        public object GetMainResult(object cm, object mu);
    }
}
