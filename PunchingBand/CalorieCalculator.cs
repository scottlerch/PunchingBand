using PunchingBand.Models;
using System;

namespace PunchingBand
{
    public static class CalorieCalculator
    {
        // Equations for Determination of Calorie Burn if VO2max is Unknown
        // 
        // Male: ((-55.0969 + (0.6309 x HR) + (0.1988 x W) + (0.2017 x A))/4.184) x 60 x T
        // Female: ((-20.4022 + (0.4472 x HR) - (0.1263 x W) + (0.074 x A))/4.184) x 60 x T
        // where
        // 
        // HR = Heart rate (in beats/minute) 
        // W = Weight (in kilograms) 
        // A = Age (in years) 
        // T = Exercise duration time (in hours)

        public static double GetCalories(Gender gender, int heartRate, double weight, int age, TimeSpan timeRange)
        {
            if (gender == Gender.Male)
            {
                return ((-55.0969 + (0.6309 * heartRate) + (0.1988 * weight) + (0.2017 * age)) / 4.184) * 60 * timeRange.TotalHours;
            }

            if (gender == Gender.Female)
            {
                return ((-20.4022 + (0.4472 * heartRate) - (0.1263 * weight) + (0.074 * age)) / 4.184) * 60 * timeRange.TotalHours;
            }

            throw new InvalidOperationException("Unknown gender");
        }
    }
}
