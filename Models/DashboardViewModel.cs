using System.Collections.Generic;

/// <summary>
/// View model for the Process Validation Dashboard
/// </summary>
public class DashboardViewModel
{
    public string SelectedFY { get; set; }
    public string FYRangeText { get; set; }
    public List<string> FYList { get; set; } = new List<string>();

    // KPI Cards
    public int TotalPlans { get; set; }
    public int PendingSubmission { get; set; }
    public int PendingPlanVerification { get; set; }
    public int PendingPlanApproval { get; set; }
    public int CompletedPlans { get; set; }
    public int TotalValidations { get; set; }

    // Chart Data
    public List<DashboardChartRow> StatusData { get; set; } = new List<DashboardChartRow>();
    public List<DashboardChartRow> MonthlyData { get; set; } = new List<DashboardChartRow>();
    public List<DashboardChartRow> QualTypeData { get; set; } = new List<DashboardChartRow>();
    public List<DashboardChartRow> DecisionData { get; set; } = new List<DashboardChartRow>();
    public List<DashboardChartRow> TopProjectsData { get; set; } = new List<DashboardChartRow>();
    public List<DashboardChartRow> PendingActionsData { get; set; } = new List<DashboardChartRow>();
}

/// <summary>
/// Generic label/value/color row used for chart data
/// </summary>
public class DashboardChartRow
{
    public string label { get; set; }
    public decimal val { get; set; }
    public string color { get; set; }
}

/// <summary>
/// Row returned by scalar count / aggregate queries
/// </summary>
public class DashboardCountRow
{
    public decimal cnt { get; set; }
}

/// <summary>
/// Row returned by the year-month grouping queries (monthly trend)
/// </summary>
public class DashboardMonthRow
{
    public int yr { get; set; }
    public int mth { get; set; }
    public decimal val { get; set; }
}

/// <summary>
/// Row returned when loading distinct financial-year labels from the database
/// </summary>
public class DashboardFyRow
{
    public string fy { get; set; }
}