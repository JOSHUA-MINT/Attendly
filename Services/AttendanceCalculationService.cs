using System;
using System.Collections.Generic;
using System.Linq;
using Attendly.ViewModels;

namespace Attendly.Services;

public interface IAttendanceCalculationService
{
    double CalculateAttendancePercentage(int presentCount, int totalConducted);
    int CalculateClassesNeededForTarget(int presentCount, int totalConducted, int targetPercentage);
    int CalculateSafeAbsences(int presentCount, int totalConducted, int targetPercentage);
    (double projected, bool isSafe) SimulateAttendance(int presentCount, int totalConducted, int targetPercentage, int additionalLectures, int absences);
    string GetAttendanceStatus(double percentage, int targetPercentage);
    List<SimulationResult> SimulateMultiple(int presentCount, int totalConducted, int targetPercentage, int maxSimulations);
}

public class AttendanceCalculationService : IAttendanceCalculationService
{
    public double CalculateAttendancePercentage(int presentCount, int totalConducted)
    {
        if (totalConducted <= 0) return 0.0;
        return Math.Round((double)presentCount / totalConducted * 100.0, 1);
    }

    public int CalculateClassesNeededForTarget(int presentCount, int totalConducted, int targetPercentage)
    {
        if (totalConducted <= 0 || targetPercentage <= 0) return 0;
        if (targetPercentage >= 100) return (presentCount < totalConducted) ? int.MaxValue : 0;
        
        double current = (double)presentCount / totalConducted * 100.0;
        if (current >= targetPercentage) return 0;

        // Formula: (target% * Total - 100 * Present) / (100 - target%)
        double numerator = (targetPercentage * totalConducted) - (100.0 * presentCount);
        double denominator = 100.0 - targetPercentage;
        int n = (int)Math.Ceiling(numerator / denominator);
        return Math.Max(0, n);
    }

    public int CalculateSafeAbsences(int presentCount, int totalConducted, int targetPercentage)
    {
        if (totalConducted <= 0 || presentCount <= 0 || targetPercentage <= 0) return 0;

        // Formula: floor( (Present * 100 / target%) - Total )
        double maxTotalAllowed = (presentCount * 100.0) / targetPercentage;
        double maxAbsences = maxTotalAllowed - totalConducted;
        return Math.Max(0, (int)Math.Floor(maxAbsences));
    }

    public (double projected, bool isSafe) SimulateAttendance(
        int presentCount, int totalConducted, int targetPercentage,
        int additionalLectures, int absences)
    {
        int newTotal = totalConducted + additionalLectures;
        int newPresent = presentCount + (additionalLectures - absences);

        if (newTotal <= 0) return (0.0, true);
        double projected = Math.Round((double)newPresent / newTotal * 100.0, 1);
        bool isSafe = projected >= targetPercentage;
        return (projected, isSafe);
    }

    public string GetAttendanceStatus(double percentage, int targetPercentage)
    {
        if (percentage >= targetPercentage) return "SAFE";
        if (percentage >= targetPercentage - 5) return "WARNING";
        return "CRITICAL";
    }

    public List<SimulationResult> SimulateMultiple(
        int presentCount, int totalConducted, int targetPercentage, int maxSimulations = 10)
    {
        var results = new List<SimulationResult>();
        for (int i = 0; i <= maxSimulations; i++)
        {
            var (projected, isSafe) = SimulateAttendance(presentCount, totalConducted, targetPercentage, 10, i);
            results.Add(new SimulationResult
            {
                MissedLectures = i,
                ProjectedAttendance = projected,
                IsSafe = isSafe,
                Message = i switch
                {
                    0 => $"Attending all 10 → {projected}%",
                    1 => $"Missing 1 of 10 → {projected}%",
                    _ => $"Missing {i} of 10 → {projected}%"
                }
            });
        }
        return results;
    }
}
