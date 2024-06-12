namespace Microsoft.Extensions.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using XUnitOtel.MonitoringFramework;

public static class Extensions
{
    public static IHostApplicationBuilder AddServiceDefaults(
        this IHostApplicationBuilder builder,
        string testRunId
    )
    {
        builder.ConfigureOpenTelemetry(testRunId);

        return builder;
    }

    public static IHostApplicationBuilder ConfigureOpenTelemetry(
        this IHostApplicationBuilder builder,
        string testRunId
    )
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<TestMetrics>();

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
                metrics
                    .AddMeter(TestMetrics.MeterName)
                    .AddProcessInstrumentation()
                    .AddRuntimeInstrumentation()
            )
            .WithTracing(tracing =>
            {
                tracing.SetSampler(new AlwaysOnSampler());

                tracing
                    .AddSource(BaseFixture.TracerName)
                    .AddProcessor(new TestRunSpanProcessor(testRunId));

                ConfigureSeparateTestRunsIfNeeded(testRunId, tracing);
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static void ConfigureSeparateTestRunsIfNeeded(
        string testRunId,
        TracerProviderBuilder tracing
    )
    {
        var separateTestsRunsArgument = Environment.GetEnvironmentVariable(
            Constants.Monitoring.SeparateTestRuns
        );

        _ = bool.TryParse(separateTestsRunsArgument, out var separateTestsRuns);
        if (separateTestsRuns)
        {
            tracing.SetResourceBuilder(
                ResourceBuilder
                    .CreateDefault()
                    .AddService(BaseFixture.TracerName, serviceInstanceId: testRunId)
            );
        }
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(
        this IHostApplicationBuilder builder
    )
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration[Constants.Otel.ExporterEndpoint]
        );

        if (useOtlpExporter)
        {
            builder.Services.Configure<OpenTelemetryLoggerOptions>(logging =>
                logging.AddOtlpExporter()
            );
            builder.Services.ConfigureOpenTelemetryMeterProvider(metrics =>
                metrics.AddOtlpExporter()
            );
            builder.Services.ConfigureOpenTelemetryTracerProvider(tracing =>
                tracing.AddOtlpExporter()
            );
        }
        return builder;
    }
}
