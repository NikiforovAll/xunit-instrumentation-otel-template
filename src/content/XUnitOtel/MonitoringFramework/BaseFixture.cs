namespace XUnitOtel.MonitoringFramework;

using System.Diagnostics;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;

public class BaseFixture : IAsyncLifetime
{
    public static ActivitySource ActivitySource { get; } = new(TracerName);
    public const string TracerName = "tests";
    public static Activity ActivityForTestRun { get; private set; } = default!;
    public static TestMetrics Metrics { get; private set; } = default!;
    public static TimeProvider TimeProvider { get; private set; } = default!;

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

    public IHost HostApp { get; private set; } = default!;

    private Task hostStartupTask = default!;

    public async Task InitializeAsync()
    {
        await BootstrapAsync();

        ActivityForTestRun = ActivitySource.StartActivity("TestRun")!;
    }

    private async Task BootstrapAsync()
    {
        var includeWarmupTraceArgument = Environment.GetEnvironmentVariable(
            Constants.Monitoring.TraceWarmup
        );

        _ = bool.TryParse(includeWarmupTraceArgument, out var includeWarmupTrace);

        TracerProvider? warmupTracerProvider = default;

        if (includeWarmupTrace)
        {
            warmupTracerProvider = Sdk.CreateTracerProviderBuilder().AddSource(TracerName).Build();
        }

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
                [Constants.Otel.ExporterEndpoint] =
                    $"http://localhost:{aspireDashboard.GetMappedPublicPort(18889)}",
                [Constants.Otel.ServiceName] = "test-host",
            }
        );
        var testRunId = DateTime.UtcNow.ToString("yyyy_MM_dd_HH_mm_ss");

        builder.AddServiceDefaults(testRunId);

        HostApp = builder.Build();

        Metrics = HostApp.Services.GetRequiredService<TestMetrics>();
        TimeProvider = HostApp.Services.GetRequiredService<TimeProvider>();

        hostStartupTask = HostApp.RunAsync();

        warmupTracerProvider?.Dispose();
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

    public async Task Runner(Func<Task> action)
    {
        var activity = Activity.Current;
        try
        {
            var task = action?.Invoke();

            if (task is not null)
            {
                await task;
            }
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
