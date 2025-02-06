using Microsoft.EntityFrameworkCore;
//using Contract.Repositories.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Repositories.Base
{
    public class FHealthSphereDBContext : IdentityDbContext
    {
        public FHealthSphereDBContext() { }
        public FHealthSphereDBContext(DbContextOptions<FHealthSphereDBContext> options) : base(options) { }
    }
}
