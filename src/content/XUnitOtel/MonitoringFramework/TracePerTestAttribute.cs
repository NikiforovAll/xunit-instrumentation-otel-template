namespace XUnitOtel.MonitoringFramework;

using System.Diagnostics;
using System.Reflection;
using XUnitOtel;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
public sealed class TracePerTestAttribute : BaseTraceTestAttribute
{
    private Activity? activityForThisTest;

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

        base.Before(methodUnderTest);
    }

    public override void After(MethodInfo methodUnderTest)
    {
        activityForThisTest?.Stop();

        base.After(methodUnderTest);
    }
}
