namespace Microsoft.Extensions.Hosting;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using XUnitOtel;
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

        builder.Logging.AddOpenTelemetry(logging =>
        {
            logging.IncludeFormattedMessage = true;
            logging.IncludeScopes = true;
        });

        builder
            .Services.AddOpenTelemetry()
            .WithMetrics(metrics => metrics.AddProcessInstrumentation().AddRuntimeInstrumentation())
            .WithTracing(tracing =>
            {
                tracing.SetSampler(new AlwaysOnSampler());

                tracing
                    .SetResourceBuilder(
                        ResourceBuilder
                            .CreateDefault()
                            .AddService(BaseFixture.TracerName, serviceInstanceId: testRunId)
                    )
                    .AddSource(BaseFixture.TracerName)
                    .AddProcessor(new TestRunSpanProcessor(testRunId));
            });

        builder.AddOpenTelemetryExporters();

        return builder;
    }

    private static IHostApplicationBuilder AddOpenTelemetryExporters(
        this IHostApplicationBuilder builder
    )
    {
        var useOtlpExporter = !string.IsNullOrWhiteSpace(
            builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]
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
