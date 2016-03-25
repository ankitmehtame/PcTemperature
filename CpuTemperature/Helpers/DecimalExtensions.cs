using System;
using System.Collections.Generic;
using System.Linq;

namespace CpuTemperature.Helpers
{
    public static class DecimalExtensions
    {
        public static decimal RoundValue(this decimal value, int decimalPlaces = 0)
        {
            var rounded = Math.Round(value, decimalPlaces, MidpointRounding.AwayFromZero);
            return rounded;
        }

        public static decimal AverageOrDefault(this IEnumerable<decimal> values)
        {
            var valuesArray = values.ToArray();
            return valuesArray.Any() ? valuesArray.Average() : default(decimal);
        }
    }
}
