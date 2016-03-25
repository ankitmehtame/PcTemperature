using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PcTemperature.Helpers
{
    public static class EnumerableExtensions
    {
        public static T MaxOrDefault<T>(this IEnumerable<T> values)
        {
            var valuesArray = values.ToArray();
            return valuesArray.Any() ? valuesArray.Max() : default(T);
        }
    }
}
