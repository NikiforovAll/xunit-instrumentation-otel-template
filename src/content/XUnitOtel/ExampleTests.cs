namespace XUnitOtel;

using XUnitOtel.MonitoringFramework;

[TracePerTestRun]
public class ExampleTests(BaseFixture fixture) : BaseContext(fixture)
{
    [Fact]
    public async Task WaitRandomTime_Success()
    {
        // Given
        int waitFor = Random.Shared.Next(100, 300);
        TimeSpan delay = TimeSpan.FromMilliseconds(waitFor);

        // When
        await Task.Delay(delay);

        // Then
        Runner(() => Assert.True(true));
    }

    [Fact]
    public async Task WaitRandomTime_ProducesSubActivity_Success()
    {
        // Given
        using var myActivity = BaseFixture.ActivitySource.StartActivity("SubActivity");
        int waitFor = Random.Shared.Next(100, 300);
        TimeSpan delay = TimeSpan.FromMilliseconds(waitFor);

        // When
        await Task.Delay(delay);
        myActivity?.AddEvent(new($"WaitedForDelay"));
        myActivity?.SetTag("subA_activity:delay", waitFor);

        // Then
        Runner(() => Assert.True(true));
    }

    [Fact]
    public async Task WaitRandomTime_Fail()
    {
        // Given
        int waitFor = Random.Shared.Next(50, 100);
        TimeSpan delay = TimeSpan.FromMilliseconds(waitFor);

        // When
        await Task.Delay(delay);

        // Then

        Runner(() => Assert.True(false));
    }
}
