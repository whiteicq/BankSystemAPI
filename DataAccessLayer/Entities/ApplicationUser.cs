using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace DataAccessLayer.Entities
{
    public class ApplicationUser : IdentityUser<long>
    {
        public long? ClientId { get; set; }
        public virtual Client? Client { get; set; }

        public long? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }
    }
}
