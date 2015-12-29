using System;

namespace Shared.Entities
{
    public class SensorData
    {
        public int Id { get; set; }
        public DateTime DateTime { get; set; }
        public int Height { get; set; }
        public int IntervalLength { get; set; }
    }
}
