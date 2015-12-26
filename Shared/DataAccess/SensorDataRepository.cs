using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Shared.Entities;

namespace Shared.DataAccess
{
    public class SensorDataContext : DbContext
    {

        public SensorDataContext() : base("SmartTable")
        {
            Database.SetInitializer<SensorDataContext>(null);
        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            base.OnModelCreating(modelBuilder);
        }

        public DbSet<SensorData> SensorData { get; set; }
    }

    public class SensorDataRepository
    {
        SensorDataContext _context;

        public SensorDataRepository()
        {
            _context = new SensorDataContext();
        }

        public SensorData LastEntrie()
        {
            return _context.SensorData
                .OrderByDescending(x => x.DateTime)
                .Take(1)
                .FirstOrDefault();
        }

        public void Add(SensorData data)
        {
            _context.SensorData.Add(data);
            _context.SaveChanges();
        }

    }
}
