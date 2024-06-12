# XUnit Tests Instrumentation with OpenTelemetry

Shows how to instrument XUnit tests with OpenTelemetry and export results to Aspire Dashboard.

## Demo

```bash
XUNIT_OTEL_SEPARATE_TEST_RUNS=false XUNIT_OTEL_TRACE_WARMUP=true dotnet test
```

## Configure

* `XUNIT_OTEL_SEPARATE_TEST_RUNS` responsible for tests separation, if true, tests are separated by `service.instance.id`
* `XUNIT_OTEL_TRACE_WARMUP` show additional "warmup" trace, useful in case we need to diagnose how much time it takes to setup dependencies.
