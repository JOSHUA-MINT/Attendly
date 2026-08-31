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
 if (totalConducted == 0) return 100.0;
 return Math.Round((double)presentCount / totalConducted * 100, 1);
 }

 public int CalculateClassesNeededForTarget(int presentCount, int totalConducted, int targetPercentage)
 {
 if (totalConducted == 0) return 0;
 double current = (double)presentCount / totalConducted * 100;
 if (current >= targetPercentage) return 0;

 // Need to find N such that: presentCount / (totalConducted + N) * 100 >= target
 // presentCount >= target * (totalConducted + N) / 100
 // presentCount * 100 >= target * totalConducted + target * N
 // N <= (presentCount * 100 - target * totalConducted) / target
 // N = ceil(...)
 double numerator = presentCount * 100.0 - targetPercentage * totalConducted;
 if (numerator <= 0) return 0;
 int n = (int)Math.Ceiling(numerator / targetPercentage);

 // Verify
 double check = (double)presentCount / (totalConducted + n) * 100;
 if (check < targetPercentage) n++;
 return Math.Max(0, n);
 }

 public int CalculateSafeAbsences(int presentCount, int totalConducted, int targetPercentage)
 {
 // Find max X such that: presentCount / (totalConducted + X) * 100 >= target
 // presentCount * 100 >= target * (totalConducted + X)
 // X <= (presentCount * 100 / target) - totalConducted
 double numerator = presentCount * 100.0 / targetPercentage;
 double maxAbsences = numerator - totalConducted;
 return Math.Max(0, (int)Math.Floor(maxAbsences));
 }

 public (double projected, bool isSafe) SimulateAttendance(
 int presentCount, int totalConducted, int targetPercentage,
 int additionalLectures, int absences)
 {
 int newTotal = totalConducted + additionalLectures;
 int newPresent = presentCount + (additionalLectures - absences);

 if (newTotal == 0) return (100.0, true);
 double projected = Math.Round((double)newPresent / newTotal * 100, 1);
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
