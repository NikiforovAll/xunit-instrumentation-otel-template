namespace XUnitOtel.MonitoringFramework;

using System.Diagnostics;
using System.Reflection;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
public sealed class TracePerTestAttribute : BaseTraceTestAttribute
{
    private Activity? activityForThisTest;
    private long startTime;

    public override void Before(MethodInfo methodUnderTest)
    {
        var linkToTestRunActivity =
            BaseFixture.ActivityForTestRun == null
                ? null
                : new List<ActivityLink> { new(BaseFixture.ActivityForTestRun.Context) };

        activityForThisTest = BaseFixture.ActivitySource.StartActivity(
            methodUnderTest.Name,
            ActivityKind.Internal,
            new ActivityContext(),
            links: linkToTestRunActivity
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
