using Microsoft.AspNetCore.Identity;

namespace MMSA.DAL.Entities
{
    public class User: IdentityUser
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
}
