using System;
using System.Collections.Generic;
using System.Linq;
using Shared.Entities;

namespace Shared.DataAccess
{
    public class SensorDataRepository
    {
        private readonly DataContext _context;

        public SensorDataRepository()
        {
            _context = new DataContext();
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

        public IEnumerable<SensorData> GetNewerThen(DateTime date)
        {
            return _context.SensorData.Where(x => x.DateTime >= date);
        }

        public IEnumerable<SensorData> All()
        {
            return _context.SensorData;
        }
    }
}
