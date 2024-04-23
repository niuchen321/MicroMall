using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Entitys;

namespace MicroMall.Data
{
    public class MicroMallContext : DbContext
    {
        public MicroMallContext (DbContextOptions<MicroMallContext> options)
            : base(options)
        {
        }

        public DbSet<Entitys.GoodsEntity> GoodsEntity { get; set; } = default!;
        public DbSet<Entitys.Book> Book { get; set; } = default!;
    }
}
