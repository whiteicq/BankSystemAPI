using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Entities
{
    public class ApplicationUser : IdentityUser<long>
    {
        public virtual Client? Client { get; set; }

        public virtual Employee? Employee { get; set; }
    }
}
