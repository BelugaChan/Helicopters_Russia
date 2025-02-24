using AbstractionsAndModels.Interfaces.Models;
using Microsoft.EntityFrameworkCore;

namespace Helicopters_Russia.Context
{
    public class MyDbContext<TStandart> : DbContext
        where TStandart : class, IStandart
    {
        public MyDbContext(DbContextOptions<MyDbContext<TStandart>> options) : base(options)
        {
            
        }
        public DbSet<TStandart> Standarts { get; set; }

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    optionsBuilder.UseNpgsql(connectionString:
        //        "Server=localhost;Port=5432;User Id=postgres;Password=postgres;Database=helirusdb");
        //    base.OnConfiguring(optionsBuilder);
        //}
    }
}
