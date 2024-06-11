namespace XUnitOtel;

using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;

public class BaseFixture : IAsyncLifetime
{
    public static ActivitySource ActivitySource { get; } = new(TracerName);
    public const string TracerName = "tests";
    public static Activity ActivityForTestRun { get; private set; } = default!;

    private readonly IContainer aspireDashboard = new ContainerBuilder()
        .WithImage("mcr.microsoft.com/dotnet/aspire-dashboard:8.0.0")
        .WithPortBinding(18888, 18888)
        .WithPortBinding(18889, true)
        .WithEnvironment("DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS", "true")
        .WithWaitStrategy(
            Wait.ForUnixContainer().UntilHttpRequestIsSucceeded(r => r.ForPort(18888))
        )
        .WithReuse(true)
        .WithLabel("aspire-dashboard", "aspire-dashboard-reuse-id")
        .Build();

    public IHost HostApp { get; private set; }

    private Task hostStartupTask;

    public async Task InitializeAsync()
    {
        await BootstrapAsync();

        ActivityForTestRun = ActivitySource.StartActivity("TestRun")!;
    }

    private async Task BootstrapAsync()
    {
        var builder = Host.CreateApplicationBuilder(
            new HostApplicationBuilderSettings()
            {
                EnvironmentName = "Test",
                ApplicationName = "XUnitOtel",
            }
        );

        using var activityForWarmup = ActivitySource.StartActivity("Warmup")!;

        await aspireDashboard.StartAsync();
        activityForWarmup?.AddEvent(new ActivityEvent("AspireDashboard Started."));

        builder.Configuration.AddInMemoryCollection(
            new Dictionary<string, string?>
            {
                ["OTEL_EXPORTER_OTLP_ENDPOINT"] =
                    $"http://localhost:{aspireDashboard.GetMappedPublicPort(18889)}",
                ["OTEL_SERVICE_NAME"] = "test-host",
            }
        );
        // get current date and format it as "yearmonthdayhoursminutesseconds"
        var testRunId = DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss");

        builder.AddServiceDefaults(testRunId);

        HostApp = builder.Build();

        hostStartupTask = HostApp.RunAsync();
    }

    public async Task DisposeAsync()
    {
        ActivityForTestRun?.Stop();

        await Task.WhenAll(hostStartupTask, HostApp.StopAsync());
    }
}

[CollectionDefinition(nameof(BaseCollection))]
public sealed class BaseCollection : ICollectionFixture<BaseFixture>;

[Collection(nameof(BaseCollection))]
public abstract class BaseContext(BaseFixture fixture)
{
    public IHost Host { get; set; } = fixture.HostApp;

    public void Runner(Action action)
    {
        var activity = Activity.Current;
        try
        {
            action?.Invoke();
        }
        catch (Exception ex)
        {
            activity?.RecordException(ex);
            activity?.SetStatus(ActivityStatusCode.Error);
            throw;
        }

        activity?.SetStatus(ActivityStatusCode.Ok);
    }
}
