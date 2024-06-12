namespace XUnitOtel.MonitoringFramework;

using System.Diagnostics.Metrics;

/// <summary>
/// Represents a class for tracking tests metrics.
/// </summary>
public class TestMetrics
{
    public const string MeterName = "XUnitOtel.MonitoringFramework";
    private readonly Histogram<double> testExecutionTime;

    /// <summary>
    /// Initializes a new instance of the <see cref="TestMetrics"/> class.
    /// </summary>
    /// <param name="meterFactory">The meter factory used to create metrics.</param>
    public TestMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create(MeterName);
        testExecutionTime = meter.CreateHistogram<double>(
            "tests.execution.time",
            unit: "Milliseconds",
            description: "Records test execution time per-test/per-class"
        );
    }

    /// <summary>
    /// Records a failed requirement.
    /// </summary>
    /// <param name="requirement">The name of the failed requirement.</param>
    public void LogExecutionTime(string name, string? group, TimeSpan duration)
    {
        testExecutionTime.Record(
            duration.Milliseconds,
            new KeyValuePair<string, object?>("name", name),
            new KeyValuePair<string, object?>("class", group)
        );
    }
}
