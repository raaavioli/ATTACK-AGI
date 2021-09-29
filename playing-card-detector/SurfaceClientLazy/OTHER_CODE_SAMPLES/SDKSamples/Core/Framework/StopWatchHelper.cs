using System;
using System.Diagnostics;

namespace CoreInteractionFramework
{
    /// <summary>
    /// A helper class to retrieve timestamp in 100-nanosecond units.
    /// </summary>
    public static class StopwatchHelper
    {
        private readonly static double ConversionRatio = 10000000.0 / (double)Stopwatch.Frequency;

        /// <summary>
        /// Gets the current timestamp in 100-nanosecond units.
        /// </summary>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate",
            Justification="Consistent with Stopwatch.GetTimestamp() method.")]
        public static Int64 Get100NanosecondsTimestamp()
        {
            return (Int64)(Stopwatch.GetTimestamp() * ConversionRatio);
        }

        /// <summary>
        /// Gets the elapsed time in 100-nanosecond units.
        /// </summary>
        /// <param name="stopwatch"></param>
        /// <returns></returns>
        public static Int64 Elapsed100Nanoseconds(this Stopwatch stopwatch)
        {
            return (Int64)(stopwatch.ElapsedTicks * ConversionRatio);
        }
    }
}
