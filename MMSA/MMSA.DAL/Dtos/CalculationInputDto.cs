namespace MMSA.DAL.Dtos
{
    public class CalculationInputDto
    {
        public string InputFunction { get; set; }
        public List<string> OperatorValues { get; set; }
        public List<string> Operators { get; set; }
        public List<string> Scopes { get; set; }
        public string LeftSide { get; set; }
        public string RightSide { get; set; }
    }
}
