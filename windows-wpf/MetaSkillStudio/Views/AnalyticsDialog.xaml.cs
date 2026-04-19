using System;
using System.Linq;
using System.Windows;
using MetaSkillStudio.ViewModels;
using ScottPlot;
using ScottPlot.WPF;

namespace MetaSkillStudio.Views
{
    /// <summary>
    /// Analytics Dashboard with ScottPlot charts and skill health metrics.
    /// Uses dependency injection for ViewModel.
    /// </summary>
    public partial class AnalyticsDialog : Window
    {
        private readonly AnalyticsViewModel _viewModel;

        /// <summary>
        /// Constructor with dependency injection.
        /// </summary>
        public AnalyticsDialog(AnalyticsViewModel viewModel)
        {
            _viewModel = viewModel ?? throw new ArgumentNullException(nameof(viewModel));
            DataContext = _viewModel;
            InitializeComponent();
            
            Loaded += AnalyticsDialog_Loaded;
        }

        // SECURITY FIX: async void event handler with try-catch to prevent application crashes
        // NEVER let exceptions escape from async void methods
        private async void AnalyticsDialog_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                await _viewModel.LoadAnalyticsAsync();
                RenderCharts();
            }
            catch (Exception ex)
            {
                // Route errors to Debug output - in production, use proper logging
                System.Diagnostics.Debug.WriteLine($"[AnalyticsDialog] Error loading analytics: {ex}");
                // Could show a user-friendly error message here
            }
        }

        private void RenderCharts()
        {
            RenderPassRateChart();
            RenderQualityScoreChart();
            RenderActivityChart();
        }

        private void RenderPassRateChart()
        {
            var plot = PassRatePlot.Plot;
            plot.Clear();

            var skills = _viewModel.SkillAnalytics.Where(s => s.TotalRuns > 0).ToList();
            if (!skills.Any()) return;

            double[] values = skills.Select(s => s.SuccessRate).ToArray();
            string[] labels = skills.Select(s => s.SkillName).ToArray();

            var bars = plot.Add.Bars(values);
            bars.Color = new ScottPlot.Color(45, 90, 240); // Primary blue
            
            // Color code based on pass rate
            var barList = bars.Bars.ToList();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < 60)
                    barList[i].FillColor = new ScottPlot.Color(211, 47, 47); // Red
                else if (values[i] < 80)
                    barList[i].FillColor = new ScottPlot.Color(255, 152, 0); // Orange
                else
                    barList[i].FillColor = new ScottPlot.Color(76, 175, 80); // Green
            }

            plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            plot.Axes.SetLimitsY(0, 100);
            plot.YLabel("Pass Rate (%)");
            plot.XLabel("Skills");
            
            PassRatePlot.Refresh();
        }

        private void RenderQualityScoreChart()
        {
            var plot = QualityScorePlot.Plot;
            plot.Clear();

            var skills = _viewModel.SkillAnalytics.Where(s => s.AverageQualityScore.HasValue).ToList();
            if (!skills.Any()) return;

            double[] values = skills.Select(s => s.AverageQualityScore!.Value).ToArray();
            string[] labels = skills.Select(s => s.SkillName).ToArray();

            var bars = plot.Add.Bars(values);
            
            // Color code based on quality score
            var qualityBarList = bars.Bars.ToList();
            for (int i = 0; i < values.Length; i++)
            {
                if (values[i] < 60)
                    qualityBarList[i].FillColor = new ScottPlot.Color(211, 47, 47); // Red
                else if (values[i] < 80)
                    qualityBarList[i].FillColor = new ScottPlot.Color(255, 152, 0); // Orange
                else
                    qualityBarList[i].FillColor = new ScottPlot.Color(76, 175, 80); // Green
            }

            plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            plot.Axes.Bottom.TickLabelStyle.Alignment = Alignment.MiddleRight;
            plot.Axes.SetLimitsY(0, 100);
            plot.YLabel("Avg Quality Score");
            plot.XLabel("Skills");
            
            QualityScorePlot.Refresh();
        }

        private void RenderActivityChart()
        {
            var plot = ActivityPlot.Plot;
            plot.Clear();

            // Aggregate runs by day for the last 30 days
            var endDate = DateTime.Now;
            var startDate = endDate.AddDays(-30);
            var dateRange = Enumerable.Range(0, 31)
                .Select(i => startDate.AddDays(i).Date)
                .ToList();

            var runsByDay = _viewModel.SkillAnalytics
                .SelectMany(s => s.RunHistory)
                .Where(r => r.TimestampUtc >= startDate)
                .GroupBy(r => r.TimestampUtc.Date)
                .ToDictionary(g => g.Key, g => g.Count());

            double[] values = dateRange.Select(d => (double)(runsByDay.TryGetValue(d, out var count) ? count : 0)).ToArray();
            double[] positions = Enumerable.Range(0, 31).Select(i => (double)i).ToArray();

            // Line chart for activity
            var scatter = plot.Add.Scatter(positions, values);
            scatter.LineWidth = 2;
            scatter.MarkerSize = 4;
            scatter.Color = new ScottPlot.Color(45, 90, 240);
            // Note: Scatter in ScottPlot 5.0.21 doesn't support FillY directly
            // Fill functionality would require Signal plot or custom polygons

            // Add trend line using simple linear regression
            if (values.Length > 1)
            {
                // Calculate simple trend line (least squares)
                double xMean = positions.Average();
                double yMean = values.Average();
                double slope = positions.Zip(values, (x, y) => (x - xMean) * (y - yMean)).Sum() / 
                               positions.Sum(x => (x - xMean) * (x - xMean));
                double intercept = yMean - slope * xMean;
                
                double x1 = positions.First();
                double x2 = positions.Last();
                double y1 = slope * x1 + intercept;
                double y2 = slope * x2 + intercept;
                var trend = plot.Add.Line(x1, y1, x2, y2);
                trend.Color = new ScottPlot.Color(255, 152, 0);
                trend.LineWidth = 2;
                trend.LinePattern = ScottPlot.LinePattern.Dashed;
            }

            // X-axis labels every 5 days
            string[] labels = dateRange.Where((d, i) => i % 5 == 0).Select(d => d.ToString("MM/dd")).ToArray();
            double[] labelPositions = positions.Where((p, i) => i % 5 == 0).ToArray();
            plot.Axes.Bottom.TickGenerator = new ScottPlot.TickGenerators.NumericManual(labelPositions, labels);
            plot.Axes.Bottom.TickLabelStyle.Rotation = 45;
            
            plot.YLabel("Number of Runs");
            plot.XLabel("Date");
            plot.Axes.SetLimitsY(0, values.Max() * 1.2);
            
            ActivityPlot.Refresh();
        }
    }
}
