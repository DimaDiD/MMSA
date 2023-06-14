namespace MMSA.DAL.Dtos
{
    public class CalculationResultDto
    {
        public double[][] MU { get; set; }
        public double[] PlotXi { get; set; }
        public double[][][] PlotFXi { get; set; }

        public bool Error { get; set; }
    }
}