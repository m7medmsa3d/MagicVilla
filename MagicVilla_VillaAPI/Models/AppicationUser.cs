using Microsoft.AspNetCore.Identity;

namespace MagicVilla_VillaAPI.Models
{
    public class AppicationUser : IdentityUser
    {
        public string Name { get; set; }
      
    }
}
