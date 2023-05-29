using System.ComponentModel.DataAnnotations;

namespace MMSA.DAL.Entities
{
    public class UserCalculation
    {
        [Key]
        public int Id { get; set; } 
        public int UserId { get; set; }
        public List<Calculation> userCalculations { get; set; }
    }
}
