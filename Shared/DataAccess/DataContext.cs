using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using Shared.Entities;

namespace Shared.DataAccess
{
    public class DataContext : DbContext
    {

        public DataContext() : base("SmartTable")
        {
            Database.SetInitializer<DataContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SensorData> SensorData { get; set; }
    }
}
