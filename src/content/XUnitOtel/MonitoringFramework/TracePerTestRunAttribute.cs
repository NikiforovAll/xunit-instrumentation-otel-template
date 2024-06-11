namespace XUnitOtel.MonitoringFramework;

using System.Diagnostics;
using System.Reflection;
using XUnitOtel;

[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Method,
    AllowMultiple = false,
    Inherited = true
)]
public sealed class TracePerTestRunAttribute : BaseTraceTestAttribute
{
    private Activity? activityForThisTest;

    public override void Before(MethodInfo methodUnderTest)
    {
        ArgumentNullException.ThrowIfNull(methodUnderTest);

        activityForThisTest = BaseFixture.ActivitySource.StartActivity(
            methodUnderTest.Name,
            ActivityKind.Internal,
            BaseFixture.ActivityForTestRun.Context
        );

        base.Before(methodUnderTest);
    }

    public override void After(MethodInfo methodUnderTest)
    {
        activityForThisTest?.Stop();
        base.After(methodUnderTest);
    }
}
