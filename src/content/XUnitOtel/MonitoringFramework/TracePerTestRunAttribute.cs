namespace XUnitOtel.MonitoringFramework;

using System.Diagnostics;
using System.Reflection;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
public sealed class TracePerTestRunAttribute : BaseTraceTestAttribute
{
    private Activity? activityForThisTest;
    private long startTime;

    public override void Before(MethodInfo methodUnderTest)
    {
        ArgumentNullException.ThrowIfNull(methodUnderTest);

        activityForThisTest = BaseFixture.ActivitySource.StartActivity(
            methodUnderTest.Name,
            ActivityKind.Internal,
            BaseFixture.ActivityForTestRun.Context
        );

        startTime = BaseFixture.TimeProvider.GetTimestamp();

        base.Before(methodUnderTest);
    }

    public override void After(MethodInfo methodUnderTest)
    {
        activityForThisTest?.Stop();

        var elapsed = BaseFixture.TimeProvider.GetElapsedTime(startTime);
        BaseFixture.Metrics.LogExecutionTime(
            methodUnderTest.Name,
            methodUnderTest.DeclaringType?.Name,
            elapsed
        );

        base.After(methodUnderTest);
    }
}
