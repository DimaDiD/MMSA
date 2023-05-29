using System.ComponentModel.DataAnnotations;

namespace MMSA.DAL.Entities
{
    public class Calculation
    {
        [Key]
        public int Id { get; set; }
        public string MethodType { get; set; }
        public string InputFunction { get; set; }
        public int UserCalculationId { get; set; }
        public UserCalculation UserCalculation { get; set; }
    }
}
