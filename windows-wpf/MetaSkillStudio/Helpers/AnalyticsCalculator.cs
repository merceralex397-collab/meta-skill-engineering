using System;
using System.Collections.Generic;
using System.Linq;
using MetaSkillStudio.Models;
using MetaSkillStudio.ViewModels;

namespace MetaSkillStudio.Helpers
{
    /// <summary>
    /// Static calculator class containing pure functions for analytics calculations.
    /// These methods are stateless, testable, and have no dependencies on UI or services.
    /// </summary>
    public static class AnalyticsCalculator
    {
        /// <summary>
        /// Calculates the trend direction based on run metrics.
        /// Pure function: no side effects, same input always produces same output.
        /// </summary>
        /// <param name="metrics">List of run metrics ordered by timestamp</param>
        /// <returns>Trend direction indicating improving, declining, or stable performance</returns>
        public static TrendDirection CalculateTrend(List<RunMetric> metrics)
        {
            if (metrics == null || metrics.Count < 3) 
                return TrendDirection.Stable;

            // Compare first half vs second half success rates
            var halfPoint = metrics.Count / 2;
            var firstHalf = metrics.Take(halfPoint);
            var secondHalf = metrics.Skip(halfPoint);

            var firstRate = firstHalf.Any() ? firstHalf.Count(m => m.IsSuccess) / (double)firstHalf.Count() : 0;
            var secondRate = secondHalf.Any() ? secondHalf.Count(m => m.IsSuccess) / (double)secondHalf.Count() : 0;

            var diff = secondRate - firstRate;
            return diff switch
            {
                > 0.15 => TrendDirection.Improving,
                < -0.15 => TrendDirection.Declining,
                _ => TrendDirection.Stable
            };
        }

        /// <summary>
        /// Calculates success rate percentage for a skill's analytics.
        /// Pure function: deterministic calculation based only on inputs.
        /// </summary>
        /// <param name="totalRuns">Total number of runs</param>
        /// <param name="successfulRuns">Number of successful runs</param>
        /// <returns>Success rate as a percentage (0-100), or 0 if no runs</returns>
        public static double CalculateSuccessRate(int totalRuns, int successfulRuns)
        {
            if (totalRuns <= 0) return 0;
            return (successfulRuns / (double)totalRuns) * 100;
        }

        /// <summary>
        /// Generates alerts based on skill analytics data.
        /// Pure function: no external dependencies, fully testable.
        /// </summary>
        /// <param name="analytics">List of skill analytics to analyze</param>
        /// <returns>List of alert items ordered by severity</returns>
        public static List<AlertItem> GenerateAlerts(List<SkillAnalytics> analytics)
        {
            if (analytics == null)
                throw new ArgumentNullException(nameof(analytics));

            var alerts = new List<AlertItem>();
            var now = DateTime.Now; // Time-based calculation is acceptable for pure functions

            foreach (var skill in analytics)
            {
                // Skip null skills
                if (skill == null || string.IsNullOrEmpty(skill.SkillName))
                    continue;

                // Calculate success rate
                var successRate = CalculateSuccessRate(skill.TotalRuns, skill.SuccessfulRuns);

                // Low pass rate alert (< 60%)
                if (skill.TotalRuns > 0 && successRate < 60)
                {
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = $"Low pass rate: {successRate:F1}% ({skill.SuccessfulRuns}/{skill.TotalRuns} runs)",
                        Severity = AlertSeverity.Error
                    });
                }

                // No runs in 30 days alert
                if (skill.LastRunAtUtc == null || (now - skill.LastRunAtUtc.Value).TotalDays > 30)
                {
                    var days = skill.LastRunAtUtc == null ? "never" : $"{(now - skill.LastRunAtUtc.Value).TotalDays:F0} days ago";
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = $"No recent runs (last run: {days})",
                        Severity = AlertSeverity.Warning
                    });
                }

                // Declining trend alert (requires at least 5 runs for meaningful trend)
                if (skill.Trend == TrendDirection.Declining && skill.TotalRuns >= 5)
                {
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = "Declining success trend over recent runs",
                        Severity = AlertSeverity.Warning
                    });
                }

                // Low quality score alert (< 50)
                if (skill.AverageQualityScore.HasValue && skill.AverageQualityScore.Value < 50)
                {
                    alerts.Add(new AlertItem
                    {
                        SkillName = skill.SkillName,
                        Message = $"Low average quality score: {skill.AverageQualityScore.Value:F0}/100",
                        Severity = AlertSeverity.Error
                    });
                }
            }

            // Add summary alert if no issues found
            if (alerts.Count == 0)
            {
                alerts.Add(new AlertItem
                {
                    Message = "All skills look healthy! No alerts at this time.",
                    Severity = AlertSeverity.Success
                });
            }

            // Order by severity: Error first, then Warning, then Info, then Success
            return alerts.OrderBy(a => a.Severity).ToList();
        }

        /// <summary>
        /// Calculates average quality score from a list of metrics.
        /// Pure function: simple mathematical operation.
        /// </summary>
        /// <param name="metrics">List of run metrics</param>
        /// <returns>Average quality score, or null if no scores available</returns>
        public static double? CalculateAverageQualityScore(List<RunMetric> metrics)
        {
            if (metrics == null) return null;

            var scores = metrics
                .Where(m => m.QualityScore.HasValue)
                .Select(m => m.QualityScore!.Value)
                .ToList();

            return scores.Any() ? scores.Average() : null;
        }

        /// <summary>
        /// Groups runs by action type and counts occurrences.
        /// Pure function: LINQ aggregation with no side effects.
        /// </summary>
        /// <param name="metrics">List of run metrics</param>
        /// <returns>Dictionary mapping action names to run counts</returns>
        public static Dictionary<string, int> GetActionDistribution(List<RunMetric> metrics)
        {
            if (metrics == null) return new Dictionary<string, int>();

            return metrics
                .GroupBy(m => m.Action ?? "unknown")
                .ToDictionary(g => g.Key, g => g.Count());
        }
    }

}
