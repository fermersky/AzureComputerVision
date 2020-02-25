using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CloudImageStorage.Models
{
    public class ImagesDbContext : DbContext
    {
        public DbSet<Image> Images { get; set; }

        public ImagesDbContext(DbContextOptions<ImagesDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Image>()
            .Property(e => e.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries));

            base.OnModelCreating(modelBuilder);
        }
    }
}
