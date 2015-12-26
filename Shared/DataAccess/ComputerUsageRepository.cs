using System.Linq;
using Shared.Entities;

namespace Shared.DataAccess
{
    public class ComputerUsageRepository
    {
        DataContext _context;

        public ComputerUsageRepository()
        {
            _context = new DataContext();
        }

        public ComputerUsageData LastEntrie()
        {
            return _context.ComputerUsageData
                .OrderByDescending(x => x.DateTime)
                .Take(1)
                .FirstOrDefault();
        }

        public void Add(ComputerUsageData data)
        {
            _context.ComputerUsageData.Add(data);
            _context.SaveChanges();
        }
    }
}
