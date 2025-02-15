using System;
using cAlgo.API;

namespace cAlgo.Robots;

public static class LastCrossoverTimeHelper
{
    public static DateTime? GetLastCrossoverTime(Bars bars, FourMovingAveragesWithCloud waves)
    {
        for (int i = bars.Count - 2; i >= 0; i--) // Start from the second-to-last bar
        {
            // Check for crossover
            if (CrossLong() || CrossShort())
            {
                return bars.OpenTimes[i]; // Return the time of the crossover
            }

            bool CrossLong()
            {
                return waves.FastLowMA[i + 1] > waves.SlowHighMA[i + 1] && waves.FastLowMA[i] <= waves.SlowHighMA[i];
            }

            bool CrossShort()
            {
                return waves.FastHighMA[i + 1] < waves.SlowLowMA[i + 1] && waves.FastHighMA[i] >= waves.SlowLowMA[i];
            }
        }


        return null; // Return null if no crossover is found
    }
}
