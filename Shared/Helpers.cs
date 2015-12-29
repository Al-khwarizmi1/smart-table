using System;
using System.Linq;
using System.Reflection;

namespace Shared
{
    public static class Helpers
    {
        public static string GetPropertyValues(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var t = obj.GetType();

            var values = String.Join(",", t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .ToList()
                .Select(x => x.Name + "-" + x.GetValue(obj)));

            return values;
        }

        public static string ShortDateTime(this DateTime date)
        {
            return date.ToString("yyyy-MM-dd HH:mm");
        }
    }
}
