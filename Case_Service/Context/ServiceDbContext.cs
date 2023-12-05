using Case_Service.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Case_Service.Context
{
    public class ServiceDbContext : DbContext
    {
        public DbSet<Word> Words { get; set; }
        public ServiceDbContext(DbContextOptions<ServiceDbContext> options) : base(options)
        {
        }
    }
}
