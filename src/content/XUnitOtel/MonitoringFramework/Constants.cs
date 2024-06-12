namespace XUnitOtel.MonitoringFramework;

public static class Constants
{
    public static class Aspire
    {
        public const string DotnetDashboardUnsecuredAllowAnonymous =
            "DOTNET_DASHBOARD_UNSECURED_ALLOW_ANONYMOUS";
    }

    public static class Monitoring
    {
        public const string TraceWarmup = "XUNIT_OTEL_TRACE_WARMUP";
        public const string SeparateTestRuns = "XUNIT_OTEL_SEPARATE_TEST_RUNS";
    }

    public static class Otel
    {
        public const string ExporterEndpoint = "OTEL_EXPORTER_OTLP_ENDPOINT";
        public const string ServiceName = "OTEL_SERVICE_NAME";
    }
}
